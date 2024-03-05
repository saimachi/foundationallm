﻿namespace FoundationaLLM.DataSource.Constants
{
    /// <summary>
    /// String constants for the types of data sources.
    /// </summary>
    public static class DataSourceTypes
    {
        /// <summary>
        /// Basic data source without practical functionality. Used as base for all other data sources.
        /// </summary>
        public const string Basic = "basic";

        /// <summary>
        /// Azure Data Lake data source.
        /// </summary>
        public const string AzureDataLake = "azure-data-lake";

        /// <summary>
        /// Azure SQL Database data source.
        /// </summary>
        public const string AzureSQLDatabase = "azure-sql-database";

        /// <summary>
        /// SharePoint Online Site data source.
        /// </summary>
        public const string SharePointOnlineSite = "sharepoint-online-site";
    }
}
