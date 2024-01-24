using Asp.Versioning;
using Azure.Identity;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.OpenAPI;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models.Configuration;
using FoundationaLLM.Vectorization.Services.ContentSources;
using FoundationaLLM.Vectorization.Services.VectorizationStates;
using FoundationaLLM.Vectorization.Worker;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", false, true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(builder.Configuration[AppConfigurationKeys.FoundationaLLM_AppConfig_ConnectionString]);
    options.ConfigureKeyVault(options =>
    {
        options.SetCredential(new DefaultAzureCredential());
    });
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_Vectorization);
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIs_VectorizationWorker);
});
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

builder.Services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
{
    ConnectionString = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_VectorizationWorker_AppInsightsConnectionString],
    DeveloperMode = builder.Environment.IsDevelopment()
});

var allowAllCorsOrigins = "AllowAllOrigins";
builder.Services.AddCors(policyBuilder =>
{
    policyBuilder.AddPolicy(allowAllCorsOrigins,
        policy =>
        {
            policy.AllowAnyOrigin();
            policy.WithHeaders("DNT", "Keep-Alive", "User-Agent", "X-Requested-With", "If-Modified-Since", "Cache-Control", "Content-Type", "Range", "Authorization", "X-AGENT-HINT");
            policy.AllowAnyMethod();
        });
});

// Add configurations to the container
builder.Services.AddOptions<VectorizationWorkerSettings>()
    .Bind(builder.Configuration.GetSection(AppConfigurationKeys.FoundationaLLM_Vectorization_VectorizationWorker));
builder.Services.AddOptions<BlobStorageVectorizationStateServiceSettings>()
    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Vectorization_StateService));
builder.Services.AddOptions<ContentSourceManagerServiceSettings>()
    .Bind(builder.Configuration.GetSection(AppConfigurationKeys.FoundationaLLM_Vectorization_ContentSourceManagerService));

builder.Services.AddSingleton(
    typeof(IEnumerable<IConfigurationSection>),
    new IConfigurationSection[] {
        builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Vectorization_Queues),
        builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Vectorization_Steps)
    });

// Add services to the container.
builder.Services.AddTransient<IAPIKeyValidationService, APIKeyValidationService>();
builder.Services.AddSingleton<IVectorizationStateService, BlobStorageVectorizationStateService>();
builder.Services.AddSingleton<IContentSourceManagerService, ContentSourceManagerService>();
builder.Services.AddHostedService<Worker>();

builder.Services.AddControllers();

// Add API Key Authorization
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<APIKeyAuthenticationFilter>();
builder.Services.AddOptions<APIKeyValidationSettings>()
    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIs_VectorizationWorker));

builder.Services
    .AddApiVersioning(options =>
    {
        // Reporting api versions will return the headers
        // "api-supported-versions" and "api-deprecated-versions"
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
    })
    .AddApiExplorer(options =>
    {
        // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
        // note: the specified format code will format the version as "'v'major[.minor][-status]"
        options.GroupNameFormat = "'v'VVV";
    });

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
    });

var app = builder.Build();

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