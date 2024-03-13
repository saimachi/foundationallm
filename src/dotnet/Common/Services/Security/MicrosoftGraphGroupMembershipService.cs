using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using Microsoft.Graph;

namespace FoundationaLLM.Common.Services.Security
{
    /// <summary>
    /// Implements group membership services using the Microsoft Graph API.
    /// </summary>
    public class MicrosoftGraphGroupMembershipService : IGroupMembershipService
    {
        private readonly GraphServiceClient _graphClient = new GraphServiceClient(
            DefaultAuthentication.GetAzureCredential());

        /// <inheritdoc/>
        public async Task<List<string>> GetGroupsForPrincipal(string userPrincipalName)
        {
            var result = await _graphClient.Users[userPrincipalName].TransitiveMemberOf.GraphGroup.GetAsync().ConfigureAwait(false);
            return result == null || result.Value == null
                ? []
                : result.Value.Where(x => x.Id != null).Select(x => x.Id!).ToList();
        }
    }
}
