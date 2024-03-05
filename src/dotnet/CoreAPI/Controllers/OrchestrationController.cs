﻿using Asp.Versioning;
using FoundationaLLM.Agent.Constants;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Configuration.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProvider;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;


namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Methods for orchestration services exposed by the Gatekeeper API service.
    /// </summary>
    /// <remarks>
    /// Constructor for the Orchestration Controller.
    /// </remarks>
    [Authorize]
    [Authorize(Policy = "RequiredScope")]
    [ApiController]
    [Route("[controller]")]
    public class OrchestrationController : ControllerBase
    {
        private readonly ICoreService _coreService;
        private readonly IResourceProviderService _agentResourceProvider;
#pragma warning disable IDE0052 // Remove unread private members.
        private readonly ILogger<OrchestrationController> _logger;

        /// <summary>
        /// Methods for orchestration services exposed by the Gatekeeper API service.
        /// </summary>
        /// <remarks>
        /// Constructor for the Orchestration Controller.
        /// </remarks>
        /// <param name="coreService">The Core service provides methods for getting
        /// completions from the orchestrator.</param>
        /// <param name="resourceProviderServices">The list of <see cref="IResourceProviderService"/> resource provider services.</param>
        /// <param name="logger">The logging interface used to log under the
        /// <see cref="OrchestrationController"/> type name.</param>
        public OrchestrationController(ICoreService coreService,
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
        }

        /// <summary>
        /// Requests a completion from the downstream APIs.
        /// </summary>
        /// <param name="directCompletionRequest">The user prompt for which to generate a completion.</param>
        [HttpPost("completion", Name = "GetCompletion")]
        public async Task<IActionResult> GetCompletion([FromBody] DirectCompletionRequest directCompletionRequest)
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

            if (await _agentResourceProvider.HandleGetAsync($"/{AgentResourceTypeNames.Agents}") is List<AgentBase> globalAgentsList && globalAgentsList.Count != 0)
            {
                agents.AddRange(globalAgentsList);
            }

            return agents;
        }
    }
}
