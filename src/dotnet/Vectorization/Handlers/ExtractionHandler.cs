using FoundationaLLM.Common.Constants;
using FoundationaLLM.Vectorization.Exceptions;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization.Handlers
{
    /// <summary>
    /// Handles the extraction stage of the vectorization pipeline.
    /// </summary>
    /// <param name="parameters">The dictionary of named parameters used to configure the handler.</param>
    /// <param name="stepsConfiguration">The app configuration section containing the configuration for vectorization pipeline steps.</param>
    /// <param name="contentSourceManagerService">The <see cref="IContentSourceManagerService"/> that manages content sources.</param>
    /// <param name="stateService">The <see cref="IVectorizationStateService"/> that manages vectorization state.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers for logging.</param>
    public class ExtractionHandler(
        Dictionary<string, string> parameters,
        IConfigurationSection? stepsConfiguration,
        IContentSourceManagerService contentSourceManagerService,
        IVectorizationStateService stateService,
        ILoggerFactory loggerFactory) : VectorizationStepHandlerBase(VectorizationSteps.Extract, parameters, stepsConfiguration, contentSourceManagerService, stateService, loggerFactory)
    {
        /// <inheritdoc/>
        protected override async Task ProcessRequest(
            VectorizationRequest request,
            VectorizationState state,
            IConfigurationSection? stepConfiguration,
            CancellationToken cancellationToken)
        {
            var contentSource = _contentSourceManagerService.GetContentSource(_parameters["content_source_name"]);

            var textContent = await contentSource.ExtractTextFromFileAsync(request.ContentIdentifier.MultipartId, cancellationToken);

            state.AddOrReplaceArtifact(new VectorizationArtifact
            {
                Type = VectorizationArtifactType.ExtractedText,
                Position = 1,
                Content = textContent
            });
        }
    }
}
