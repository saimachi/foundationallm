using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Common.Models.TextEmbedding;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;
using FoundationaLLM.Vectorization.Exceptions;

namespace FoundationaLLM.Vectorization.Services.ContentSources
{
    /// <summary>
    /// Extracts text from a web page.
    /// </summary>
    public class WebPageContentSourceService : IContentSourceService
    {
        private readonly ILogger<WebPageContentSourceService> _logger;

        /// <summary>
        /// Creates a new instance of the vectorization content source that scrapes web pages.
        /// </summary>
        /// <param name="loggerFactory">Logger factory that generates loggers for the class.</param>
        public WebPageContentSourceService(ILoggerFactory loggerFactory) =>
            _logger = loggerFactory.CreateLogger<WebPageContentSourceService>();

        /// <inheritdoc/>
        /// <remarks>
        /// Expected MultipartId:
        /// contentId[0] = Protocol: either http or https
        /// contentId[1] = the web URL without the protocol
        /// </remarks>
        public async Task<string> ExtractTextAsync(ContentIdentifier contentId, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            string url = $"{contentId[0]}://{contentId[1]}";

            using (var httpClient = new HttpClient())
            {
                try
                {
                    // Get the HTML content from the URL.  
                    string html = await httpClient.GetStringAsync(url);

                    // Load the HTML content into an HtmlDocument.  
                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(html);

                    // Use XPath to select the body of the HTML document.  
                    var bodyNode = htmlDocument.DocumentNode.SelectSingleNode("//body");

                    if (bodyNode == null)
                    {
                        throw new VectorizationException($"No body tag found in the HTML document located at: {url}.");
                    }

                    // Remove script and style elements.  
                    htmlDocument.DocumentNode.Descendants()
                        .Where(n => n.Name == "script" || n.Name == "style")
                        .ToList()
                        .ForEach(n => n.Remove());

                    // Extract the inner text of the body element, which includes all visible text within the body.
                    // This text will not include any HTML tags.
                    string visibleText = bodyNode.InnerText;

                    // Convert multiple whitespace into a single space and trim the text.  
                    visibleText = System.Text.RegularExpressions.Regex.Replace(visibleText, @"\s+", " ").Trim();

                    return visibleText;
                }
                catch (HttpRequestException ex)
                {                    
                    throw new VectorizationException($"Failed to retrieve content from {url}: {ex.Message}");
                }

            }           
        } 
    }
}
