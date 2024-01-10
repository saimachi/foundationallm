using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Models
{
    /// <summary>
    /// Represents the content associated with a vectorization request.
    /// </summary>
    public class VectorizationContentIdentifier
    {
        /// <summary>
        /// The unique identifier of the content (i.e., document) being vectorized.
        /// </summary>
        [JsonPropertyOrder(1)]
        [JsonPropertyName("unique_id")]
        public required string UniqueId { get; set; }

        /// <summary>
        /// The canonical identifier of the content being vectorized.
        /// Vectorization state services use it to derive the location of the state in the underlying storage.
        /// </summary>
        [JsonPropertyOrder(2)]
        [JsonPropertyName("canonical_id")]
        public required string CanonicalId { get; set; }
    }
}
