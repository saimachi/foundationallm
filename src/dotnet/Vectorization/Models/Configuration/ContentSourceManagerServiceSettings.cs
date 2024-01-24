using FoundationaLLM.Vectorization.Services.ContentSources;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Vectorization.Models.Configuration
{
    /// <summary>
    /// Provides configuration settings for the <see cref="ContentSourceManagerService"/> service.
    /// </summary>
    public class ContentSourceManagerServiceSettings
    {
        /// <summary>
        /// The list of all content sources that are registered for use by the vectorization pipelines.
        /// </summary>
        public required List<ContentSource> ContentSources { get; set; }
    }
}
