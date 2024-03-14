﻿using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Events;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Events;
using FoundationaLLM.Common.Models.ResourceProvider;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Services.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Text.Json;

namespace FoundationaLLM.Common.Services.ResourceProviders
{
    /// <summary>
    /// Implements basic resource provider functionality
    /// </summary>
    public class ResourceProviderServiceBase : IResourceProviderService
    {
        private bool _isInitialized = false;

        private LocalEventService? _localEventService;
        private readonly List<string>? _eventNamespacesToSubscribe;
        private readonly ImmutableList<string> _allowedResourceProviders;
        private readonly Dictionary<string, ResourceTypeDescriptor> _allowedResourceTypes;

        /// <summary>
        /// The <see cref="IAuthorizationService"/> providing authorization services to the resource provider.
        /// </summary>
        protected readonly IAuthorizationService _authorizationService;

        /// <summary>
        /// The <see cref="IStorageService"/> providing storage services to the resource provider.
        /// </summary>
        protected readonly IStorageService _storageService;

        /// <summary>
        /// The <see cref="IEventService"/> providing event services to the resource provider.
        /// </summary>
        protected readonly IEventService _eventService;

        /// <summary>
        /// The <see cref="IResourceValidatorFactory"/> providing services to instantiate resource validators.
        /// </summary>
        protected readonly IResourceValidatorFactory _resourceValidatorFactory;

        /// <summary>
        /// The logger used for logging.
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// The <see cref="InstanceSettings"/> that provides instance-wide settings.
        /// </summary>
        protected readonly InstanceSettings _instanceSettings;

        /// <summary>
        /// The name of the storage container name used by the resource provider to store its internal data.
        /// </summary>
        protected virtual string _storageContainerName => "resource-provider";

        /// <summary>
        /// The name of the resource provider. Must be overridden in derived classes.
        /// </summary>
        protected virtual string _name => throw new NotImplementedException();

        /// <summary>
        /// Default JSON serialization settings.
        /// </summary>
        protected virtual JsonSerializerOptions _serializerSettings => new()
        {
            WriteIndented = true
        };

        /// <inheritdoc/>
        public string Name => _name;

        /// <inheritdoc/>
        public bool IsInitialized  => _isInitialized;

        /// <summary>
        /// Creates a new instance of the resource provider.
        /// </summary>
        /// <param name="instanceSettings">The <see cref="InstanceSettings"/> that provides instance-wide settings.</param>
        /// <param name="authorizationService">The <see cref="IAuthorizationService"/> providing authorization services to the resource provider.</param>
        /// <param name="storageService">The <see cref="IStorageService"/> providing storage services to the resource provider.</param>
        /// <param name="eventService">The <see cref="IEventService"/> providing event services to the resource provider.</param>
        /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing services to instantiate resource validators.</param>
        /// <param name="logger">The logger used for logging.</param>
        /// <param name="eventNamespacesToSubscribe">The list of Event Service event namespaces to subscribe to for local event processing.</param>
        public ResourceProviderServiceBase(
            InstanceSettings instanceSettings,
            IAuthorizationService authorizationService,
            IStorageService storageService,
            IEventService eventService,
            IResourceValidatorFactory resourceValidatorFactory,
            ILogger logger,
            List<string>? eventNamespacesToSubscribe = default)
        {
            _authorizationService = authorizationService;
            _storageService = storageService;
            _eventService = eventService;
            _resourceValidatorFactory = resourceValidatorFactory;
            _logger = logger;
            _instanceSettings = instanceSettings;
            _eventNamespacesToSubscribe = eventNamespacesToSubscribe;

            _allowedResourceProviders = [_name];
            _allowedResourceTypes = GetResourceTypes();

            // Kicks off the initialization on a separate thread and does not wait for it to complete.
            // The completion of the initialization process will be signaled by setting the _isInitialized property.
            _ = Task.Run(Initialize);
        }

        #region Initialization

        /// <inheritdoc/>
        private async Task Initialize()
        {
            try
            {
                await InitializeInternal();

                if (_eventNamespacesToSubscribe != null
                    && _eventNamespacesToSubscribe.Count > 0)
                {
                    _localEventService = new LocalEventService(
                        new LocalEventServiceSettings { EventProcessingCycleSeconds = 10 },
                        _eventService,
                        _logger);
                    _localEventService.SubscribeToEventNamespaces(_eventNamespacesToSubscribe);
                    _localEventService.StartLocalEventProcessing(HandleEvents);
                }

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The resource provider {ResourceProviderName} failed to initialize.", _name);
            }
        }

        #region Virtuals to override in derived classes

        /// <summary>
        /// The internal implementation of Initialize. Must be overridden in derived classes.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task InitializeInternal()
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the details about the resource types managed by the resource provider.
        /// </summary>
        /// <returns>A dictionary of <see cref="ResourceTypeDescriptor"/> objects with details about the resource types.</returns>
        protected virtual Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() => [];

        #endregion

        #endregion

        #region IManagementProviderService

        /// <inheritdoc/>
        public async Task<object> HandleGetAsync(string resourcePath, UnifiedUserIdentity? userIdentity)
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
             var parsedResourcePath = new ResourcePath(
                resourcePath,
                _allowedResourceProviders,
                _allowedResourceTypes,
                allowAction: false);

            // Authorize access to the resource path.
            await Authorize(parsedResourcePath, userIdentity, "read");

