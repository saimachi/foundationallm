﻿using Azure.Identity;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.SemanticKernel.Core.Models.Configuration;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using System.ComponentModel;
using System.Net;

#pragma warning disable SKEXP0001, SKEXP0010

namespace FoundationaLLM.SemanticKernel.Core.Services
{
    /// <summary>
    /// Generates text embeddings using the Semantic Kernel orchestrator.
    /// </summary>
    public class SemanticKernelTextEmbeddingService : ITextEmbeddingService
    {
        private readonly SemanticKernelTextEmbeddingServiceSettings _settings;
        private readonly ILogger _logger;
        private readonly Kernel _kernel;
        private readonly ITextEmbeddingGenerationService _textEmbeddingService;

        /// <summary>
        /// Creates a new <see cref="SemanticKernelTextEmbeddingService"/> instance.
        /// </summary>
        /// <param name="options">The <see cref="IOptions{TOptions}"/> providing configuration settings.</param>
        /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
        public SemanticKernelTextEmbeddingService(
            IOptions<SemanticKernelTextEmbeddingServiceSettings> options,
            ILogger<SemanticKernelTextEmbeddingService> logger)
        {
            _settings = options.Value;
            _logger = logger;
            _kernel = CreateKernel();
            _textEmbeddingService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
        }

        /// <inheritdoc/>
        public async Task<TextEmbeddingResult> GetEmbeddingsAsync(IList<TextChunk> textChunks, string modelName = "text-embedding-ada-002")
        {
            try
            {
                var embeddings = await _textEmbeddingService.GenerateEmbeddingsAsync(textChunks.Select(tc => tc.Content!).ToList());
                return new TextEmbeddingResult
                {
                    InProgress = false,
                    TextChunks = textChunks.Select(tc =>
                    {
                        tc.Embedding = new Embedding(embeddings[tc.Position - 1]);
                        return tc;
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating embeddings.");
                return new TextEmbeddingResult
                {
                    InProgress = false,
                    Failed = true,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <inheritdoc/>
        public Task<TextEmbeddingResult> GetEmbeddingsAsync(string operationId) => throw new NotImplementedException();

        private Kernel CreateKernel()
        {
            ValidateDeploymentName(_settings.DeploymentName);
            ValidateEndpoint(_settings.Endpoint);

            var builder = Kernel.CreateBuilder();
            if (_settings.AuthenticationType == AzureOpenAIAuthenticationTypes.AzureIdentity)
            {
                builder.AddAzureOpenAITextEmbeddingGeneration(
                    _settings.DeploymentName,
                    _settings.Endpoint,
                    DefaultAuthentication.GetAzureCredential());
            }
            else
            {
                ValidateAPIKey(_settings.APIKey);
                builder.AddAzureOpenAITextEmbeddingGeneration(
                    _settings.DeploymentName,
                    _settings.Endpoint,
                    _settings.APIKey!);
            }

            builder.Services.ConfigureHttpClientDefaults(c =>
            {
                // Use a standard resiliency policy configured to retry on 429 (too many requests).
                c.AddStandardResilienceHandler().Configure(o =>
                {
                    o.Retry.ShouldHandle = args => ValueTask.FromResult(args.Outcome.Result?.StatusCode is HttpStatusCode.TooManyRequests);
                });
            });

            return builder.Build();
        }

        private void ValidateDeploymentName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _logger.LogCritical("The Azure Open AI deployment name is invalid.");
                throw new ConfigurationValueException("The Azure Open AI deployment name is invalid.");
            }
        }

        private void ValidateEndpoint(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _logger.LogCritical("The Azure Open AI endpoint is invalid.");
                throw new ConfigurationValueException("The Azure Open AI endpoint is invalid.");
            }
        }
        private void ValidateAPIKey(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _logger.LogCritical("The Azure Open AI API key is invalid.");
                throw new ConfigurationValueException("The Azure Open AI API key is invalid.");
            }
        }
    }
}
