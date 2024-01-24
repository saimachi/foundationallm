using FoundationaLLM.Common.Constants;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ML.Tokenizers;

namespace FoundationaLLM.Vectorization.Handlers
{
    /// <summary>
    /// Handles the partitioning stage of the vectorization pipeline.
    /// </summary>
    /// <param name="parameters">The dictionary of named parameters used to configure the handler.</param>
    /// <param name="stepsConfiguration">The app configuration section containing the configuration for vectorization pipeline steps.</param>
    /// <param name="contentSourceManagerService">The <see cref="IContentSourceManagerService"/> that manages content sources.</param>
    /// <param name="stateService">The <see cref="IVectorizationStateService"/> that manages vectorization state.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers for logging.</param>
    public class PartitionHandler(
        Dictionary<string, string> parameters,
        IConfigurationSection? stepsConfiguration,
        IContentSourceManagerService contentSourceManagerService,
        IVectorizationStateService stateService,
        ILoggerFactory loggerFactory) : VectorizationStepHandlerBase(VectorizationSteps.Partition, parameters, stepsConfiguration, contentSourceManagerService, stateService, loggerFactory)
    {
        /// <inheritdoc/>
        protected override async Task ProcessRequest(
            VectorizationRequest request,
            VectorizationState state,
            IConfigurationSection? stepConfiguration,
            CancellationToken cancellationToken)
        {
            await _stateService.LoadArtifacts(state, VectorizationArtifactType.ExtractedText);

            var extractedTextArtifact = state.Artifacts.SingleOrDefault(a => a.Type == VectorizationArtifactType.ExtractedText
                && a.Position == 1 && !string.IsNullOrWhiteSpace(a.Content));

            if (extractedTextArtifact == null)
            {
                state.Log(this, request.Id, "The extracted text artifact was not found.");
                return;
            }

            var tokenizer = new Tokenizer(new Bpe());
            var tokens = tokenizer.Encode(extractedTextArtifact.Content!);
        }
    }
}
