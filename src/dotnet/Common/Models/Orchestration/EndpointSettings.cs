namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <summary>
    /// Settings for an orchestration endpoint.
    /// </summary>
    public class EndpointSettings
    {
        /// <summary>
        /// Uri of the orchestration endpoint.
        /// </summary>
        public string? Endpoint { get; set; }
        /// <summary>
        /// API key for authorizing against an endpoint.
        /// </summary>
        public string? APIKey { get; set; }
        /// <summary>
        /// The type of authentication to use against the endpoint.
        /// This value should be either key or token.
        /// </summary>
        public string? AuthenticationType { get; set; }
    }
}
