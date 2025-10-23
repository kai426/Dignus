using System.ComponentModel.DataAnnotations;

namespace Dignus.Candidate.Back.Configuration
{
    /// <summary>
    /// Configuration settings for Databricks integration
    /// </summary>
    public class DatabricksSettings
    {
        /// <summary>
        /// Databricks workspace URL
        /// </summary>
        [Required]
        public string WorkspaceUrl { get; set; } = string.Empty;

        /// <summary>
        /// Access token for Databricks API
        /// </summary>
        [Required]
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Cluster ID to use for Gupy data processing
        /// </summary>
        [Required]
        public string ClusterId { get; set; } = string.Empty;

        /// <summary>
        /// Databricks notebook path for Gupy integration
        /// </summary>
        [Required]
        public string GupyNotebookPath { get; set; } = "/Shared/gupy-integration";

        /// <summary>
        /// Database name for candidate data
        /// </summary>
        [Required]
        public string DatabaseName { get; set; } = "candidates_db";

        /// <summary>
        /// Table name for candidate synchronization
        /// </summary>
        [Required]
        public string CandidateTableName { get; set; } = "gupy_candidates";

        /// <summary>
        /// Table name for job listings synchronization
        /// </summary>
        [Required]
        public string JobsTableName { get; set; } = "gupy_jobs";

        /// <summary>
        /// Synchronization frequency in minutes (default: 60 minutes)
        /// </summary>
        public int SyncFrequencyMinutes { get; set; } = 60;

        /// <summary>
        /// Maximum retry attempts for failed sync operations
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Timeout for Databricks job execution in minutes
        /// </summary>
        public int JobTimeoutMinutes { get; set; } = 30;

        /// <summary>
        /// Enable monitoring and alerting for sync operations
        /// </summary>
        public bool EnableMonitoring { get; set; } = true;

        /// <summary>
        /// Batch size for candidate data synchronization
        /// </summary>
        public int BatchSize { get; set; } = 1000;

        /// <summary>
        /// Connection timeout in seconds
        /// </summary>
        public int ConnectionTimeoutSeconds { get; set; } = 30;
    }

    /// <summary>
    /// Configuration settings for Gupy API integration
    /// </summary>
    public class GupySettings
    {
        public const string SectionName = "GupyAPI";

        /// <summary>
        /// Gupy API base URL
        /// </summary>
        [Required]
        public string ApiBaseUrl { get; set; } = "https://api.gupy.io/v2";

        /// <summary>
        /// Gupy API key
        /// </summary>
        [Required]
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Company ID in Gupy system
        /// </summary>
        public string CompanyId { get; set; } = string.Empty;

        /// <summary>
        /// Whether to use mock data for development
        /// </summary>
        public bool UseMockData { get; set; } = true;

        /// <summary>
        /// Client ID for Gupy OAuth
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Client secret for Gupy OAuth
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// Enable webhook notifications from Gupy
        /// </summary>
        public bool EnableWebhooks { get; set; } = true;

        /// <summary>
        /// Webhook secret for signature validation
        /// </summary>
        public string WebhookSecret { get; set; } = string.Empty;

        /// <summary>
        /// Rate limiting: requests per minute
        /// </summary>
        public int RateLimitRequestsPerMinute { get; set; } = 60;

        /// <summary>
        /// Data retention period in days
        /// </summary>
        public int DataRetentionDays { get; set; } = 90;
    }
}