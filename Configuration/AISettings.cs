namespace Dignus.Candidate.Back.Configuration
{
    /// <summary>
    /// Configuration settings for AI processing services
    /// </summary>
    public class AISettings
    {
        /// <summary>
        /// Google AI/Gemini configuration
        /// </summary>
        public GoogleAISettings Google { get; set; } = null!;
        
        /// <summary>
        /// LangChain configuration
        /// </summary>
        public LangChainSettings LangChain { get; set; } = null!;
        
        /// <summary>
        /// Processing timeout settings
        /// </summary>
        public ProcessingTimeouts Timeouts { get; set; } = null!;
        
        /// <summary>
        /// Callback endpoint configuration
        /// </summary>
        public CallbackSettings Callbacks { get; set; } = null!;
    }

    /// <summary>
    /// Google AI/Gemini specific settings
    /// </summary>
    public class GoogleAISettings
    {
        /// <summary>
        /// Google AI API key
        /// </summary>
        public string ApiKey { get; set; } = null!;
        
        /// <summary>
        /// Model to use for text processing (e.g., "gemini-1.5-pro")
        /// </summary>
        public string TextModel { get; set; } = "gemini-1.5-pro";
        
        /// <summary>
        /// Model to use for audio transcription
        /// </summary>
        public string AudioModel { get; set; } = "gemini-1.5-pro";
        
        /// <summary>
        /// Model to use for video analysis
        /// </summary>
        public string VideoModel { get; set; } = "gemini-1.5-pro";
        
        /// <summary>
        /// Maximum tokens for responses
        /// </summary>
        public int MaxTokens { get; set; } = 4000;
        
        /// <summary>
        /// Temperature setting for AI responses (0.0-1.0)
        /// </summary>
        public double Temperature { get; set; } = 0.7;
    }

    /// <summary>
    /// LangChain framework settings
    /// </summary>
    public class LangChainSettings
    {
        /// <summary>
        /// Enable verbose logging for LangChain operations
        /// </summary>
        public bool VerboseLogging { get; set; } = false;
        
        /// <summary>
        /// Maximum retry attempts for failed operations
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;
        
        /// <summary>
        /// Delay between retry attempts in milliseconds
        /// </summary>
        public int RetryDelayMs { get; set; } = 1000;
    }

    /// <summary>
    /// Processing timeout configurations
    /// </summary>
    public class ProcessingTimeouts
    {
        /// <summary>
        /// Timeout for audio transcription processing in minutes
        /// </summary>
        public int AudioTranscriptionMinutes { get; set; } = 10;
        
        /// <summary>
        /// Timeout for video analysis processing in minutes
        /// </summary>
        public int VideoAnalysisMinutes { get; set; } = 15;
        
        /// <summary>
        /// Timeout for text evaluation processing in minutes
        /// </summary>
        public int TextEvaluationMinutes { get; set; } = 5;
        
        /// <summary>
        /// HTTP client timeout in seconds
        /// </summary>
        public int HttpClientSeconds { get; set; } = 300;
    }

    /// <summary>
    /// Callback endpoint settings for async processing
    /// </summary>
    public class CallbackSettings
    {
        /// <summary>
        /// Base URL for callback endpoints
        /// </summary>
        public string BaseUrl { get; set; } = null!;
        
        /// <summary>
        /// Shared secret for callback authentication
        /// </summary>
        public string SharedSecret { get; set; } = null!;
        
        /// <summary>
        /// Enable callback authentication
        /// </summary>
        public bool EnableAuthentication { get; set; } = true;
        
        /// <summary>
        /// Callback endpoints configuration
        /// </summary>
        public CallbackEndpoints Endpoints { get; set; } = null!;
    }

    /// <summary>
    /// Specific callback endpoint paths
    /// </summary>
    public class CallbackEndpoints
    {
        /// <summary>
        /// Endpoint for audio transcription completion
        /// </summary>
        public string AudioTranscription { get; set; } = "/api/ai/callback/audio-transcription";
        
        /// <summary>
        /// Endpoint for video analysis completion
        /// </summary>
        public string VideoAnalysis { get; set; } = "/api/ai/callback/video-analysis";
        
        /// <summary>
        /// Endpoint for evaluation completion
        /// </summary>
        public string EvaluationComplete { get; set; } = "/api/ai/callback/evaluation-complete";
    }
}