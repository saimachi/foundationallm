using FoundationaLLM;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Validation;

var builder = WebApplication.CreateBuilder(args);

DefaultAuthentication.Production = builder.Environment.IsProduction();

builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", false, true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddAzureKeyVault(
    new Uri(Environment.GetEnvironmentVariable(EnvironmentVariables.FoundationaLLM_AuthorizationAPI_KeyVaultURI)!),
    DefaultAuthentication.GetAzureCredential());
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

// Add services to the container.
builder.AddAzureKeyVaultService(
    EnvironmentVariables.FoundationaLLM_AuthorizationAPI_KeyVaultURI);

// Resource validation.
builder.Services.AddSingleton<IResourceValidatorFactory, ResourceValidatorFactory>();

// Authorization core.
builder.AddAuthorizationCore();

// CORS policies
builder.AddCorsPolicies();

// Add authentication configuration.
builder.AddAuthenticationConfiguration(
    KeyVaultSecretNames.FoundationaLLM_AuthorizationAPI_Entra_Instance,
    KeyVaultSecretNames.FoundationaLLM_AuthorizationAPI_Entra_TenantId,
    KeyVaultSecretNames.FoundationaLLM_AuthorizationAPI_Entra_ClientId,
    KeyVaultSecretNames.FoundationaLLM_AuthorizationAPI_Entra_Scopes);

// Add OpenTelemetry.
builder.AddOpenTelemetry(
    KeyVaultSecretNames.FoundationaLLM_AuthorizationAPI_AppInsights_ConnectionString,
    ServiceNames.AuthorizationAPI);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Set the CORS policy before other middleware.
app.UseCors(CorsPolicyNames.AllowAllOrigins);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler(exceptionHandlerApp
    => exceptionHandlerApp.Run(async context
        => await Results.Problem().ExecuteAsync(context)));

app.MapControllers();

app.Run();
