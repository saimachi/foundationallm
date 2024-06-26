﻿using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Methods for orchestration services exposed by the Gatekeeper API service.
    /// </summary>
    /// <remarks>
    /// Constructor for the Orchestration Controller.
    /// </remarks>
    [Authorize(Policy = "DefaultPolicy")]
    [ApiController]
    [Route("[controller]")]
    public class OrchestrationController : ControllerBase
    {
        private readonly ICoreService _coreService;
        private readonly IResourceProviderService _agentResourceProvider;
#pragma warning disable IDE0052 // Remove unread private members.
        private readonly ILogger<OrchestrationController> _logger;
        ICallContext _callContext;

        /// <summary>
        /// Methods for orchestration services exposed by the Gatekeeper API service.
        /// </summary>
        /// <remarks>
        /// Constructor for the Orchestration Controller.
        /// </remarks>
        /// <param name="coreService">The Core service provides methods for getting
        /// completions from the orchestrator.</param>
        /// <param name="callContext">The call context for the request.</param>
        /// <param name="resourceProviderServices">The list of <see cref="IResourceProviderService"/> resource provider services.</param>
        /// <param name="logger">The logging interface used to log under the
        /// <see cref="OrchestrationController"/> type name.</param>
        public OrchestrationController(ICoreService coreService,
            ICallContext callContext,
            IEnumerable<IResourceProviderService> resourceProviderServices,
            ILogger<OrchestrationController> logger)
        {
            _coreService = coreService;
            var resourceProviderServicesDictionary = resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);
            if (!resourceProviderServicesDictionary.TryGetValue(ResourceProviderNames.FoundationaLLM_Agent, out var agentResourceProvider))
                throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Agent} was not loaded.");
            _agentResourceProvider = agentResourceProvider; ;
            _logger = logger;
            _callContext = callContext;
        }

        /// <summary>
        /// Requests a completion from the downstream APIs.
        /// </summary>
        /// <param name="directCompletionRequest">The user prompt for which to generate a completion.</param>
        [HttpPost("completion", Name = "GetCompletion")]
        public async Task<IActionResult> GetCompletion([FromBody] CompletionRequest directCompletionRequest)
        {
            var completionResponse = await _coreService.GetCompletionAsync(directCompletionRequest);

            return Ok(completionResponse);
        }

        /// <summary>
        /// Retrieves a list of global and private agents.
        /// </summary>
        /// <returns></returns>
        [HttpGet("agents", Name = "GetAgents")]
        public async Task<IEnumerable<ResourceBase>> GetAgents()
        {
            var agents = new List<ResourceBase>();

            if (await _agentResourceProvider.HandleGetAsync($"/{AgentResourceTypeNames.Agents}", _callContext.CurrentUserIdentity) is List<AgentBase> globalAgentsList && globalAgentsList.Count != 0)
            {
                agents.AddRange(globalAgentsList);
            }

            return agents;
        }
    }
}
