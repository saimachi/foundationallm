using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Models.Configuration
{
    /// <summary>
    /// Provides configuration settings to initialize a vectorization state service.
    /// </summary>
    public class VectorizationStateServiceSettings
    {
        /// <summary>
        /// The connection string to connect to the underlying persistence service.
        /// </summary>
        public required string ConnectionString { get; set; }

        /// <summary>
        /// The name of the container where the underlying persistence service stores vectorization state.
        /// </summary>
        public required string ContainerName { get; set; }
    }
}
