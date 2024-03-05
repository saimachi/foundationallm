﻿using FoundationaLLM.AgentFactory.Core.Orchestration;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Tests.Orchestration
{
    public class KnowledgeManagementOrchestrationTests
    {
        private KnowledgeManagementOrchestration _knowledgeManagementOrchestration;
        private KnowledgeManagementAgent _agent = new KnowledgeManagementAgent() { Name = "Test_agent", ObjectId="Test_objctid", Type = AgentTypes.KnowledgeManagement };
        private ICacheService _cacheService = Substitute.For<ICacheService>();
        private ICallContext _callContext = Substitute.For<ICallContext>();
        private ILLMOrchestrationService _orchestrationService = Substitute.For<ILLMOrchestrationService>();
        private IPromptHubAPIService _promptHubService = Substitute.For<IPromptHubAPIService>();
        private IDataSourceHubAPIService _dataSourceHubService = Substitute.For<IDataSourceHubAPIService>();
        private ILogger<OrchestrationBase> _logger = Substitute.For<ILogger<OrchestrationBase>>();

        public KnowledgeManagementOrchestrationTests()
        {
            _knowledgeManagementOrchestration = new KnowledgeManagementOrchestration(
                _agent,
                _cacheService,
                _callContext,
                _orchestrationService,
                _promptHubService,
                _dataSourceHubService,
                _logger);
        }

        [Fact]
        public async Task GetCompletion_ReturnsCompletionResponse()
        {
            // Arrange
            var completionRequest = new CompletionRequest() { UserPrompt = "Test_userprompt"};
            var orchestrationResult = new LLMCompletionResponse { Completion = "Completion" };
            _orchestrationService.GetCompletion(Arg.Any<KnowledgeManagementCompletionRequest>())
                .Returns(Task.FromResult(orchestrationResult));

            // Act
            var completionResponse = await _knowledgeManagementOrchestration.GetCompletion(completionRequest);

            // Assert
            Assert.Equal(orchestrationResult.Completion, completionResponse.Completion);
            Assert.Equal(completionRequest.UserPrompt, completionResponse.UserPrompt);
            Assert.Equal(orchestrationResult.FullPrompt, completionResponse.FullPrompt);
            Assert.Equal(orchestrationResult.PromptTemplate, completionResponse.PromptTemplate);
            Assert.Equal(orchestrationResult.AgentName, completionResponse.AgentName);
            Assert.Equal(orchestrationResult.PromptTokens, completionResponse.PromptTokens);
            Assert.Equal(orchestrationResult.CompletionTokens, completionResponse.CompletionTokens);
        }
    }
}
