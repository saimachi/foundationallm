using Azure.Core;
using Azure.Storage.Blobs;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Vectorization.Services.VectorizationStates
{
    /// <summary>
    /// Provides vectorization state persistence services using  Azure blob storage.
    /// </summary>
    public class BlobStorageVectorizationStateService : VectorizationStateServiceBase, IVectorizationStateService
    {
        private readonly BlobStorageService _storageService;
        private readonly BlobStorageVectorizationStateServiceSettings _settings;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Creates a new vectorization state service instance.
        /// </summary>
        /// <param name="options">The options used to configure the new instance.</param>
        /// <param name="loggerFactory">The logger factory used to create loggers.</param>
        public BlobStorageVectorizationStateService(
            IOptions<BlobStorageVectorizationStateServiceSettings> options,
            ILoggerFactory loggerFactory)
        {
            _settings = options.Value;
            _loggerFactory = loggerFactory;
            _storageService = new BlobStorageService(
                Options.Create(_settings.Storage),
                _loggerFactory.CreateLogger<BlobStorageService>());
        }

        /// <inheritdoc/>
        public async Task<bool> HasState(VectorizationRequest request) =>
            await _storageService.FileExistsAsync(
                _settings.StorageContainerName,
                $"{GetPersistenceIdentifier(request.ContentIdentifier)}.json");


        /// <inheritdoc/>
        public async Task<VectorizationState> ReadState(VectorizationRequest request)
        {
            var content = await _storageService.ReadFileAsync(
                _settings.StorageContainerName,
                $"{GetPersistenceIdentifier(request.ContentIdentifier)}.json");

            return JsonSerializer.Deserialize<VectorizationState>(content)!;
        }

        /// <inheritdoc/>
        public async Task LoadArtifacts(VectorizationState state, VectorizationArtifactType artifactType)
        {
            foreach (var artifact in state.Artifacts.Where(a => a.Type == artifactType))
                if (!string.IsNullOrWhiteSpace(artifact.CanonicalId))
                    artifact.Content = Encoding.UTF8.GetString(
                        await _storageService.ReadFileAsync(
                            _settings.StorageContainerName,
                            artifact.CanonicalId));
        }

        /// <inheritdoc/>
        public async Task SaveState(VectorizationState state)
        {
            var persistenceIdentifier = GetPersistenceIdentifier(state.ContentIdentifier);

            foreach (var artifact in state.Artifacts)
                if (artifact.IsDirty)
                {
                    var artifactPath =
                        $"{persistenceIdentifier}_{artifact.Type.ToString().ToLower()}_{artifact.Position:D6}.txt";

                    await _storageService.WriteFileAsync(
                        _settings.StorageContainerName,
                        artifactPath,
                        artifact.Content);
                    artifact.CanonicalId = artifactPath;
                }

            var content = JsonSerializer.Serialize(state);
            await _storageService.WriteFileAsync(
                _settings.StorageContainerName,
                $"{persistenceIdentifier}.json",
                content);
        }
    }
}
