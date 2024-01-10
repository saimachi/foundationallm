using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Common.Services
{
    /// <summary>
    /// Provides access to Azure Data Lake blob storage.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="DataLakeStorageService"/> with the specified options and logger.
    /// </remarks>
    /// <param name="options">The options object containing the <see cref="BlobStorageServiceSettings"/> object with the settings.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class DataLakeStorageService(
        IOptions<BlobStorageServiceSettings> options,
        ILogger<DataLakeStorageService> logger) : IStorageService
    {
#pragma warning disable IDE0052 // Remove unread private members
        private readonly BlobStorageServiceSettings _settings = options.Value;
        private readonly ILogger<DataLakeStorageService> _logger = logger;
    }
}
