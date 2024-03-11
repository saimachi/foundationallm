using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <summary>
    /// The response from the Azure AI orchestration service.
    /// </summary>
    public class AzureAIDirectResponse
    {
        /// <summary>
        /// The completion output from an Azure AI model.
        /// </summary>
        [JsonPropertyName("output")]
        public string? Output { get; set; } 
    }
}
