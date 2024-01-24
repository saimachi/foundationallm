using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Interfaces
{
    /// <summary>
    /// Manages content sources registered for use by the vectorization pipelines.
    /// </summary>
    public interface IContentSourceManagerService
    {
        /// <summary>
        /// Gets a content source specified by name.
        /// </summary>
        /// <param name="contentSourceName">The name of the content source to retrieve.</param>
        /// <returns>The <see cref="IContentSourceService"/> instance of the requested content source.</returns>
        IContentSourceService GetContentSource(string contentSourceName);
    }
}
