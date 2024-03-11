using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <summary>
    /// Input for a direct Azure AI request.
    /// </summary>
    public class AzureAIDirectRequest
    {
        /// <summary>
        /// Input data for a direct request to an Azure AI model.
        /// </summary>
        [JsonPropertyName("input_data")]
        public InputData? InputData { get; set; }
    }

    /// <summary>
    /// Input data for a direct request to an Azure AI model.
    /// </summary>
    public class InputData
    {
        /// <summary>
        /// Object defining the required input role and content key value pairs.
        /// </summary>
        [JsonPropertyName("input_string")]
        public InputString[]? InputString { get; set; }

        /// <summary>
        /// Model configuration parameters.
        /// </summary>
        [JsonPropertyName("parameters")]
        public Parameters? Parameters { get; set; }
    }

    /// <summary>
    /// Supported model configuration parameters.
    /// </summary>
    public class Parameters
    {
        /// <summary>
        /// Controls randomness in the model.
        /// Lower values will make the model more deterministic
        /// and higher values will make the model more random.
        /// </summary>
        [JsonPropertyName("temperature")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? Temperature { get; set; } = 0.0f;

        /// <summary>
        /// The number of highest probability vocabulary tokens to keep for top-k-filtering.
        /// Default value is null, which disables top-k-filtering.
        /// </summary>
        [JsonPropertyName("top_k")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? TopK { get; set; }

        /// <summary>
        /// The cumulative probability of parameter highest probability vocabulary tokens
        /// to keep for nucleus sampling, defaults to null.
        /// </summary>
        [JsonPropertyName("top_p")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? TopP { get; set; }

        /// <summary>
        /// Whether or not to use sampling; use greedy decoding otherwise.
        /// </summary>
        [JsonPropertyName("do_sample")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? DoSample { get; set; }

        /// <summary>
        /// The maximum number of tokens to generate.
        /// </summary>
        [JsonPropertyName("max_new_tokens")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxNewTokens { get; set; }

        /// <summary>
        /// Whether or not to return the full text (prompt + response) or only the generated part (response).
        /// Default value is false.
        /// </summary>
        [JsonPropertyName("return_full_text")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? ReturnFullText { get; set; }

        /// <summary>
        /// Whether to ignore the EOS token and continue generating tokens after the EOS token is generated.
        /// Defaults to False.
        /// </summary>
        [JsonPropertyName("ignore_eos")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IgnoreEOS { get; set; }
    }

    /// <summary>
    /// Object defining the required input role and content key value pairs.
    /// </summary>
    public class InputString
    {
        /// <summary>
        /// The role of the chat persona creating content.
        /// Value will be either "user" or "assistant".
        /// </summary>
        [JsonPropertyName("role")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Role { get; set; }

        /// <summary>
        /// The text either input into or output by the model.
        /// </summary>
        [JsonPropertyName("content")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Content { get; set; }
    }
}
