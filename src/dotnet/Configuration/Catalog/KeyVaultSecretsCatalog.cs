﻿using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Configuration.KeyVault;

namespace FoundationaLLM.Configuration.Catalog
{
    /// <summary>
    /// A catalog of Key Vault secrets used in this solution.
    /// </summary>
    public static class KeyVaultSecretsCatalog
    {
        /// <summary>
        /// The list of generic Key Vault secret entries.
        /// </summary>
        public static readonly List<KeyVaultSecretEntry> GenericEntries =
        [
            new(
                secretName: KeyVaultSecretNames
                    .FoundationaLLM_AgentHub_StorageManager_BlobStorage_ConnectionString,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: KeyVaultSecretNames
                    .FoundationaLLM_Agent_ResourceProvider_Storage_ConnectionString,
                minimumVersion: "0.3.0",
                description: "The connection string to the Azure Storage account used for the agent resource provider."
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_APIs_AgentFactoryAPI_APIKey,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_App_Insights_Connection_String,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_APIs_AgentHubAPI_APIKey,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_APIs_DataSourceHubAPI_APIKey,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_APIs_GatekeeperAPI_APIKey,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_APIs_GatekeeperIntegrationAPI_APIKey,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_APIs_LangChainAPI_APIKey,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: KeyVaultSecretNames
                    .FoundationaLLM_Prompt_ResourceProvider_Storage_ConnectionString,
                minimumVersion: "0.3.0",
                description: "The connection string to the Azure Storage account used for the prompt resource provider."
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_APIs_PromptHubAPI_APIKey,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_APIs_SemanticKernelAPI_APIKey,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_AzureContentSafety_APIKey,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_AzureOpenAI_Api_Key,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: KeyVaultSecretNames
                    .FoundationaLLM_BlobStorageMemorySource_Blobstorageconnection,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_CosmosDB_Key,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: KeyVaultSecretNames
                    .FoundationaLLM_DataSourceHub_StorageManager_BlobStorage_ConnectionString,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: KeyVaultSecretNames
                    .FoundationaLLM_DataSourceHub_StorageManager_BlobStorage_ConnectionString,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_OpenAI_Api_Key,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: KeyVaultSecretNames
                    .FoundationaLLM_PromptHub_StorageManager_BlobStorage_ConnectionString,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_SemanticKernelAPI_OpenAI_Key,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_APIs_VectorizationAPI_APIKey,
                minimumVersion: "0.3.0",
                description: "The API key of the vectorization API."
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_APIs_VectorizationWorker_APIKey,
                minimumVersion: "0.3.0",
                description: "The API key of the vectorization worker API."
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_Vectorization_Queues_ConnectionString,
                minimumVersion: "0.3.0",
                description:
                "The connection string to the Azure Storage account used for the embed vectorization queue."
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_Vectorization_Queues_ConnectionString,
                minimumVersion: "0.3.0",
                description:
                "The connection string to the Azure Storage account used for the extract vectorization queue."
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_Vectorization_Queues_ConnectionString,
                minimumVersion: "0.3.0",
                description:
                "The connection string to the Azure Storage account used for the index vectorization queue."
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_Vectorization_Queues_ConnectionString,
                minimumVersion: "0.3.0",
                description:
                "The connection string to the Azure Storage account used for the partition vectorization queue."
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_Vectorization_State_ConnectionString,
                minimumVersion: "0.3.0",
                description:
                "The connection string to the Azure Storage account used for the vectorization state service."
            ),
            new(
                secretName: KeyVaultSecretNames
                    .FoundationaLLM_Vectorization_ResourceProvider_Storage_ConnectionString,
                minimumVersion: "0.3.0",
                description:
                "The connection string to the Azure Storage account used for the vectorization state service."
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_Events_AzureEventGrid_APIKey,
                minimumVersion: "0.4.0",
                description:
                "The API key for the Azure Event Grid service."
            ),
            new(
                secretName: KeyVaultSecretNames
                    .FoundationaLLM_DataSource_ResourceProvider_Storage_ConnectionString,
                minimumVersion: "0.5.0",
                description: "The connection string to the Azure Storage account used for the data source resource provider."
            ),
        ];

        /// <summary>
        /// The list of Key Vault secret entries specific to the Authorization API.
        /// </summary>
        public static readonly List<KeyVaultSecretEntry> AuthorizationEntries =
        [
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_AuthorizationAPI_AppInsights_ConnectionString,
                minimumVersion: "0.5.0",
                description: "The connection string used by OpenTelemetry to connect to App Insights."
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_AuthorizationAPI_Entra_Instance,
                minimumVersion: "0.5.0",
                description: "The Entra ID instance."
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_AuthorizationAPI_Entra_TenantId,
                minimumVersion: "0.5.0",
                description: "The Entra ID tenant id."
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_AuthorizationAPI_Entra_ClientId,
                minimumVersion: "0.5.0",
                description: "The Entra ID client id."
            ),
            new(secretName: KeyVaultSecretNames.FoundationaLLM_AuthorizationAPI_Entra_ClientSecret,
                minimumVersion: "0.5.0",
                description: "The Entra ID client secret."
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_AuthorizationAPI_Entra_Scopes,
                minimumVersion: "0.5.0",
                description: "The Entra ID scopes."
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_AuthorizationAPI_Storage_AccountName,
                minimumVersion: "0.5.0",
                description: "The name of the storage account used by the Authorization API."
            ),
            new(
                secretName: KeyVaultSecretNames.FoundationaLLM_AuthorizationAPI_InstanceIds,
                minimumVersion: "0.5.0",
                description: "The comma separated list of the identifiers of FoundationaLLM instances managed by the authorization core."
            )
        ];

        /// <summary>
        /// Returns the list of all the Key Vault secrets for this solution that are required for the given version.
        /// </summary>
        /// <param name="version">The current version of the caller.</param>
        /// <param name="serviceName">Optional service name. When not specified, the generic key vault secrets list is returned.</param>
        /// <returns></returns>
        public static IEnumerable<KeyVaultSecretEntry> GetRequiredKeyVaultSecretsForVersion(
            string version,
            string serviceName = "")
        {
            // Extract the numeric part of the version, ignoring pre-release tags.
            var numericVersionPart = version.Split('-')[0];
            if (!Version.TryParse(numericVersionPart, out var currentVersion))
            {
                throw new ArgumentException($"Invalid version format for the provided version ({version}).", nameof(version));
            }

            var entriesList =  (serviceName == ServiceNames.AuthorizationAPI)
                ? AuthorizationEntries
                : GenericEntries;

            // Compare based on the Major, Minor, and Build numbers only.
            return entriesList.Where(entry =>
            {
                if (string.IsNullOrWhiteSpace(entry.MinimumVersion))
                {
                    return false;
                }

                var entryNumericVersionPart = entry.MinimumVersion.Split('-')[0];
                if (!Version.TryParse(entryNumericVersionPart, out var entryVersion))
                {
                    return false;
                }

                var entryVersionWithoutRevision = new Version(entryVersion.Major, entryVersion.Minor, entryVersion.Build);
                var currentVersionWithoutRevision = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build);

                return entryVersionWithoutRevision <= currentVersionWithoutRevision;
            });
        }
    }

}
