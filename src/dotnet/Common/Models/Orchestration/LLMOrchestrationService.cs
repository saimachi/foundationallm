namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <summary>
    /// Orchestration service enumerator.
    /// </summary>
    public enum LLMOrchestrationService
    {
        /// <summary>
        /// Azure AI orchestration service.
        /// </summary>
        AzureAIDirect,
        /// <summary>
        /// Azure OpenAI orchestration service.
        /// </summary>
        AzureOpenAIDirect,
        /// <summary>
        /// LangChain orchestration service.
        /// </summary>
        LangChain,
        /// <summary>
        /// SemanticKernel orchestration service.
        /// </summary>
        SemanticKernel
    }
}
