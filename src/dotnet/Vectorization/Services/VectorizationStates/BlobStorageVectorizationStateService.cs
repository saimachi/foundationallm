using Azure.Core;
using Azure.Storage.Blobs;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Models.Configuration;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Vectorization.Services.VectorizationStates
{
    public class BlobStorageVectorizationStateService : VectorizationStateServiceBase, IVectorizationStateService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobStorageVectorizationStateServiceSettings _settings;

        public BlobStorageVectorizationStateService(IOptions<BlobStorageVectorizationStateServiceSettings> options)
        {
            _settings = options.Value;
            _blobServiceClient = new BlobServiceClient(_settings.ConnectionString);
        }

        /// <inheritdoc/>
        public async Task<bool> HasState(VectorizationRequest request)
        {
            var blobClient = GetBlobClient(request.ContentIdentifier);

            return await blobClient.ExistsAsync();
        }

        /// <inheritdoc/>
        public async Task<VectorizationState> ReadState(VectorizationRequest request)
        {
            var blobClient = GetBlobClient(request.ContentIdentifier);

            var response = await blobClient.DownloadAsync();
            using var reader = new StreamReader(response.Value.Content);
            var content = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<VectorizationState>(content)!;
        }

        /// <inheritdoc/>
        public async Task SaveState(VectorizationState state)
        {
            var blobClient = GetBlobClient(state.ContentIdentifier);

            var content = JsonSerializer.Serialize(state);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            await blobClient.UploadAsync(stream, true);
        }

        private BlobClient GetBlobClient(VectorizationContentIdentifier contentIdentifier)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_settings.ContainerName);
            return containerClient.GetBlobClient(
                GetPersistenceIdentifier(contentIdentifier));
        }

        protected override string GetPersistenceIdentifier(VectorizationContentIdentifier contentIdentifier) =>
            $"{contentIdentifier.CanonicalId}_state_{HashContentIdentifier(contentIdentifier)}.json";
    }
}
