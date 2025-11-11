using System.ComponentModel.DataAnnotations;

namespace Dignus.Candidate.Back.Configuration
{
    /// <summary>
    /// Configuration settings for Databricks integration
    /// </summary>
    public class DatabricksSettings
    {
        /// <summary>
        /// Databricks workspace URL (e.g., https://your-workspace.cloud.databricks.com)
        /// </summary>
        [Required]
        public string WorkspaceUrl { get; set; } = string.Empty;

        /// <summary>
        /// Access token for Databricks API authentication
        /// </summary>
        [Required]
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// SQL Warehouse ID for executing SQL queries (uses SQL Statement Execution API)
        /// </summary>
        [Required]
        public string WarehouseId { get; set; } = string.Empty;

        /// <summary>
        /// Catalog name (default: spark_catalog)
        /// </summary>
        public string? Catalog { get; set; } = "spark_catalog";

        /// <summary>
        /// Schema name (default: default)
        /// </summary>
        public string? Schema { get; set; } = "default";

        /// <summary>
        /// Database name for candidate data
        /// </summary>
        [Required]
        public string DatabaseName { get; set; } = "default";

        /// <summary>
        /// Connection timeout in seconds
        /// </summary>
        public int ConnectionTimeoutSeconds { get; set; } = 30;
    }

}