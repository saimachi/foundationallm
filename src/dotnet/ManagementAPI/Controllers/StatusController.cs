﻿using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Management.API.Controllers
{
    /// <summary>
    /// Provides methods for checking the status of the service.
    /// </summary>
    [Authorize]
    [Authorize(Policy = "RequiredScope")]
    [ApiController]
    [Route("status")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class StatusController : ControllerBase
    {
        /// <summary>
        /// Returns the status of the Core API service.
        /// </summary>
        [AllowAnonymous]
        [HttpGet(Name = "GetServiceStatus")]
        public IActionResult GetServiceStatus() =>
            new OkObjectResult(new ServiceStatusInfo
            {
                Name = ServiceNames.ManagementAPI,
                Instance = Environment.GetEnvironmentVariable(EnvironmentVariables.Hostname),
                Version = Environment.GetEnvironmentVariable(EnvironmentVariables.FoundationaLLM_Version),
                Status = ServiceStatuses.Ready
            });

        /// <summary>
        /// Returns OK if the requester is authenticated and allowed to execute
        /// requests against this service.
        /// </summary>
        [HttpGet("auth", Name = "GetAuthStatus")]
        public IActionResult GetAuthStatus() =>
            Ok();

        /// <summary>
        /// Returns the allowed HTTP methods for the Core API service.
        /// </summary>
        [AllowAnonymous]
        [HttpOptions]
        public IActionResult Options()
        {
            HttpContext.Response.Headers.Append("Allow", new[] { "GET", "POST", "OPTIONS" });

            return Ok();
        }
    }
}
