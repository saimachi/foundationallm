using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <summary>
    /// Settings for an orchestration request.
    /// </summary>
    public class OrchestrationSettings
    {
        /// <summary>
        /// The agent's LLM orchestrator type.
        /// </summary>
        [JsonPropertyName("orchestrator")]
        public string? Orchestrator { get; set; }

        /// <summary>
        /// Parameters to override the behavior of the agent.
        /// </summary>
        [JsonPropertyName("agent_parameters")]
        public Dictionary<string, object>? AgentParameters { get; set; }

        /// <summary>
        /// Options to override endpoint configuration (endpoint and key) used to
        /// access a language model by the orchstrator.
        /// </summary>
        [JsonPropertyName("endpoint_configuration")]
        public Dictionary<string, object>? EndpointConfiguration { get; set; }

        /// <summary>
        /// Parameters to override the behavior of the language model as defined on the agent.
        /// </summary>
        [JsonPropertyName("model_parameters")]
        public Dictionary<string, object>? ModelParameters { get; set; }
    }
}
