using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Models.Configuration
{
    /// <summary>
    /// Provides configuration settings to initialize a request manager instance.
    /// </summary>
    public record RequestManagerServiceSettings
    {
        /// <summary>
        /// The name of the request source that provides the requests processed by the request manager.
        /// </summary>
        [JsonPropertyOrder(0)]
        public required string RequestSourceName { get; set; }

        /// <summary>
        /// The maximum number of handler instances allowed to run in parallel.
        /// </summary>
        [JsonPropertyOrder(1)]
        public int MaxHandlerInstances { get; set; }

        /// <summary>
        /// The wait time after processing a request from the queue in seconds.
        /// </summary>
        [JsonPropertyOrder(3)]
        public int QueueProcessingPace { get; set; } = 5;

        /// <summary>
        /// The interval in seconds to poll the queue for new requests, when the request queue is empty.
        /// </summary>
        [JsonPropertyOrder(4)]
        public int QueuePollingInterval { get; set; } = 60;
    }
}
