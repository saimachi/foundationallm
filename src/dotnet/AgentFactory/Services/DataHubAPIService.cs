﻿using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.AgentFactory.Models.Orchestration;
using FoundationaLLM.AgentFactory.Services;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Settings;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http;
using System.Runtime;
using System.Text;
using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.AgentFactory.Core.Services;

public class DataSourceHubAPIService : IDataSourceHubService
{
    readonly DataSourceHubSettings _settings;
    readonly ILogger<DataSourceHubAPIService> _logger;
    private readonly IHttpClientFactoryService _httpClientFactoryService;
    readonly JsonSerializerSettings _jsonSerializerSettings;

    public DataSourceHubAPIService(
            IOptions<DataSourceHubSettings> options,
            ILogger<DataSourceHubAPIService> logger,
            IHttpClientFactoryService httpClientFactoryService)
    {
        _settings = options.Value;
        _logger = logger;
        _httpClientFactoryService = httpClientFactoryService;
    }

    public async Task<string> Status()
    {
        try
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.DataSourceHubAPI);

            var responseMessage = await client.GetAsync("/status");

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                return responseContent;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting agent hub status.");
            throw ex;
        }

        return null;
    }

    public async Task<DataSourceHubResponse> ResolveRequest(List<string> sources, string userContext)
    {
        try
        {
            DataSourceHubMessage phm = new DataSourceHubMessage { DataSources =  sources };

            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.DataSourceHubAPI);

            var responseMessage = await client.PostAsync("/resolve_request", new StringContent(
                    JsonConvert.SerializeObject(phm),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var dshr = JsonConvert.DeserializeObject<DataSourceHubResponse>(responseContent, _jsonSerializerSettings);
                
                return dshr;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error resolving request for Agent Hub.");
            throw ex;
        }

        return new DataSourceHubResponse();
    }

}