﻿using FoundationaLLM.Orchestration.Core.Interfaces;
using FoundationaLLM.Orchestration.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FoundationaLLM
{
    /// <summary>
    /// General purpose dependency injection extensions.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Adds the Orchestration service to the dependency injection container.
        /// </summary>
        /// <param name="builder">The host application builder.</param>
        public static void AddOrchestrationService(this IHostApplicationBuilder builder) =>
            builder.Services.AddScoped<IOrchestrationService, OrchestrationService>();

        /// <summary>
        /// Adds all internal LLM orchestration services and the LLM orchestration service manager to the dependency injection container.
        /// </summary>
        /// <param name="builder"></param>
        public static void AddLLMOrchestrationServices(this IHostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<ILLMOrchestrationServiceManager, LLMOrchestrationServiceManager>();

            builder.Services.AddSingleton<ILLMOrchestrationService, SemanticKernelService>();
            builder.Services.AddSingleton<ILLMOrchestrationService, LangChainService>();
            builder.Services.AddSingleton<ILLMOrchestrationService, AzureAIDirectService>();
            builder.Services.AddSingleton<ILLMOrchestrationService, AzureOpenAIDirectService>();
        }
    }
}
