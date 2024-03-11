using FoundationaLLM.AgentFactory.Interfaces;

namespace FoundationaLLM.AgentFactory.Core.Interfaces
{
    /// <summary>
    /// Interface for going directly to Azure AI for orchestration.
    /// </summary>
    public interface IAzureAIDirectService : ILLMOrchestrationService
    {
    }
}
