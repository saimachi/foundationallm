using Asp.Versioning;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Middleware;
using FoundationaLLM.Common.Models.Configuration.Branding;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.OpenAPI;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Services.API;
using FoundationaLLM.Common.Services.Azure;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Common.Validation;
using FoundationaLLM.Management.Interfaces;
using FoundationaLLM.Management.Models.Configuration;
using FoundationaLLM.Management.Services;
using FoundationaLLM.Management.Services.APIServices;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Polly;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FoundationaLLM.Management.API
{
    /// <summary>
    /// Main entry point for the Management API.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Management API service configuration.
        /// </summary>
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            DefaultAuthentication.Production = builder.Environment.IsProduction();

            builder.Configuration.Sources.Clear();
            builder.Configuration.AddJsonFile("appsettings.json", false, true);
            builder.Configuration.AddEnvironmentVariables();
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                options.Connect(builder.Configuration[EnvironmentVariables.FoundationaLLM_AppConfig_ConnectionString]);
                options.ConfigureKeyVault(options => { options.SetCredential(DefaultAuthentication.GetAzureCredential()); });
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Instance);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIs);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_CosmosDB);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Branding);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_ManagementAPI_Entra);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Vectorization);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Agent);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Prompt);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_DataSource);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Events);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Configuration);
            });

            if (builder.Environment.IsDevelopment())
                builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

            builder.AddGroupMembership();
            builder.AddAuthorizationService();

            // CORS policies
            builder.AddCorsPolicies();

            builder.Services.AddOptions<CosmosDbSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_CosmosDB));
            builder.Services.AddOptions<ClientBrandingConfiguration>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Branding));
            builder.Services.AddOptions<AppConfigurationSettings>()
                .Configure(o =>
                    o.ConnectionString = builder.Configuration[EnvironmentVariables.FoundationaLLM_AppConfig_ConnectionString]!);

            builder.Services.AddInstanceProperties(builder.Configuration);

            // Add Azure ARM services
            builder.Services.AddAzureResourceManager();

            // Add event services
            builder.Services.AddAzureEventGridEvents(
                builder.Configuration,
                AppConfigurationKeySections.FoundationaLLM_Events_AzureEventGridEventService_Profiles_ManagementAPI);

            builder.Services.AddScoped<IAgentFactoryAPIService, AgentFactoryAPIService>();
            builder.Services.AddScoped<IAgentHubAPIService, AgentHubAPIService>();
            builder.Services.AddScoped<IDataSourceHubAPIService, DataSourceHubAPIService>();
            builder.Services.AddScoped<IPromptHubAPIService, PromptHubAPIService>();
            builder.Services.AddScoped<IConfigurationManagementService, ConfigurationManagementService>();
            builder.Services.AddScoped<ICacheManagementService, CacheManagementService>();

            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            builder.Services.AddScoped<ICallContext, CallContext>();
            builder.Services.AddScoped<IHttpClientFactoryService, HttpClientFactoryService>();

            // Add event services.
            builder.Services.AddAzureEventGridEvents(
                builder.Configuration,
                AppConfigurationKeySections.FoundationaLLM_Events_AzureEventGridEventService);

            // Resource validation.
            builder.Services.AddSingleton<IResourceValidatorFactory, ResourceValidatorFactory>();

            //----------------------------
            // Resource providers
            //----------------------------
            builder.AddAuthorizationResourceProvider();
            builder.AddConfigurationResourceProvider();
            builder.AddVectorizationResourceProvider();
            builder.AddAgentResourceProvider();
            builder.AddPromptResourceProvider();
            builder.AddDataSourceResourceProvider();

            // Add authentication configuration.
            builder.AddAuthenticationConfiguration(
                AppConfigurationKeys.FoundationaLLM_ManagementAPI_Entra_Instance,
                AppConfigurationKeys.FoundationaLLM_ManagementAPI_Entra_TenantId,
                AppConfigurationKeys.FoundationaLLM_ManagementAPI_Entra_ClientId,
                AppConfigurationKeys.FoundationaLLM_ManagementAPI_Entra_Scopes);

            // Add OpenTelemetry.
            builder.AddOpenTelemetry(
                AppConfigurationKeys.FoundationaLLM_APIs_ManagementAPI_AppInsightsConnectionString,
                ServiceNames.ManagementAPI);

            // Register the downstream services and HTTP clients.
            RegisterDownstreamServices(builder);

            builder.Services.AddControllers();
            builder.Services.AddProblemDetails();
            builder.Services
                .AddApiVersioning(options =>
                {
                    // Reporting api versions will return the headers
                    // "api-supported-versions" and "api-deprecated-versions"
                    options.ReportApiVersions = true;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = new ApiVersion(new DateOnly(2024, 2, 16));
                })
                .AddMvc()
                .AddApiExplorer();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(
                options =>
                {
                    // Add a custom operation filter which sets default values
                    options.OperationFilter<SwaggerDefaultValues>();

                    var fileName = typeof(Program).Assembly.GetName().Name + ".xml";
                    var filePath = Path.Combine(AppContext.BaseDirectory, fileName);

                    // Integrate xml comments
                    options.IncludeXmlComments(filePath);

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Id = "azure_auth",
                                    Type = ReferenceType.SecurityScheme
                                }
                            },
                            new[] {"user_impersonation"}
                        }
                    });                    

                    options.AddSecurityDefinition("azure_auth", new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Azure Active Directory Oauth2 Flow",
                        Name = "azure_auth",
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            Implicit = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri("https://login.microsoftonline.com/common/oauth2/authorize"),
                                Scopes = new Dictionary<string, string>
                                {
                                    {
                                        "user_impersonation",
                                        "impersonate your user account"
                                    }
                                }
                            }
                        },
                        BearerFormat = "JWT",
                        Scheme = "bearer"
                    });
                })
                .AddSwaggerGenNewtonsoftSupport();

            var app = builder.Build();

            // Set the CORS policy before other middleware.
            app.UseCors(CorsPolicyNames.AllowAllOrigins);

            // For the CoreAPI, we need to make sure that UseAuthentication is called before the UserIdentityMiddleware.
            app.UseAuthentication();
            app.UseAuthorization();

            // Register the middleware to extract the user identity context and other HTTP request context data required by the downstream services.
            app.UseMiddleware<CallContextMiddleware>();

            app.UseExceptionHandler(exceptionHandlerApp
                => exceptionHandlerApp.Run(async context
                    => await Results.Problem().ExecuteAsync(context)));

            // Configure the HTTP request pipeline.
            app.UseSwagger(p => p.SerializeAsV2 = true);
            app.UseSwaggerUI(
                options =>
                {
                    var descriptions = app.DescribeApiVersions();

                    // build a swagger endpoint for each discovered API version
                    foreach (var description in descriptions)
                    {
                        var url = $"/swagger/{description.GroupName}/swagger.json";
                        var name = description.GroupName.ToUpperInvariant();
                        options.SwaggerEndpoint(url, name);
                    }

                    options.OAuthAdditionalQueryStringParams(new Dictionary<string, string>() { { "resource", builder.Configuration[AppConfigurationKeys.FoundationaLLM_Management_Entra_ClientId] } });
                });

            app.UseHttpsRedirection();
            app.MapControllers();

            app.Run();
        }

        /// <summary>
        /// Bind the downstream API settings to the configuration and register the HTTP clients.
        /// The AddResilienceHandler extension method is used to add the standard Polly resilience
        /// strategies to the HTTP clients.
        /// </summary>
        /// <param name="builder"></param>
        private static void RegisterDownstreamServices(WebApplicationBuilder builder)
        {
            var downstreamAPISettings = new DownstreamAPISettings
            {
                DownstreamAPIs = []
            };
            var retryOptions = CommonHttpRetryStrategyOptions.GetCommonHttpRetryStrategyOptions();

            // AgentFactoryAPI:
            var agentFactoryAPISettings = new DownstreamAPIKeySettings
            {
                APIUrl = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_AgentFactoryAPI_APIUrl]!,
                APIKey = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_AgentFactoryAPI_APIKey]!
            };
            downstreamAPISettings.DownstreamAPIs[HttpClients.AgentFactoryAPI] = agentFactoryAPISettings;

            builder.Services
                .AddHttpClient(HttpClients.AgentFactoryAPI,
                    client => { client.BaseAddress = new Uri(agentFactoryAPISettings.APIUrl); })
                .AddResilienceHandler(
                    "DownstreamPipeline",
                    strategyBuilder =>
                    {
                        strategyBuilder.AddRetry(retryOptions);
                    });

            // AgentHubAPI:
            var agentHubAPISettings = new DownstreamAPIKeySettings
            {
                APIUrl = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_AgentHubAPI_APIUrl]!,
                APIKey = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_AgentHubAPI_APIKey]!
            };
            downstreamAPISettings.DownstreamAPIs[HttpClients.AgentHubAPI] = agentHubAPISettings;

            builder.Services
                    .AddHttpClient(HttpClients.AgentHubAPI,
                        client => { client.BaseAddress = new Uri(agentHubAPISettings.APIUrl); })
                    .AddResilienceHandler(
                        "DownstreamPipeline",
                        strategyBuilder =>
                        {
                            strategyBuilder.AddRetry(retryOptions);
                        });

            // DataSourceHubAPI:
            var dataSourceHubAPISettings = new DownstreamAPIKeySettings
            {
                APIUrl = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_DataSourceHubAPI_APIUrl]!,
                APIKey = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_DataSourceHubAPI_APIKey]!
            };
            downstreamAPISettings.DownstreamAPIs[HttpClients.DataSourceHubAPI] = dataSourceHubAPISettings;

            builder.Services
                .AddHttpClient(HttpClients.DataSourceHubAPI,
                    client => { client.BaseAddress = new Uri(dataSourceHubAPISettings.APIUrl); })
                .AddResilienceHandler(
                    "DownstreamPipeline",
                    strategyBuilder =>
                    {
                        strategyBuilder.AddRetry(retryOptions);
                    });

            // PromptHubAPI:
            var promptHubAPISettings = new DownstreamAPIKeySettings
            {
                APIUrl = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_PromptHubAPI_APIUrl]!,
                APIKey = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_PromptHubAPI_APIKey]!
            };
            downstreamAPISettings.DownstreamAPIs[HttpClients.PromptHubAPI] = promptHubAPISettings;

            builder.Services
                    .AddHttpClient(HttpClients.PromptHubAPI,
                        client => { client.BaseAddress = new Uri(promptHubAPISettings.APIUrl); })
                    .AddResilienceHandler(
                        "DownstreamPipeline",
                        strategyBuilder =>
                        {
                            strategyBuilder.AddRetry(retryOptions);
                        });

            // LangChainAPI:
            var langChainAPISettings = new DownstreamAPIKeySettings
            {
                APIUrl = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_LangChainAPI_APIUrl]!,
                APIKey = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_LangChainAPI_APIKey]!
            };
            downstreamAPISettings.DownstreamAPIs[HttpClients.LangChainAPI] = langChainAPISettings;

            builder.Services
                    .AddHttpClient(HttpClients.LangChainAPI,
                        client => { client.BaseAddress = new Uri(langChainAPISettings.APIUrl); })
                    .AddResilienceHandler(
                        "DownstreamPipeline",
                        strategyBuilder =>
                        {
                            strategyBuilder.AddRetry(retryOptions);
                        });

            // SemanticKernelAPI:
            var semanticKernelAPISettings = new DownstreamAPIKeySettings
            {
                APIUrl = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_SemanticKernelAPI_APIUrl]!,
                APIKey = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_SemanticKernelAPI_APIKey]!
            };
            downstreamAPISettings.DownstreamAPIs[HttpClients.SemanticKernelAPI] = semanticKernelAPISettings;

            builder.Services
                    .AddHttpClient(HttpClients.SemanticKernelAPI,
                        client => { client.BaseAddress = new Uri(semanticKernelAPISettings.APIUrl); })
                    .AddResilienceHandler(
                        "DownstreamPipeline",
                        strategyBuilder =>
                        {
                            strategyBuilder.AddRetry(retryOptions);
                        });

            builder.Services.AddSingleton<IDownstreamAPISettings>(downstreamAPISettings);
        }
    }
}
