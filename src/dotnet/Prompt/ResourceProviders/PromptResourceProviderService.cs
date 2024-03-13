﻿using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProvider;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.Prompt.Constants;
using FoundationaLLM.Prompt.Models.Metadata;
using FoundationaLLM.Prompt.Models.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Prompt.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Prompt resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationService"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class PromptResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationService authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Prompt)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        ILogger<PromptResourceProviderService> logger)
        : ResourceProviderServiceBase(
            instanceOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            logger)
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            PromptResourceProviderMetadata.AllowedResourceTypes;

        private ConcurrentDictionary<string, PromptReference> _promptReferences = [];

        private const string PROMPT_REFERENCES_FILE_NAME = "_prompt-references.json";
        private const string PROMPT_REFERENCES_FILE_PATH = $"/{ResourceProviderNames.FoundationaLLM_Prompt}/{PROMPT_REFERENCES_FILE_NAME}";

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Prompt;

        /// <inheritdoc/>
        protected override async Task InitializeInternal()
        {
            _logger.LogInformation("Starting to initialize the {ResourceProvider} resource provider...", _name);

            if (await _storageService.FileExistsAsync(_storageContainerName, PROMPT_REFERENCES_FILE_PATH, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, PROMPT_REFERENCES_FILE_PATH, default);
                var promptReferenceStore = JsonSerializer.Deserialize<PromptReferenceStore>(
                    Encoding.UTF8.GetString(fileContent.ToArray()));

                _promptReferences = new ConcurrentDictionary<string, PromptReference>(
                    promptReferenceStore!.ToDictionary());
            }
            else
            {
                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    PROMPT_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(new PromptReferenceStore { PromptReferences = [] }),
                    default,
                    default);
            }

            _logger.LogInformation("The {ResourceProvider} resource provider was successfully initialized.", _name);
        }

        #region Support for Management API

        /// <inheritdoc/>
        protected override async Task<object> GetResourcesAsyncInternal(ResourcePath resourcePath) =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                PromptResourceTypeNames.Prompts => await LoadPrompts(resourcePath.ResourceTypeInstances[0]),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for GetResourcesAsyncInternal

        private async Task<List<PromptBase>> LoadPrompts(ResourceTypeInstance instance)
        {
            if (instance.ResourceId == null)
            {
                return
                [
                    .. (await Task.WhenAll(
                        _promptReferences.Values
                            .Where(pr => !pr.Deleted)
                            .Select(pr => LoadPrompt(pr))))
                ];
            }
            else
            {
                if (!_promptReferences.TryGetValue(instance.ResourceId, out var promptReference)
                    || promptReference.Deleted)
                    throw new ResourceProviderException($"Could not locate the {instance.ResourceId} prompt resource.",
                        StatusCodes.Status404NotFound);

                var prompt = await LoadPrompt(promptReference!);

                return [prompt];
            }
        }

        private async Task<MultipartPrompt> LoadPrompt(PromptReference promptReference)
        {
            if (await _storageService.FileExistsAsync(_storageContainerName, promptReference.Filename, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, promptReference.Filename, default);
                return JsonSerializer.Deserialize(
                    Encoding.UTF8.GetString(fileContent.ToArray()),
                    promptReference.PromptType,
                    _serializerSettings) as Models.Metadata.MultipartPrompt
                    ?? throw new ResourceProviderException($"Failed to load the prompt {promptReference.Name}.");
            }

            throw new ResourceProviderException($"Could not locate the {promptReference.Name} prompt resource.",
                StatusCodes.Status404NotFound);
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task<object> UpsertResourceAsync(ResourcePath resourcePath, string serializedResource) =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                PromptResourceTypeNames.Prompts => await UpdatePrompt(resourcePath, serializedResource),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest),
            };

        #region Helpers for UpsertResourceAsync

        private async Task<ResourceProviderUpsertResult> UpdatePrompt(ResourcePath resourcePath, string serializedPrompt)
        {
            var promptBase = JsonSerializer.Deserialize<PromptBase>(serializedPrompt)
                ?? throw new ResourceProviderException("The object definition is invalid.");

            if (_promptReferences.TryGetValue(promptBase.Name!, out var existingPromptReference)
                && existingPromptReference!.Deleted)
                throw new ResourceProviderException($"The prompt resource {existingPromptReference.Name} cannot be added or updated.",
                        StatusCodes.Status400BadRequest);

            if (resourcePath.ResourceTypeInstances[0].ResourceId != promptBase.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var promptReference = new PromptReference
            {
                Name = promptBase.Name!,
                Type = promptBase.Type!,
                Filename = $"/{_name}/{promptBase.Name}.json",
                Deleted = false
            };

            var prompt = JsonSerializer.Deserialize(serializedPrompt, promptReference.PromptType, _serializerSettings);
            (prompt as PromptBase)!.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);

            await _storageService.WriteFileAsync(
                _storageContainerName,
                promptReference.Filename,
                JsonSerializer.Serialize(prompt, promptReference.PromptType, _serializerSettings),
                default,
                default);

            _promptReferences.AddOrUpdate(promptReference.Name, promptReference, (k, v) => v);

            await _storageService.WriteFileAsync(
                    _storageContainerName,
                    PROMPT_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(PromptReferenceStore.FromDictionary(_promptReferences.ToDictionary())),
                    default,
                    default);

            return new ResourceProviderUpsertResult
            {
                ObjectId = (prompt as PromptBase)!.ObjectId
            };
        }

        #endregion

        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<object> ExecuteActionAsync(ResourcePath resourcePath, string serializedAction) =>
            resourcePath.ResourceTypeInstances.Last().ResourceType switch
            {
                PromptResourceTypeNames.Prompts => resourcePath.ResourceTypeInstances.Last().Action switch
                {
                    PromptResourceProviderActions.CheckName => CheckPromptName(serializedAction),
                    _ => throw new ResourceProviderException($"The action {resourcePath.ResourceTypeInstances.Last().Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                _ => throw new ResourceProviderException()
            };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        #region Helpers for ExecuteActionAsync

        private ResourceNameCheckResult CheckPromptName(string serializedAction)
        {
            var resourceName = JsonSerializer.Deserialize<ResourceName>(serializedAction);
            return _promptReferences.Values.Any(ar => ar.Name == resourceName!.Name)
                ? new ResourceNameCheckResult
                {
                    Name = resourceName!.Name,
                    Type = resourceName.Type,
                    Status = NameCheckResultType.Denied,
                    Message = "A resource with the specified name already exists or was previously deleted and not purged."
                }
                : new ResourceNameCheckResult
                {
                    Name = resourceName!.Name,
                    Type = resourceName.Type,
                    Status = NameCheckResultType.Allowed
                };
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath)
        {
            switch (resourcePath.ResourceTypeInstances.Last().ResourceType)
            {
                case PromptResourceTypeNames.Prompts:
                    await DeletePrompt(resourcePath.ResourceTypeInstances);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances.Last().ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
        }

        #region Helpers for DeleteResourceAsync

        private async Task DeletePrompt(List<ResourceTypeInstance> instances)
        {
            if (_promptReferences.TryGetValue(instances.Last().ResourceId!, out var promptReference)
                || promptReference!.Deleted)
            {
                promptReference.Deleted = true;

                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    PROMPT_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(PromptReferenceStore.FromDictionary(_promptReferences.ToDictionary())),
                    default,
                    default);
            }
            else
                throw new ResourceProviderException($"Could not locate the {instances.Last().ResourceId} agent resource.",
                            StatusCodes.Status404NotFound);
        }

        #endregion

        #endregion
    }
}
