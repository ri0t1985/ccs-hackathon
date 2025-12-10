using CcsHackathon.Components;
using CcsHackathon.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
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

// Configure Azure AD authentication
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

// Add authorization
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

// Register services
builder.Services.AddScoped<IGraphService, GraphService>();
builder.Services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();

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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();

app.Run();
