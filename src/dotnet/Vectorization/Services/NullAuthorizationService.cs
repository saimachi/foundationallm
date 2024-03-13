using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authorization;

namespace FoundationaLLM.Vectorization.Services
{
    public class NullAuthorizationService : IAuthorizationService
    {
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
