using CcsHackathon.Components;
using CcsHackathon.Data;
using CcsHackathon.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.Kiota.Authentication.Azure;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Azure.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var mvcBuilder = builder.Services.AddControllersWithViews();

// Check feature flag for dummy authentication
var useDummyAuth = builder.Configuration.GetValue<bool>("Features:UseDummyAuth", true);

if (useDummyAuth)
{
    // Dummy authentication mode - no external calls
    builder.Services.AddAuthentication("Dummy")
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, DummyAuthenticationHandler>(
            "Dummy", options => { });
    
    // Register dummy user context
    builder.Services.AddScoped<IUserContext, DummyUserContext>();
}
else
{
    // Real Azure AD authentication mode
    var azureAdSection = builder.Configuration.GetSection("AzureAd");
    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(azureAdSection)
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddInMemoryTokenCaches();

    // Configure Microsoft Graph
    builder.Services.AddHttpClient();
    builder.Services.AddScoped(serviceProvider =>
    {
        var tokenAcquisition = serviceProvider.GetRequiredService<ITokenAcquisition>();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var graphSection = builder.Configuration.GetSection("MicrosoftGraph");
        var scopes = graphSection["Scopes"]?.Split(' ') ?? new[] { "User.Read.All" };
        
        var credential = new CcsHackathon.TokenCredentialWrapper(tokenAcquisition, scopes);
        var authProvider = new AzureIdentityAuthenticationProvider(credential);
        var httpClient = httpClientFactory.CreateClient();
        
        var requestAdapter = new Microsoft.Kiota.Http.HttpClientLibrary.HttpClientRequestAdapter(authProvider, httpClient: httpClient);
        return new GraphServiceClient(requestAdapter);
    });

    mvcBuilder.AddMicrosoftIdentityUI();
    
    // Register Azure AD user context
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IUserContext, AzureAdUserContext>();
}

// Add authorization
builder.Services.AddAuthorization(options =>
{
    if (useDummyAuth)
    {
        // In dummy mode, allow unauthenticated access but treat as authenticated via handler
        options.FallbackPolicy = options.DefaultPolicy;
    }
    else
    {
        options.FallbackPolicy = options.DefaultPolicy;
    }
});

// Register services
if (!useDummyAuth)
{
    // Only register GraphService in real auth mode
    builder.Services.AddScoped<IGraphService, GraphService>();
}
else
{
    // Register a dummy GraphService for dummy mode
    builder.Services.AddScoped<IGraphService, DummyGraphService>();
}
builder.Services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IBoardGameAiService, BoardGameAiService>();
builder.Services.AddScoped<IBoardGameService, BoardGameService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IBoardGameOverviewService, BoardGameOverviewService>();
builder.Services.AddScoped<IBoardGameFaqService, BoardGameFaqService>();

// Register background service for AI data processing
builder.Services.AddHostedService<BoardGameAiBackgroundService>();

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure graceful shutdown timeout (10 seconds)
builder.Services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(10));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Ensure database is created and matches the current model
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Check if database exists
        var databaseExists = await dbContext.Database.CanConnectAsync();
        
        if (databaseExists)
        {
            // Try to query the database to check if schema is compatible
            // If the schema has changed (e.g., new columns added), queries will fail
            try
            {
                // Attempt to query each table to verify schema compatibility
                // This will fail if columns are missing or structure has changed
                await dbContext.Registrations.CountAsync();
                await dbContext.GameRegistrations.CountAsync();
                await dbContext.BoardGameCaches.CountAsync();
                await dbContext.BoardGames.CountAsync();
                await dbContext.Sessions.CountAsync();
                await dbContext.BoardGameFaqCaches.CountAsync();
                await dbContext.BoardGameConversations.CountAsync();
                await dbContext.BoardGameConversationMessages.CountAsync();
                
                // Try to access a newer column to ensure it exists
                // If FoodRequirements or AI fields don't exist, this will fail
                var testQuery = await dbContext.Registrations
                    .Select(r => new { r.Id, r.FoodRequirements, r.SessionId })
                    .FirstOrDefaultAsync();
                
                var testAiQuery = await dbContext.BoardGameCaches
                    .Select(b => new { b.Id, b.Complexity, b.TimeToSetupMinutes, b.Summary, b.HasAiData, b.BoardGameId })
                    .FirstOrDefaultAsync();
                
                var testBoardGameQuery = await dbContext.BoardGames
                    .Select(bg => new { bg.Id, bg.Name, bg.Description, bg.SetupComplexity, bg.Score })
                    .FirstOrDefaultAsync();
                
                // Check if migration from GameId to BoardGameId is needed
                await MigrateGameIdToBoardGameIdAsync(dbContext, logger);
                
                logger.LogInformation("Database schema is compatible with current model");
            }
            catch (Exception ex)
            {
                // Schema mismatch detected - delete and recreate
                logger.LogWarning(ex, "Database schema mismatch detected. Deleting and recreating database...");
                await dbContext.Database.EnsureDeletedAsync();
                await dbContext.Database.EnsureCreatedAsync();
                logger.LogInformation("Database recreated successfully with updated schema");
            }
        }
        else
        {
            // Database doesn't exist, create it
            await dbContext.Database.EnsureCreatedAsync();
            logger.LogInformation("Database created successfully");
        }
    }
    catch (Exception ex)
    {
        // If anything goes wrong, try to delete and recreate
        logger.LogError(ex, "Error during database initialization. Attempting to recreate database...");
        try
        {
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
            logger.LogInformation("Database recreated successfully after error");
        }
        catch (Exception recreateEx)
        {
            logger.LogError(recreateEx, "Failed to recreate database");
            throw;
        }
    }
}

// Data migration: Ensure BoardGames exist for all GameRegistrations
// This handles the case where the database was recreated but GameRegistrations reference BoardGames
async Task MigrateGameIdToBoardGameIdAsync(ApplicationDbContext dbContext, ILogger logger)
{
    try
    {
        // Get all GameRegistrations that might not have a valid BoardGame reference
        var gameRegistrationsWithoutBoardGame = await dbContext.GameRegistrations
            .Where(gr => !dbContext.BoardGames.Any(bg => bg.Id == gr.BoardGameId))
            .ToListAsync();

        if (gameRegistrationsWithoutBoardGame.Any())
        {
            logger.LogWarning("Found {Count} GameRegistrations with invalid BoardGameId references. These will need to be fixed manually or the database should be recreated.", 
                gameRegistrationsWithoutBoardGame.Count);
        }
    }
    catch (Exception ex)
    {
        // Migration check is optional
        logger.LogDebug(ex, "Migration check encountered an issue (this is expected during database recreation)");
    }
}

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();

app.Run();
