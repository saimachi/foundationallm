using System.Text.Json.Serialization;

namespace FoundationaLLM.Vectorization.Models
{
    /// <summary>
    /// Provides detials about a content source.
    /// </summary>
    public class ContentSource
    {
        /// <summary>
        /// The name of the content source.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// The type of the content source.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required ContentSourceType Type { get; set; }
    }
}
