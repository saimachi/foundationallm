
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models.Configuration;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Vectorization.Worker
{
    /// <summary>
    /// The background service used to run the worker.
    /// </summary>
    /// <remarks>
    /// Creates a new instance of the worker.
    /// </remarks>
    /// <param name="stateService">The <see cref="IVectorizationStateService"/> used to manage the vectorization state.</param>
    /// <param name="contentSourceManagerService">The <see cref="IContentSourceManagerService"/> used to manage content sources.</param>
    /// <param name="settings">The <see cref="VectorizationWorkerSettings"/> options holding the vectorization worker settings.</param>
    /// <param name="configurationSections">The list of configuration sections required by the vectorization worker builder.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to create loggers in child objects.</param>
    public class Worker(
        IVectorizationStateService stateService,
        IContentSourceManagerService contentSourceManagerService,
        IOptions<VectorizationWorkerSettings> settings,
        IEnumerable<IConfigurationSection> configurationSections,
        ILoggerFactory loggerFactory) : BackgroundService
    {
        private readonly IVectorizationStateService _stateService = stateService;
        private readonly IContentSourceManagerService _contentSourceManagerService = contentSourceManagerService;
        private readonly VectorizationWorkerSettings _settings = settings.Value;
        private readonly IEnumerable<IConfigurationSection> _configurationSections = configurationSections;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var vectorizationWorker = new VectorizationWorkerBuilder()
                .WithStateService(_stateService)
                .WithSettings(_settings)
                .WithQueuesConfiguration(_configurationSections.Single(cs => cs.Path == AppConfigurationKeySections.FoundationaLLM_Vectorization_Queues))
                .WithStepsConfiguration(_configurationSections.Single(cs => cs.Path == AppConfigurationKeySections.FoundationaLLM_Vectorization_Steps))
                .WithContentSourceManager(_contentSourceManagerService)
                .WithLoggerFactory(_loggerFactory)
                .WithCancellationToken(stoppingToken)
                .Build();

            await vectorizationWorker.Run();
        }
    }
}
