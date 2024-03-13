using FoundationaLLM.Authorization.Interfaces;
using FoundationaLLM.Common.Models.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationAPI.Controllers
{
    /// <summary>
    /// Provides methods for processing authorization requests.
    /// </summary>
    /// <param name="authorizationCore">The <see cref="IAuthorizationCore"/> service used to process authorization requests.</param>
    [Authorize]
    [Authorize(Policy = "RequiredScope")]
    [ApiController]
    [Route("authorize")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class AuthorizeController(
        IAuthorizationCore authorizationCore)
    {
        private readonly IAuthorizationCore _authorizationCore = authorizationCore;

        [HttpPost(Name = "ProcessAuthorizationRequest")]
        public IActionResult ProcessAuthorizationRequest([FromBody] ActionAuthorizationRequest request) =>
            new OkObjectResult(
                _authorizationCore.ProcessAuthorizationRequest(request));
    }
}
