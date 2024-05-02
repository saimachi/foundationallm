﻿using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.AzureAIService;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Core.Examples.Interfaces;
using FoundationaLLM.Core.Examples.Models;

namespace FoundationaLLM.Core.Examples.Services
{
    /// <summary>
    /// Service for running agent conversations using the Core API.
    /// </summary>
    /// <param name="coreAPITestManager"></param>
    /// <param name="azureAIService"></param>
    public class AgentConversationTestService(
        ICoreAPITestManager coreAPITestManager,
        IAzureAIService azureAIService) : IAgentConversationTestService
    {
        /// <inheritdoc/>
        public async Task<IEnumerable<Message>> RunAgentConversationWithSession(string agentName,
            List<string> userPrompts, string? sessionId = null)
        {
            var sessionCreated = false;
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                // Create a new session since an existing ID was not provided.
                sessionId = await coreAPITestManager.CreateSessionAsync();
                sessionCreated = true;
            }

            // TODO: Create a new agent if it does not exist. Use the ManagementAPITestManager to create the agent.

            // Send user prompts and agent responses.
            foreach (var userPrompt in userPrompts)
            {
                // Create a new orchestration request for the user prompt and chat session.
                var orchestrationRequest = new OrchestrationRequest
                {
                    SessionId = sessionId,
                    AgentName = agentName,
                    UserPrompt =userPrompt,
                    Settings = null
                };

                // Send the orchestration request to the Core API's session completion endpoint.
                await coreAPITestManager.SendSessionCompletionRequestAsync(orchestrationRequest);
            }

            // Retrieve the messages from the chat session.
            var messages = await coreAPITestManager.GetChatSessionMessagesAsync(sessionId);

            // Delete the session to clean up after the test.
            if (sessionCreated)
            {
                await coreAPITestManager.DeleteSessionAsync(sessionId);
            }

            return messages;
        }

        /// <inheritdoc/>
        public async Task<Completion> RunAgentCompletionWithSession(string agentName,
            string userPrompt, string? sessionId = null)
        {
            var sessionCreated = false;
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                // Create a new session since an existing ID was not provided.
                sessionId = await coreAPITestManager.CreateSessionAsync();
                sessionCreated = true;
            }

            // TODO: Create a new agent if it does not exist. Use the ManagementAPITestManager to create the agent.

            // Create a new orchestration request for the user prompt and chat session.
            var orchestrationRequest = new OrchestrationRequest
            {
                SessionId = sessionId,
                AgentName = agentName,
                UserPrompt = userPrompt,
                Settings = null
            };

            // Send the orchestration request to the Core API's session completion endpoint.
            var completion = await coreAPITestManager.SendSessionCompletionRequestAsync(orchestrationRequest);

            // Delete the session to clean up after the test.
            if (sessionCreated)
            {
                await coreAPITestManager.DeleteSessionAsync(sessionId);
            }

            return completion;
        }

        /// <inheritdoc/>
        public async Task<Completion> RunAgentCompletionWithNoSession(string agentName,
            string userPrompt)
        {
            // TODO: Create a new agent if it does not exist. Use the ManagementAPITestManager to create the agent.

            // Create a new orchestration request for the user prompt and chat session.
            var completionRequest = new CompletionRequest
            {
                AgentName = agentName,
                UserPrompt = userPrompt,
                Settings = null
            };

            // Send the orchestration request to the Core API's orchestration completion endpoint.
            var completion = await coreAPITestManager.SendOrchestrationCompletionRequestAsync(completionRequest);

            return completion;
        }

        /// <inheritdoc/>
        public async Task<CompletionQualityMeasurementOutput> RunAgentCompletionWithQualityMeasurements(string agentName,
            string userPrompt, string expectedCompletion, string? sessionId = null)
        {
            var sessionCreated = false;
            var completionQualityMeasurementOutput = new CompletionQualityMeasurementOutput();
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                // Create a new session since an existing ID was not provided.
                sessionId = await coreAPITestManager.CreateSessionAsync();
                sessionCreated = true;
            }

            // TODO: Create a new agent if it does not exist. Use the ManagementAPITestManager to create the agent.

            // Create a new orchestration request for the user prompt and chat session.
            var orchestrationRequest = new OrchestrationRequest
            {
                SessionId = sessionId,
                AgentName = agentName,
                UserPrompt = userPrompt,
                Settings = null
            };

            // Send the orchestration request to the Core API's session completion endpoint.
            var completionResponse = await coreAPITestManager.SendSessionCompletionRequestAsync(orchestrationRequest);

            // Retrieve the messages from the chat session.
            var messages = await coreAPITestManager.GetChatSessionMessagesAsync(sessionId);

            // Get the last message where the agent is the sender.
            var lastAgentMessage = messages.LastOrDefault(m => m.Sender == nameof(Participants.Assistant));
            if (lastAgentMessage != null && !string.IsNullOrWhiteSpace(lastAgentMessage.CompletionPromptId))
            {
                // Get the completion prompt from the last agent message.
                var completionPrompt = await coreAPITestManager.GetCompletionPromptAsync(sessionId,
                    lastAgentMessage.CompletionPromptId);
                // For the context, take everything in the prompt that comes after `\\n\\nContext:\\n`. If it doesn't exist, take the whole prompt.
                var contextIndex =
                    completionPrompt.Prompt.IndexOf(@"\n\nContext:\n", StringComparison.Ordinal);
                if (contextIndex != -1)
                {
                    completionPrompt.Prompt = completionPrompt.Prompt[(contextIndex + 14)..];
                }

                var dataSet = new InputsMapping
                {
                    Question = userPrompt,
                    Answer = completionResponse?.Text,
                    Context = completionPrompt.Prompt,
                    GroundTruth = expectedCompletion,
                };
                // Create a new Azure AI evaluation from the data.
                var dataSetName = $"{agentName}_{sessionId}";
                var dataSetPath = await azureAIService.CreateDataSet(dataSet, dataSetName);
                var dataSetVersion = await azureAIService.CreateDataSetVersion(dataSetName, dataSetPath);
                _ = int.TryParse(dataSetVersion.DataVersion.VersionId, out var dataSetVersionNumber);
                var jobId = await azureAIService.SubmitJob(dataSetName, dataSetName,
                    dataSetVersionNumber == 0 ? 1 : dataSetVersionNumber,
                    string.Empty);

                completionQualityMeasurementOutput.JobID = jobId;
                completionQualityMeasurementOutput.UserPrompt = userPrompt;
                completionQualityMeasurementOutput.AgentCompletion = completionResponse?.Text;
                completionQualityMeasurementOutput.ExpectedCompletion = expectedCompletion;
            }

            // Delete the session to clean up after the test.
            if (sessionCreated)
            {
                await coreAPITestManager.DeleteSessionAsync(sessionId);
            }

            return completionQualityMeasurementOutput;
        }
    }
}
