﻿using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Vectorization.ResourceProviders;

namespace FoundationaLLM.Vectorization.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="VectorizationResourceProviderService"/>.
    /// </summary>
    public static class VectorizationResourceProviderServiceExtensions
    {
        /// <summary>
        /// Retrieves all active vectorization pipelines.
        /// </summary>
        /// <param name="vectorizationResourceProvider">An instance of the vectorization resource provider service</param>
        /// <returns>List of active pipelines.</returns>
        public static async Task<List<VectorizationPipeline>> GetActivePipelines(this VectorizationResourceProviderService vectorizationResourceProvider)
        {
            var pipelinesList = await vectorizationResourceProvider.GetResourcesAsync($"/{VectorizationResourceTypeNames.VectorizationPipelines}") as List<VectorizationPipeline>;
            if (pipelinesList == null)
                return new List<VectorizationPipeline>();
            return pipelinesList.Where(p => p.Active).ToList();
        }

        /// <summary>
        /// Sets the specified vectorization pipeline to active or inactive.
        /// </summary>
        /// <param name="vectorizationResourceProvider">An instance of the vectorization resource provider.</param>
        /// <param name="pipelineObjectId">The object id of the pipeline to deactivate</param>
        /// <param name="activate">true if the pipeline should be activated, false if it is to be deactivated.</param>
        /// <returns></returns>
        public static async Task TogglePipelineActivation(this VectorizationResourceProviderService vectorizationResourceProvider, string pipelineObjectId, bool activate)
        {
            var pipeline =  vectorizationResourceProvider.GetResource<VectorizationPipeline>(pipelineObjectId);                        
           
            if (pipeline == null || pipeline.Active == activate)
                // nothing to update
                return;

            // update the pipeline active state
            string slug = activate ? "activate" : "deactivate";           
            await vectorizationResourceProvider.ExecuteActionAsync($"{pipelineObjectId}/{slug}");
        }
        
    }
}
