﻿using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Core.Interfaces;

public interface IGatekeeperAPIService
{
    Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest);
    Task<string> GetSummary(string content);
    Task<bool> SetLLMOrchestrationPreference(string orchestrationService);
    Task AddMemory(object item, string itemName, Action<object, float[]> vectorizer);
    Task RemoveMemory(object item);
}