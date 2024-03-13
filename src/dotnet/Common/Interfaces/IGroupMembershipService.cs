namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Provides group membership services.
    /// </summary>
    public interface IGroupMembershipService
    {
        /// <summary>
        /// Retrieves the group identifiers list of the groups where the specified user principal is a member.
        /// </summary>
        /// <param name="userPrincipalName">The user principal name (UPN) for which group membership is retrieved.</param>
        /// <returns></returns>
        Task<List<string>> GetGroupsForPrincipal(string userPrincipalName);
    }
}
