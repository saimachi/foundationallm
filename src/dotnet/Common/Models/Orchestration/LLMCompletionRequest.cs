using FoundationaLLM.Common.Models.Chat;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <summary>
    /// Base LLM orchestration request
    /// </summary>
    [JsonDerivedType(typeof(KnowledgeManagementCompletionRequest), typeDiscriminator: "knowledge-management")]
    [JsonDerivedType(typeof(InternalContextCompletionRequest), typeDiscriminator: "internal-context")]    
    [JsonDerivedType(typeof(LegacyCompletionRequest), typeDiscriminator: "legacy")]
    [JsonDerivedType(typeof(LLMCompletionRequest), typeDiscriminator: "llm")]
    public class LLMCompletionRequest
    {
        /// <summary>
        /// The session ID.
        /// </summary>
        [JsonPropertyName("session_id")]
        public string? SessionId { get; set; }

        /// <summary>
        /// Prompt entered by the user.
        /// </summary>
        [JsonPropertyName("user_prompt")]
        public string? UserPrompt { get; set; }

        /// <summary>
        /// The message history associated with the completion request.
        /// </summary>
        [JsonPropertyName("message_history")]
        public List<MessageHistoryItem>? MessageHistory { get; init; } = [];

        /// <summary>
        /// Collection of model settings to override with the orchestration request.
        /// </summary>
        [JsonPropertyName("settings")]
        public OrchestrationSettings? Settings { get; set; }
    }
}