            return await GetResourcesAsyncInternal(parsedResourcePath);
        }

        /// <inheritdoc/>
        public async Task<object> HandlePostAsync(string resourcePath, string serializedResource, UnifiedUserIdentity? userIdentity)
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var parsedResourcePath = new ResourcePath(
                resourcePath,
                _allowedResourceProviders,
                _allowedResourceTypes);

            // Authorize access to the resource path.
            await Authorize(parsedResourcePath, userIdentity, "write");

            if (parsedResourcePath.ResourceTypeInstances.Last().Action != null)
                return await ExecuteActionAsync(parsedResourcePath, serializedResource);
            else
                return await UpsertResourceAsync(parsedResourcePath, serializedResource);
        }

        /// <inheritdoc/>
        public async Task HandleDeleteAsync(string resourcePath, UnifiedUserIdentity? userIdentity)
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var parsedResourcePath = new ResourcePath(
                resourcePath,
                _allowedResourceProviders,
                _allowedResourceTypes,
                allowAction: false);

            // Authorize access to the resource path.
            await Authorize(parsedResourcePath, userIdentity, "delete");

            await DeleteResourceAsync(parsedResourcePath);
        }

        #region Virtuals to override in derived classes

        /// <summary>
        /// The internal implementation of GetResourcesAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="resourcePath">A <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <returns></returns>
        protected virtual async Task<object> GetResourcesAsyncInternal(ResourcePath resourcePath)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of UpsertResourceAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="resourcePath">A <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <param name="serializedResource">The serialized resource being created or updated.</param>
        /// <returns></returns>
        protected virtual async Task<object> UpsertResourceAsync(ResourcePath resourcePath, string serializedResource)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of ExecuteActionAsync. Must be overriden in derived classes.
        /// </summary>
        /// <param name="resourcePath">A <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <param name="serializedAction">The serialized details of the action being executed.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected virtual async Task<object> ExecuteActionAsync(ResourcePath resourcePath, string serializedAction)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of DeleteResourceAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="resourcePath">A <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <returns></returns>
        protected virtual async Task DeleteResourceAsync(ResourcePath resourcePath)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region IResourceProviderService

        /// <inheritdoc/>
        public T GetResource<T>(string resourcePath) where T : class
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var parsedResourcePath = new ResourcePath(resourcePath, _allowedResourceProviders, _allowedResourceTypes);
            return GetResourceInternal<T>(parsedResourcePath);
        }

        /// <inheritdoc/>
        public async Task<string> UpsertResourceAsync<T>(string resourcePath, T resource) where T : class
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var parsedResourcePath = new ResourcePath(resourcePath, _allowedResourceProviders, _allowedResourceTypes);
            await UpsertResourceAsync<T>(parsedResourcePath, resource);
            return parsedResourcePath.GetObjectId(_instanceSettings.Id, _name);
        }

        #region Virtuals to override in derived classes

        /// <summary>
        /// The internal implementation of GetResource. Must be overridden in derived classes.
        /// </summary>
        /// <param name="resourcePath">A <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <returns></returns>
        protected virtual T GetResourceInternal<T>(ResourcePath resourcePath) where T : class =>
            throw new NotImplementedException();

        /// <summary>
        /// The internal implementation of UpsertResourceAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="resourcePath">A <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <param name="resource">The instance of the resource being created or updated.</param>
        /// <returns></returns>
        protected virtual async Task UpsertResourceAsync<T>(ResourcePath resourcePath, T resource)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region Authorization

        /// <summary>
        /// Authorizes the specified action on a resource path.
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> containing information about the identity of the user.</param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        /// <exception cref="ResourceProviderException"></exception>
        private async Task Authorize(ResourcePath resourcePath, UnifiedUserIdentity? userIdentity, string actionType)
        {
            try
            {
                if (userIdentity == null
                    || userIdentity.UserId == null)
                    throw new Exception("The provided user identity information cannot be used for authorization.");

                var result = await _authorizationService.ProcessAuthorizationRequest(new ActionAuthorizationRequest
                {
                    Action = $"{_name}/{resourcePath.MainResourceType}/{actionType}",
                    ResourcePath = resourcePath.GetObjectId(_instanceSettings.Id, _name),
                    PrincipalId = userIdentity.UserId,
                    SecurityGroupIds = userIdentity.GroupIds
                });

                if (!result.Authorized)
                    throw new AuthorizationException("Access is not authorized.");
            }
            catch (AuthorizationException)
            {
                _logger.LogWarning("The {ActionType} access to the resource path {ResourcePath} was not authorized for user {UserName}.",
                    actionType, resourcePath.GetObjectId(_instanceSettings.Id, _name), userIdentity!.Username);
                throw new ResourceProviderException("Access is not authorized.", StatusCodes.Status403Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while attempting to authorize access to the resource path.");
                throw new ResourceProviderException(
                    "An error occurred while attempting to authorize access to the resource path.",
                    StatusCodes.Status403Forbidden);
            }
        }

        #endregion

        #region Events handling

        /// <summary>
        /// Handles events received from the <see cref="IEventService"/> when they are dequeued locally.
        /// </summary>
        /// <param name="e">The <see cref="EventSetEventArgs"/> containing the events namespace and the actual events.</param>
        /// <returns></returns>
        protected virtual async Task HandleEvents(EventSetEventArgs e) =>
            await Task.CompletedTask;

        #endregion
    }
}
