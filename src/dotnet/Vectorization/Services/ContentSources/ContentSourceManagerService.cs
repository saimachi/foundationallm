using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Vectorization.Exceptions;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Models.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Vectorization.Services.ContentSources
{
    /// <summary>
    /// Manages content sources registered for use in the vectorization pipelines.
    /// </summary>
    /// <remarks>
    /// Creates a new instance of the content source manager service.
    /// </remarks>
    /// <param name="options">The configuration settings used to initialize the service.</param>
    /// <param name="configuration">The global configuration provider.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers.</param>
    public class ContentSourceManagerService(
        IOptions<ContentSourceManagerServiceSettings> options,
        IConfiguration configuration,
        ILoggerFactory loggerFactory) : IContentSourceManagerService
    {
        private readonly ContentSourceManagerServiceSettings _settings = options.Value;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;

        /// <inheritdoc/>
        public IContentSourceService GetContentSource(string contentSourceName)
        {
            var contentSource = _settings.ContentSources.SingleOrDefault(cs => cs.Name == contentSourceName)
                ?? throw new VectorizationException($"The content source {contentSourceName} is not registered.");

            return contentSource.Type switch
            {
                ContentSourceType.AzureDataLake => CreateAzureDataLakeContentSourceService(contentSourceName),
                _ => throw new VectorizationException($"The content source type {contentSource.Type} is not supported."),
            };
        }

        private DataLakeContentSourceService CreateAzureDataLakeContentSourceService(string contentSourceName)
        {
            var blobStorageServiceSettings = new BlobStorageServiceSettings { AuthenticationType = BlobStorageAuthenticationTypes.Unknown };
            _configuration.Bind(
                $"{AppConfigurationKeySections.FoundationaLLM_Vectorization_ContentSources}:{contentSourceName}",
                blobStorageServiceSettings);

            return new DataLakeContentSourceService(
                blobStorageServiceSettings,
                _loggerFactory);
        }
    }
}
