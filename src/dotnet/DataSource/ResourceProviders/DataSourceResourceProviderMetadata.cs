using FoundationaLLM.Common.Models.ResourceProvider;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.DataSource.Constants;
using FoundationaLLM.DataSource.Models;

namespace FoundationaLLM.DataSource.ResourceProviders
{
    /// <summary>
    /// Provides metadata for the FoundationaLLM.DataSource resource provider.
    /// </summary>
    public static class DataSourceResourceProviderMetadata
    {
        /// <summary>
        /// The metadata describing the resource types allowed by the resource provider.
        /// </summary>
        public static Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes => new()
        {
            {
                DataSourceResourceTypeNames.DataSources,
                new ResourceTypeDescriptor(
                        DataSourceResourceTypeNames.DataSources)
                {
                    AllowedTypes = [
                            new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(DataSourceBase)]),
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(DataSourceBase)], [typeof(ResourceProviderUpsertResult)]),
                            new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, [], [], []),
                    ],
                    Actions = [
                            new ResourceTypeAction(DataSourceResourceProviderActions.CheckName, false, true, [
                                new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(ResourceName)], [typeof(ResourceNameCheckResult)])
                            ])
                        ]
                }
            }
        };
    }
}
