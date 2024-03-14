using FoundationaLLM.AgentFactory.Interfaces;

namespace FoundationaLLM.AgentFactory.Core.Interfaces
{
    /// <summary>
    /// Interface for going directly to Azure OpenAI for orchestration.
    /// </summary>
    public interface IAzureOpenAIDirectService : ILLMOrchestrationService
    {
    }
}
