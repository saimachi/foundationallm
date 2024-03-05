﻿using Asp.Versioning;
using FoundationaLLM.Agent.Constants;
using FoundationaLLM.Agent.Models.Resources;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Configuration.Branding;
using FoundationaLLM.Common.Models.Configuration.Users;
using FoundationaLLM.Common.Models.ResourceProvider;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;


namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides methods for retrieving and managing user profiles.
    /// </summary>
    /// <remarks>
    /// Constructor for the UserProfiles Controller.
    /// </remarks>
    /// <param name="userProfileService">The Core service provides methods for managing the user profile.</param>
    [Authorize]
    [Authorize(Policy = "RequiredScope")]
    [ApiController]
    [Route("[controller]")]
    public class UserProfilesController(
        IUserProfileService userProfileService) : ControllerBase
    {
        private readonly IUserProfileService _userProfileService = userProfileService;

        /// <summary>
        /// Retrieves user profiles.
        /// </summary>
        [HttpGet(Name = "GetUserProfile")]
        public async Task<IActionResult> Index() =>
            Ok(await _userProfileService.GetUserProfileAsync());
    }
}
