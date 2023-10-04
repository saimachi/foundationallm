﻿using Asp.Versioning;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Core.API.Controllers
{

    [ApiVersion(1.0)]
    [ApiController]
    [Route("[controller]")]
    public class OrchestrationController : ControllerBase
    {
        private readonly ICoreService _coreService;
        private readonly ILogger<OrchestrationController> _logger;

        public OrchestrationController(ICoreService coreService,
            ILogger<OrchestrationController> logger)
        {
            _coreService = coreService;
            _logger = logger;
        }

        [HttpPost(Name = "SetOrchestratorChoice")]
        public IActionResult SetPreference([FromBody] string orchestrationService)
        {
            var orchestrationPreferenceSet = _coreService.SetLLMOrchestrationPreference(orchestrationService);

            if (orchestrationPreferenceSet)
            {
                return Ok();
            }

            _logger.LogError($"The LLM orchestrator {orchestrationService} is not supported.");
            return BadRequest($"The LLM orchestrator {orchestrationService} is not supported.");
        }
    }
}