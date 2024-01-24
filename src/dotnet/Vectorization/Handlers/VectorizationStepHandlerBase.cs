using FoundationaLLM.Vectorization.Exceptions;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization.Handlers
{
    /// <summary>
    /// Implements basic vectorization step handler functionality.
    /// </summary>
    /// <param name="stepId">The identifier of the vectorization step.</param>
    /// <param name="parameters">The dictionary of named parameters used to configure the handler.</param>
    /// <param name="stepsConfiguration">The app configuration section containing the configuration for vectorization pipeline steps.</param>
    /// <param name="contentSourceManagerService">The <see cref="IContentSourceManagerService"/> that manages content sources.</param>
    /// <param name="stateService">The <see cref="IVectorizationStateService"/> that manages vectorization state.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers for logging.</param>
    public class VectorizationStepHandlerBase(
        string stepId,
        Dictionary<string, string> parameters,
        IConfigurationSection? stepsConfiguration,
        IContentSourceManagerService contentSourceManagerService,
        IVectorizationStateService stateService,
        ILoggerFactory loggerFactory) : IVectorizationStepHandler
    {
        /// <summary>
        /// The identifier of the vectorization step.
        /// </summary>
        protected readonly string _stepId = stepId;
        /// <summary>
        /// The dictionary of named parameters used to configure the handler.
        /// </summary>
        protected readonly Dictionary<string, string> _parameters = parameters;
        /// <summary>
        /// The app configuration section containing the configuration for vectorization pipeline steps.
        /// </summary>
        protected readonly IConfigurationSection? _stepsConfiguration = stepsConfiguration;
        /// <summary>
        /// The content source manager service.
        /// </summary>
        protected readonly IContentSourceManagerService _contentSourceManagerService = contentSourceManagerService;
        /// <summary>
        /// The vectorization state service.
        /// </summary>
        protected readonly IVectorizationStateService _stateService = stateService;
        /// <summary>
        /// The logger used for logging.
        /// </summary>
        protected readonly ILogger<VectorizationStepHandlerBase> _logger =
            loggerFactory.CreateLogger<VectorizationStepHandlerBase>();

        /// <inheritdoc/>
        public string StepId => _stepId;

        /// <inheritdoc/>
        public async Task Invoke(VectorizationRequest request, VectorizationState state, CancellationToken cancellationToken)
        {
            try
            {
                state.LogHandlerStart(this, request.Id);
                _logger.LogInformation("Starting handler {HandlerId} for request {RequestId}", _stepId, request.Id);

                var stepConfiguration = default(IConfigurationSection);

                if (_parameters.TryGetValue("configuration_section", out string? configurationSection))
                {
                    stepConfiguration = _stepsConfiguration!.GetSection(configurationSection);

                    if (stepConfiguration == null
                        || (
                            stepConfiguration.Value == null
                            && !stepConfiguration.GetChildren().Any()
                            ))
                    {
                        _logger.LogError("The configuration section {ConfigurationSection} expected by the {StepId} handler is not available.",
                            configurationSection, _stepId);
                        throw new VectorizationException(
                            $"The configuration section {configurationSection} expected by the {_stepId} handler is not available.");
                    }
                }

                ValidateRequest(request);
                await ProcessRequest(request, state, stepConfiguration, cancellationToken);

                state.LogHandlerEnd(this, request.Id);
                _logger.LogInformation("Finished handler {HandlerId} for request {RequestId}", _stepId, request.Id);
            }
            catch (Exception ex)
            {
                state.LogHandlerError(this, request.Id, ex);
                _logger.LogError(ex, "Error in executing [extract] step handler for request {VectorizationRequestId}.", request.Id);
            }
        }

        private void ValidateRequest(VectorizationRequest request)
        {
            if (request[_stepId] == null)
                throw new VectorizationException($"The request with id {request.Id} does not contain a step with id {_stepId}.");
        }

        /// <summary>
        /// Processes a vectorization request.
        /// The vectorization state will be updated with the result(s) of the processing.
        /// </summary>
        /// <param name="request">The <see cref="VectorizationRequest"/> to be processed.</param>
        /// <param name="state">The <see cref="VectorizationState"/> associated with the vectorization request.</param>
        /// <param name="stepConfiguration">The <see cref="IConfigurationSection"/> providing the configuration required by the step.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that signals stopping the processing.</param>
        /// <returns></returns>
        protected virtual async Task ProcessRequest(
            VectorizationRequest request,
            VectorizationState state,
            IConfigurationSection? stepConfiguration,
            CancellationToken cancellationToken) =>
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
    }
}
