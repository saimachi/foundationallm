using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Direct
{
    /// <summary>
    /// Object defining the completion role and content key value pairs.
    /// </summary>
    public class CompletionMessage
    {
        /// <summary>
        /// The role of the chat persona creating content.
        /// Value will be one of: "user", "assistant", "tool", or "system".
        /// </summary>
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        /// <summary>
        /// The text either input into or output by the model.
        /// </summary>
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}
