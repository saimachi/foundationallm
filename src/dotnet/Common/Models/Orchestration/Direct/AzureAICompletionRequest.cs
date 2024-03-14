using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Direct
{
    /// <summary>
    /// Input for a direct Azure AI request.
    /// </summary>
    public class AzureAICompletionRequest
    {
        /// <summary>
        /// Input data for a direct request to an Azure AI model.
        /// </summary>
        [JsonPropertyName("input_data")]
        public AzureAICompletionInputData? InputData { get; set; }
    }
}
