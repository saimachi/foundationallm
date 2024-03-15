using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authorization;

namespace FoundationaLLM.Vectorization.Services
{
    /// <summary>
    /// Implements an authorization service that bypasses the Authorization API and allows all access by default.
    /// </summary>
    public class NullAuthorizationService : IAuthorizationService
    {
        /// <inheritdoc/>
        public async Task<ActionAuthorizationResult> ProcessAuthorizationRequest(ActionAuthorizationRequest authorizationRequest)
        {
            await Task.CompletedTask;
            return new ActionAuthorizationResult
            {
                Authorized = true
            };
        }
    }
}
