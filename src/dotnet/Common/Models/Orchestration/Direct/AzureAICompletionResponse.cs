using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Direct
{
    /// <summary>
    /// The response from the Azure AI orchestration service.
    /// </summary>
    public class AzureAICompletionResponse
    {
        /// <summary>
        /// The completion output from an Azure AI model.
        /// </summary>
        [JsonPropertyName("output")]
        public string? Output { get; set; }
    }
}
