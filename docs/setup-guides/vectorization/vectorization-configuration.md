# Configuring vectorization

This section provides details on how to configure the vectorization API and workers in FoundationaLLM.

> [!NOTE]
> These configurations should already be in place if you deployed FoundationaLLM (FLLM) using the recommended deployment scripts.
> The detailes presented here are provided for cases in which you need to troubleshoot or customize the configuration.

## Configuration for Vectorization API

The following table describes the Azure artifacts required for the vectorization pipelines.

| Artifact name | Description |
| --- | --- |
| `vectorization-input` | Azure storage container used by default to store documents to be picked up by the vectorization pipeline. Must be created on a Data Lake storage account (with the hierarchical namespace enabled). |

The following table describes the environment variables required for the vectorization pipelines.

Environment variable | Description
--- | ---
`FoundationaLLM:AppConfig:ConnectionString` | Connection string to the Azure App Configuration instance.

The following table describes the required configuration parameters for the vectorization pipelines.

| App Configuration Key | Default Value | Description |
| --- | --- | --- |
| `FoundationaLLM:APIs:VectorizationAPI:APIUrl` | | The URL of the vectorization API. |
| `FoundationaLLM:APIs:VectorizationAPI:APIKey` | Key Vault secret name: `foundationallm-apis-vectorizationapi-apikey` | The API key of the vectorization API. |
| `FoundationaLLM:APIs:VectorizationAPI:AppInsightsConnectionString` | Key Vault secret name: `foundationallm-app-insights-connection-string` | The connection string to the Application Insights instance used by the vectorization API. |

> [!NOTE]
> Refer to the [App Configuration values](../../deployment/app-configuration-values.md) page for more information on how to set these and other configuration values.

## Configuration for Vectorization workers

The following table describes the Azure artifacts required for the vectorization pipelines.

