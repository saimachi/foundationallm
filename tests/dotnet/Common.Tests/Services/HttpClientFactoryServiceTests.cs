﻿using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Services;
using NSubstitute;
using System.Text.Json;

namespace FoundationaLLM.Common.Tests.Services
{
    public class HttpClientFactoryServiceTests
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICallContext _callContext;
        private readonly IDownstreamAPISettings _apiSettings;

        public HttpClientFactoryServiceTests()
        {
            _httpClientFactory = Substitute.For<IHttpClientFactory>();
            _callContext = Substitute.For<ICallContext>();
            _apiSettings = Substitute.For<IDownstreamAPISettings>();
        }

        [Fact]
        public void CreateClient_WithValidClientName_ReturnsHttpClientWithHeaders()
        {
            // Arrange
            var clientName = "TestClient";
            var apiKey = "TestAPIKey";
            var userContext = new UnifiedUserIdentity
            {
                UPN = "TestUPN",
                Username = "TestUsername",
                Name = "TestName"
            };

            _apiSettings.DownstreamAPIs.Returns(new System.Collections.Generic.Dictionary<string, DownstreamAPIKeySettings>
            {
                { 
                    clientName, new DownstreamAPIKeySettings
                    {
                        APIUrl = "TestAPIUrl",
                        APIKey = "TestAPIKey"
                    }
                }
            });

            _callContext.CurrentUserIdentity.Returns(userContext);

            var httpClient = new HttpClient();
            _httpClientFactory.CreateClient(clientName).Returns(httpClient);

            var service = new HttpClientFactoryService(_httpClientFactory, _callContext, _apiSettings);

            // Act
            var result = service.CreateClient(clientName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(httpClient.Timeout, TimeSpan.FromSeconds(600));
            Assert.Equal(result.DefaultRequestHeaders.GetValues(Constants.HttpHeaders.APIKey), new[] { apiKey });
            Assert.Equal(result.DefaultRequestHeaders.GetValues(Constants.HttpHeaders.UserIdentity), new[] { JsonSerializer.Serialize(userContext) });
        }
    }
}
