using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Services;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization
{
    public class VectorizationWorker
    {
        private readonly IEnumerable<IRequestManagerService> _requestManagerServices;

        public VectorizationWorker(
            IVectorizationStateService vectorizationStateService,
            IDictionary<string, IRequestSourceService> requestSourceServices,
            IEnumerable<IRequestManagerService> requestManagerServices,
            ILogger<VectorizationWorker> logger,
            CancellationToken cancellationToken) =>
            _requestManagerServices = requestManagerServices;

        public async Task Run()
        {
            var requestManagerTasks = _requestManagerServices
                .Select(async rms => await rms.Run())
                .ToArray();

            await Task.WhenAll(requestManagerTasks);
        }
    }
}