| Artifact Name | Description |
| --- | --- |
| `embed` | Azure storage queue used for the embed vectorization pipeline. Can be created on the storage account used for the other queues. |
| `extract` | Azure storage queue used for the extract vectorization pipeline. Can be created on the storage account used for the other queues. |
| `index` | Azure storage queue used for the index vectorization pipeline. Can be created on the storage account used for the other queues. |
| `partition` | Azure storage queue used for the partition vectorization pipeline. Can be created on the storage account used for the other queues. |
| `vectorization-state` | Azure storage container used for the vectorization state service. Can be created on the storage account used for the other queues. |
| `resource-provider`| Azure storage container used for the internal states of the FoundationaLLM resource providers. |
| `resource-provider/FoundationaLLM.Vectorization/vectorization-content-source-profiles.json` | Azure storage blob used for the content sources managed by the `FoundationaLLM.Vectorization` resource provider. For more details, see [vectorization content source profiles structure](#vectorization-content-source-profiles-structure).
| `resource-provider/FoundationaLLM.Vectorization/vectorization-text-partitioning-profiles.json` | Azure storage blob used for the text partitioning profiles managed by the `FoundationaLLM.Vectorization` resource provider. For more details, see [vectorization text partitioning profiles structure](#vectorization-text-partitioning-profiles-structure).
| `resource-provider/FoundationaLLM.Vectorization/vectorization-text-embedding-profiles.json` | Azure storage blob used for the text embedding profiles managed by the `FoundationaLLM.Vectorization` resource provider. For more details, see [vectorization text embedding profiles structure](#vectorization-text-embedding-profiles-structure).
| `resource-provider/FoundationaLLM.Vectorization/vectorization-indexing-profiles.json` | Azure storage blob used for the indexing profiles managed by the `FoundationaLLM.Vectorization` resource provider. For more details, see [vectorization indexing profiles structure](#vectorization-indexing-profiles-structure).

The following table describes the environment variables required for the vectorization pipelines.

| Environment variable | Description |
| --- | --- |
| `FoundationaLLM:AppConfig:ConnectionString` | Connection string to the Azure App Configuration instance. |

The following table describes the required App Configuration parameters for the vectorization pipelines.

| App Configuration Key | Default Value | Description |
| --- | --- | --- |
| `FoundationaLLM:APIs:VectorizationWorker:APIUrl` | | The URL of the vectorization worker API. |
| `FoundationaLLM:APIs:VectorizationWorker:APIKey` | Key Vault secret name: `foundationallm-apis-vectorizationworker-apikey` | The API key of the vectorization worker API. |
| `FoundationaLLM:APIs:VectorizationWorker:AppInsightsConnectionString` | Key Vault secret name: `foundationallm-app-insights-connection-string` | The connection string to the Application Insights instance used by the vectorization worker API. |
| `FoundationaLLM:Vectorization:VectorizationWorker` | | The settings used by each instance of the vectorization worker service. For more details, see [default vectorization worker settings](#default-vectorization-worker-settings). |
| `FoundationaLLM:Vectorization:Queues:Embed:ConnectionString` | Key Vault secret name: `foundationallm-vectorization-queues-connectionstring` | The connection string to the Azure Storage account used for the embed vectorization queue. |
| `FoundationaLLM:Vectorization:Queues:Extract:ConnectionString` | Key Vault secret name: `foundationallm-vectorization-queues-connectionstring` | The connection string to the Azure Storage account used for the extract vectorization queue. |
| `FoundationaLLM:Vectorization:Queues:Index:ConnectionString` | Key Vault secret name: `foundationallm-vectorization-queues-connectionstring` | The connection string to the Azure Storage account used for the index vectorization queue. |
| `FoundationaLLM:Vectorization:Queues:Partition:ConnectionString` | Key Vault secret name: `foundationallm-vectorization-queues-connectionstring` | The connection string to the Azure Storage account used for the partition vectorization queue. |
| `FoundationaLLM:Vectorization:StateService:Storage:AuthenticationType` | | The authentication type used to connect to the underlying storage. Can be one of `AzureIdentity`, `AccountKey`, or `ConnectionString`. |
| `FoundationaLLM:Vectorization:StateService:Storage:ConnectionString` | Key Vault secret name: `foundationallm-vectorization-state-connectionstring` | The connection string to the Azure Storage account used for the vectorization state service. |
| `FoundationaLLM:Vectorization:ResourceProviderService:Storage:AuthenticationType` | | The authentication type used to connect to the underlying storage. Can be one of `AzureIdentity`, `AccountKey`, or `ConnectionString`. |
| `FoundationaLLM:Vectorization:ResourceProviderService:Storage:ConnectionString` | Key Vault secret name: `foundationallm-vectorization-resourceprovider-storage-connectionstring` | The connection string to the Azure Storage account used for the vectorization state service. |
| `FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:APIKey` | Key Vault secret name: `foundationallm-vectorization-semantickerneltextembedding-openai-apikey` | The API key used to connect to the Azure OpenAI service.
| `FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:AuthenticationType` | | The authentication type used to connect to the Azure OpenAI service. Can be one of `AzureIdentity` or `APIKey`.
| `FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:DeploymentName` | | The name of the Azure OpenAI model deployment. The default value is `embeddings`.
| `FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:Endpoint` | | The endpoint of the Azure OpenAI service.
| `FoundationaLLM:Vectorization:AzureAISearchIndexingService:APIKey` | Key Vault secret name: `foundationallm-vectorization-azureaisearch-apikey` | The API key used to connect to the Azure OpenAI service.
| `FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType` | | The authentication type used to connect to the Azure OpenAI service. Can be one of `AzureIdentity` or `APIKey`.
| `FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint` | | The endpoint of the Azure OpenAI service.

> [!NOTE]
> Refer to the [App Configuration values](../../app-configuration-values.md) page for more information on how to set these and other configuration values.

The following table describes the external content used by the vectorization worker to initialize:

| Uri | Description |
| --- | --- |
| `https://openaipublic.blob.core.windows.net/encodings/cl100k_base.tiktoken` | The public Azure Blob Storage account used to download the OpenAI BPE ranking files. |

> [!NOTE]
> The vectorization worker must be able to open HTTPS connections to the external content listed above.

### Default vectorization worker settings

The default settings for the vectorization worker are stored in the `FoundationaLLM:Vectorization:VectorizationWorker` App Configuration key. The default structure for this key is:

```json
{
    "RequestManagers": [
        {
            "RequestSourceName": "extract",
            "MaxHandlerInstances": 1
        },
        {
            "RequestSourceName": "partition",
            "MaxHandlerInstances": 1
        },
        {
            "RequestSourceName": "embed",
            "MaxHandlerInstances": 1
        },
        {
            "RequestSourceName": "index",
            "MaxHandlerInstances": 1
        }
    ],
    "RequestSources": [
        {
            "Name": "extract",
            "ConnectionConfigurationName": "Extract:ConnectionString",
            "VisibilityTimeoutSeconds": 600
        },
        {
            "Name": "partition",
            "ConnectionConfigurationName": "Partition:ConnectionString",
            "VisibilityTimeoutSeconds": 600
        },
        {
            "Name": "embed",
            "ConnectionConfigurationName": "Embed:ConnectionString",
            "VisibilityTimeoutSeconds": 600
        },
        {
            "Name": "index",
            "ConnectionConfigurationName": "Index:ConnectionString",
            "VisibilityTimeoutSeconds": 600
        }
    ],
    "QueuingEngine": "AzureStorageQueue"
}
```

The following table provides details about the configuration parameters:

| Parameter | Description |
| --- | --- |
| `RequestManagers` | The list of request managers used by the vectorization worker. Each request manager is responsible for managing the execution of vectorization pipelines for a specific vectorization step. The configuration must include all request managers. |
| `RequestManagers.MaxHandlerInstances` | The maximum number of request handlers that process requests for the specified request source. By default, the value is 1. You can change the value to increase the processing capacity of each vectorization worker instance. The value applies to all istances of the vectorization worker. NOTE: It is important to align the value of this setting with the level of compute and memory resources allocated to the individual vectorization worker instances. |
| `RequestSources` | The list of request sources used by the vectorization worker. Each request source is responsible for managing the requests for a specific vectorization step. The configuration must include all request sources. |
| `RequestSources.VisibilityTimeoutSeconds` | In the case of queue-based request sources (the default for the vectorization worker), specifies the time in seconds until a dequeued vectorization step request must be executed. During this timeout, the message will not be visible to other handler instances within the same worker or from other worker instances. If the handler fails to process the vectorization step request successfully and remove it from the queue within the specified timeout, the message will become visibile again. The default value is 600 seconds and should not be changed.|

### Vectorization content source profiles structure

Structure for the `vectorization-content-source-profiles.json` file:

```json
{
    "Profiles": [
        {
            "type": "content-source-profile",
            "name": "DefaultAzureDataLake",
            "object_id": "/instances/[INSTANCE ID]/providers/FoundationaLLM.Vectorization/contentsourceprofiles/DefaultAzureDataLake",
            "description": null,
            "deleted": false,
            "content_source": "AzureDataLake",
            "settings": {},
            "configuration_references": {
                "AuthenticationType": "FoundationaLLM:Vectorization:ContentSources:DefaultAzureDataLake:AuthenticationType",
                "ConnectionString": "FoundationaLLM:Vectorization:ContentSources:DefaultAzureDataLake:ConnectionString"
            }
        }
    ]
}
```

Currently, the following `content_source` types are supported:

- `AzureDataLake` - uses an Azure Data Lake storage account as the content source (see [`AzureDataLakeContentSource`](./vectorization-profiles.md#azuredatalake)).
- `SharePointOnline` - uses a SharePoint Online site as the content source (see [`SharePointOnlineContentSource`](./vectorization-profiles.md#sharepointonline)).
- `AzureSQLDatabase` - uses an Azure SQL database as the content source (see [`AzureSQLDatabaseContentSource`](./vectorization-profiles.md#azuresqldatabase)).

### Vectorization text partitioning profiles structure

Structure for the `vectorization-text-partitioning-profiles.json` file:

```json
{
    "Profiles": [
        {
            "type": "text-partitioning-profile",
            "name": "DefaultTokenTextPartition_Small",
            "object_id": "/instances/[INSTANCE ID]/providers/FoundationaLLM.Vectorization/textpartitioningprofiles/DefaultTokenTextPartition_Small",
            "description": null,
            "deleted": true,
            "text_splitter": "TokenTextSplitter",
            "settings": {
                "Tokenizer": "MicrosoftBPETokenizer",
                "TokenizerEncoder": "cl100k_base",
                "ChunkSizeTokens": "500",
                "OverlapSizeTokens": "50"
            },
            "configuration_references": {}
        }
    ]
}
```

Currently, the following `text_splitter` types are supported:

- `TokenTextSplitter` - splits the text into chunks based on the number of tokens (see [`TextTokenSplitter`](./vectorization-profiles.md#texttokensplitter)).

### Vectorization text embedding profiles structure

Structure for the `vectorization-text-embedding-profiles.json` file:

```json
{
    "Profiles": [
        {
            "type": "text-embedding-profile",
            "name": "AzureOpenAI_Embedding",
            "object_id": "/instances/[INSTANCE ID]/providers/FoundationaLLM.Vectorization/textembeddingprofiles/AzureOpenAI_Embedding",
            "description": null,
            "deleted": true,
            "text_embedding": "SemanticKernelTextEmbedding",
            "settings": {},
            "configuration_references": {
                "APIKey": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:APIKey",
                "APIVersion": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:APIVersion",
                "AuthenticationType": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:AuthenticationType",
                "DeploymentName": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:DeploymentName",
                "Endpoint": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:Endpoint"
            }
        }
    ]
}
```

Currently, the following `text_embedding` types are supported:

- `SemanticKernelTextEmbedding` - embeds the text using Semantic Kernel to call into the default FLLM Azure OpenAI embedding model (see [`SemanticKernelTextEmbedding`](./vectorization-profiles.md#semantickerneltextembedding)).

### Vectorization indexing profiles structure

Structure for the `vectorization-indexing-profiles.json` file:

```json
{
    "Profiles": [
        {
            "type": "indexing-profile",
            "name": "Azure_AI_Search_Indexing_Profile",
            "object_id": "/instances/[INSTANCE ID]/providers/FoundationaLLM.Vectorization/indexingprofiles/Azure_AI_Search_Indexing_Profile",
            "description": null,
            "deleted": true,
            "indexer": "AzureAISearchIndexer",
            "settings": {
                "IndexName": "azure-ai-search",
                "TopN": "3",
                "Filters": "",
                "EmbeddingFieldName": "Embedding",
                "TextFieldName": "Text"
            },
            "configuration_references": {
                "APIKey": "FoundationaLLM:Vectorization:AzureAISearchIndexingService:APIKey",
                "QueryAPIKey": "FoundationaLLM:Vectorization:AzureAISearchIndexingService:QueryAPIKey",
                "AuthenticationType": "FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType",
                "Endpoint": "FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint"
            }
        }
    ]
}
```

Currently, the following `indexer` types are supported:

- `AzureAISearchIndexer` - indexes the vectors into an Azure AI Search index (see [`AzureAISearchIndexer`](./vectorization-profiles.md#azureaisearchindexer)).
