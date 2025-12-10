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

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

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

app.UseAuthentication();
app.UseAuthorization();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();

app.Run();
