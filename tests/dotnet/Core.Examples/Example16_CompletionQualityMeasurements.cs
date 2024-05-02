﻿using FoundationaLLM.Core.Examples.Interfaces;
using FoundationaLLM.Core.Examples.Models;
using FoundationaLLM.Core.Examples.Setup;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples
{
    /// <summary>
    /// Example class for running agent completions and evaluating the quality of the completions using Azure AI Studio.
    /// </summary>
    public class Example16_CompletionQualityMeasurements : BaseTest, IClassFixture<TestFixture>
	{
		private readonly IAgentConversationTestService _agentConversationTestService;

		public Example16_CompletionQualityMeasurements(ITestOutputHelper output, TestFixture fixture)
			: base(output, fixture.ServiceProvider)
		{
            _agentConversationTestService = GetService<IAgentConversationTestService>();
		}

		[Fact]
		public async Task RunAsync()
		{
			WriteLine("============ Agent Completions ============");
			await RunExampleAsync();
		}

		private async Task RunExampleAsync()
		{
			var agentPrompts = TestConfiguration.CompletionQualityMeasurementConfiguration.AgentPrompts;
			if (agentPrompts == null || agentPrompts.Length == 0)
			{
				WriteLine("No agent prompts found. Make sure you enter them in testsettings.json.");
				return;
			}
			foreach (var agentPrompt in agentPrompts)
			{
				await RunAgentCompletionAsync(agentPrompt);
			}
		}

		private async Task RunAgentCompletionAsync(AgentPrompt agentPrompt)
        {
            if (string.IsNullOrWhiteSpace(agentPrompt.AgentName))
            {
                throw new InvalidOperationException("The agent name is required.");
            }
            if (string.IsNullOrWhiteSpace(agentPrompt.UserPrompt))
            {
                throw new InvalidOperationException("The user prompt is required.");
            }
            if (string.IsNullOrWhiteSpace(agentPrompt.ExpectedCompletion))
            {
                throw new InvalidOperationException("The expected completion is required.");
            }
            WriteLine($"Agent: {agentPrompt.AgentName}");

            try
            {
                var output = await _agentConversationTestService.RunAgentCompletionWithQualityMeasurements(agentPrompt.AgentName,
                    agentPrompt.UserPrompt, agentPrompt.ExpectedCompletion, agentPrompt.SessionConfiguration?.SessionId ?? null);

                WriteLine($"Azure AI evaluation Job ID -> {output.JobID}");

                WriteLine($"User prompt -> '{output.UserPrompt}'");
                WriteLine($"Agent completion -> '{output.AgentCompletion}'");
                WriteLine($"Expected completion -> '{output.ExpectedCompletion}'");
                WriteLine("-------------------------------");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
	}
}
