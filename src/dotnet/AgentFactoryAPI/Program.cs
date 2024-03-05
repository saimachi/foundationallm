using Asp.Versioning;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Core.Models.ConfigurationOptions;
using FoundationaLLM.AgentFactory.Core.Services;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.AgentFactory.Services;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Middleware;
using FoundationaLLM.Common.Models.Configuration.API;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.OpenAPI;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Services.API;
using FoundationaLLM.Common.Services.Azure;
using FoundationaLLM.Common.Services.Security;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Common.Validation;
using Microsoft.Extensions.Options;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FoundationaLLM.AgentFactory.API
{
    /// <summary>
    /// Program class for the Agent Factory API
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point for the Agent Factory API
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            DefaultAuthentication.Production = builder.Environment.IsProduction();

            builder.Configuration.Sources.Clear();
            builder.Configuration.AddJsonFile("appsettings.json", false, true);
            builder.Configuration.AddEnvironmentVariables();
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                options.Connect(builder.Configuration[EnvironmentVariables.FoundationaLLM_AppConfig_ConnectionString]);
                options.ConfigureKeyVault(options =>
                {
                    options.SetCredential(DefaultAuthentication.GetAzureCredential());
                });
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIs);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_AgentFactory);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Agent);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_AzureOpenAI);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Events);
            });
            if (builder.Environment.IsDevelopment())
                builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

            // Add services to the container.
            // Add the OpenTelemetry telemetry service and send telemetry data to Azure Monitor.
            builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
            {
                options.ConnectionString = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_AgentFactoryAPI_AppInsightsConnectionString];
            });

            // Create a dictionary of resource attributes.
            var resourceAttributes = new Dictionary<string, object> {
                { "service.name", "AgentFactoryAPI" },
                { "service.namespace", "FoundationaLLM" },
                { "service.instance.id", Guid.NewGuid().ToString() }
            };

            // Configure the OpenTelemetry tracer provider to add the resource attributes to all traces.
            builder.Services.ConfigureOpenTelemetryTracerProvider((sp, builder) =>
                builder.ConfigureResource(resourceBuilder =>
                    resourceBuilder.AddAttributes(resourceAttributes)));

            builder.Services.AddInstanceProperties(builder.Configuration);

            // Add Azure ARM services
            builder.Services.AddAzureResourceManager();

            // Add event services
            builder.Services.AddAzureEventGridEvents(
                builder.Configuration,
                AppConfigurationKeySections.FoundationaLLM_Events_AzureEventGridEventService_Profiles_AgentFactoryAPI);

            //builder.Services.AddServiceProfiler();
            builder.Services.AddControllers();

            // Add API Key Authorization
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IUserClaimsProviderService, NoOpUserClaimsProviderService>();
            builder.Services.AddScoped<APIKeyAuthenticationFilter>();
            builder.Services.AddOptions<APIKeyValidationSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIs_AgentFactoryAPI));
            builder.Services.AddTransient<IAPIKeyValidationService, APIKeyValidationService>();
            builder.Services.AddOptions<InstanceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Instance));

            builder.Services.AddOptions<SemanticKernelServiceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIs_SemanticKernelAPI));

            builder.Services.AddOptions<LangChainServiceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIs_LangChainAPI));

            builder.Services.AddOptions<AgentHubSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIs_AgentHubAPI));

            builder.Services.AddOptions<PromptHubSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIs_PromptHubAPI));

            builder.Services.AddOptions<AgentFactorySettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_AgentFactory));
            
            builder.Services.AddScoped<ILLMOrchestrationService, SemanticKernelService>();
            builder.Services.AddScoped<ILLMOrchestrationService, LangChainService>();

            builder.Services.AddScoped<IAgentFactoryService, AgentFactoryService>();
            builder.Services.AddScoped<IAgentHubAPIService, AgentHubAPIService>();
            builder.Services.AddScoped<IDataSourceHubAPIService, DataSourceHubAPIService>();
            builder.Services.AddScoped<IPromptHubAPIService, PromptHubAPIService>();
            builder.Services.AddScoped<ICallContext, CallContext>();
            builder.Services.AddScoped<IHttpClientFactoryService, HttpClientFactoryService>();
            builder.Services.AddScoped<IUserClaimsProviderService, NoOpUserClaimsProviderService>();

            builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
            builder.Services.AddHostedService<Warmup>();

            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            builder.Services.AddSingleton<IResourceValidatorFactory, ResourceValidatorFactory>();
            builder.Services.AddAgentResourceProvider(builder.Configuration);

            // Register the downstream services and HTTP clients.
            RegisterDownstreamServices(builder);

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

            // Add services to the container.
            builder.Services.AddAuthorization();

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

                    // Adds auth via X-API-KEY header
                    options.AddAPIKeyAuth();
                })
                .AddSwaggerGenNewtonsoftSupport();

            builder.Services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });

            var app = builder.Build();

            // Register the middleware to extract the user identity context and other HTTP request context data required by the downstream services.
            app.UseMiddleware<CallContextMiddleware>();

            app.UseExceptionHandler(exceptionHandlerApp
                => exceptionHandlerApp.Run(async context
                    => await Results.Problem().ExecuteAsync(context)));

            // Configure the HTTP request pipeline.
            app.UseSwagger();
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
                });

            app.UseHttpsRedirection();
            app.UseAuthorization();
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
                DownstreamAPIs = new Dictionary<string, DownstreamAPIKeySettings>()
            };

            var agentHubAPISettings = new DownstreamAPIKeySettings
            {
                APIUrl = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_AgentHubAPI_APIUrl]!,
                APIKey = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_AgentHubAPI_APIKey]!
            };
            downstreamAPISettings.DownstreamAPIs[HttpClients.AgentHubAPI] = agentHubAPISettings;

            var retryOptions = CommonHttpRetryStrategyOptions.GetCommonHttpRetryStrategyOptions();

            builder.Services
                    .AddHttpClient(HttpClients.AgentHubAPI,
                        client => { client.BaseAddress = new Uri(agentHubAPISettings.APIUrl); })
                    .AddResilienceHandler(
                        "DownstreamPipeline",
                        strategyBuilder =>
                        {
                            strategyBuilder.AddRetry(retryOptions);
                        });

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
