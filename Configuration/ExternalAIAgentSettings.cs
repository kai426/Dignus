namespace Dignus.Candidate.Back.Configuration
{
    /// <summary>
    /// Configuration settings for external AI agent API integration
    /// </summary>
    public class ExternalAIAgentSettings
    {
        /// <summary>
        /// Base URL of the external AI agent API
        /// Example: "https://ai-agent.example.com"
        /// </summary>
        public string BaseUrl { get; set; } = null!;

        /// <summary>
        /// Endpoint path for video analysis
        /// Example: "/api/analyze" or "/api/v1/video/analyze"
        /// </summary>
        public string AnalyzeEndpoint { get; set; } = "/api/analyze";

        /// <summary>
        /// API key or bearer token for authentication (optional)
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Request timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Delay between batch requests in milliseconds
        /// </summary>
        public int BatchDelayMilliseconds { get; set; } = 500;

        /// <summary>
        /// Enable automatic video analysis after upload
        /// </summary>
        public bool EnableAutoAnalysis { get; set; } = true;

        /// <summary>
        /// Retry count for failed requests
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// Retry delay in milliseconds
        /// </summary>
        public int RetryDelayMilliseconds { get; set; } = 1000;
    }
}
