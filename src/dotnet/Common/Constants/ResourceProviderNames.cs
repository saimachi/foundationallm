﻿using System.Collections.Immutable;

namespace FoundationaLLM.Common.Constants
{
    /// <summary>
    /// Contains constants of the resource provider names.
    /// </summary>
    public static class ResourceProviderNames
    {
        /// <summary>
        /// The name of the FoundationaLLM.Vectorization resource provider.
        /// </summary>
        public const string FoundationaLLM_Vectorization = "FoundationaLLM.Vectorization";

        /// <summary>
        /// The name of the FoundationaLLM.Agent resource provider.
        /// </summary>
        public const string FoundationaLLM_Agent = "FoundationaLLM.Agent";

        /// <summary>
        /// The name of the FoundationaLLM.Configuration resource provider.
        /// </summary>
        public const string FoundationaLLM_Configuration = "FoundationaLLM.Configuration";

        /// <summary>
        /// The name of the FoundationaLLM.Prompt resource provider.
        /// </summary>
        public const string FoundationaLLM_Prompt = "FoundationaLLM.Prompt";

        /// <summary>
        /// The name of the FoundationaLLM.DataSource resource provider.
        /// </summary>
        public const string FoundationaLLM_DataSource = "FoundationaLLM.DataSource";

        /// <summary>
        /// The name of the FoundationaLLM.Authorization resource provider.
        /// </summary>
        public const string FoundationaLLM_Authorization = "FoundationaLLM.Authorization";

        /// <summary>
        /// Contains all the resource provider names.
        /// </summary>
        public readonly static ImmutableList<string> All = [
            FoundationaLLM_Vectorization,
            FoundationaLLM_Agent,
            FoundationaLLM_Configuration,
            FoundationaLLM_Prompt,
            FoundationaLLM_DataSource,
            FoundationaLLM_Authorization];
    }
}
