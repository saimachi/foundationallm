using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.Orchestration.Direct
{
    /// <summary>
    /// Contains the usage statistics for a completion.
    /// </summary>
    public class CompletionUsage
    {
        /// <summary>
        /// The number of tokens used for the prompt.
        /// </summary>
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; } = 0;

        /// <summary>
        /// The number of tokens used for the completion.
        /// </summary>
        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; } = 0;

        /// <summary>
        /// The total number of tokens used for the completion.
        /// </summary>
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; } = 0;
    }
}
