﻿namespace FoundationaLLM.Common.Constants;

/// <summary>
/// Name constants used to configure and retrieve an <see cref="T:System.Net.Http.HttpClient" />,
/// using <see cref="T:System.Net.Http.IHttpClientFactory" />.
/// </summary>
public static class HttpClients
{
    /// <summary>
    /// Named client with matching configuration for the Core API.
    /// </summary>
    public const string CoreAPI = "CoreAPI";
    /// <summary>
    /// Named client with matching configuration for the Gatekeeper API.
    /// </summary>
    public const string GatekeeperAPI = "GatekeeperAPI";
    /// <summary>
    /// Named client with matching configuration for the Gatekeeper Integration API.
    /// </summary>
    public const string GatekeeperIntegrationAPI = "GatekeeperIntegrationAPI";
    /// <summary>
    /// Named client with matching configuration for the Orchestration API.
    /// </summary>
    public const string OrchestrationAPI = "OrchestrationAPI";
    /// <summary>
    /// Named client with matching configuration for the LangChain API.
    /// </summary>
    public const string LangChainAPI = "LangChainAPI";
    /// <summary>
    /// Named client with matching configuration for the Semantic Kernel API.
    /// </summary>
    public const string SemanticKernelAPI = "SemanticKernelAPI";
    /// <summary>
    /// Named client with matching configuration for the Agent Hub API.
    /// </summary>
    public const string AgentHubAPI = "AgentHubAPI";
    /// <summary>
    /// Named client with matching configuration for the Prompt Hub API.
    /// </summary>
    public const string PromptHubAPI = "PromptHubAPI";
    /// <summary>
    /// Named client with matching configuration for the DataSource Hub API.
    /// </summary>
    public const string DataSourceHubAPI = "DataSourceHubAPI";

    /// <summary>
    /// Named client with matching configuration for the Vectorization API.
    /// </summary>
    public const string VectorizationAPI = "VectorizationAPI";

    /// <summary>
    /// Named client with matching configuration for a direct connection to Azure AI.
    /// </summary>
    public const string AzureAIDirect = "AzureAIDirect";

    /// <summary>
    /// Named client with matching configuration for a direct connection to Azure AI.
    /// </summary>
    public const string AzureOpenAIDirect = "AzureOpenAIDirect";

    /// <summary>
    /// Named client with matching configuration for the Authorization API.
    /// </summary>
    public const string AuthorizationAPI = "AuthorizationAPI";

    /// <summary>
    /// Named client with matching configuration for the Management API.
    /// </summary>
    public const string ManagementAPI = "ManagementAPI";
}
