using System.ComponentModel.DataAnnotations;

namespace Dignus.Candidate.Back.DTOs
{
    /// <summary>
    /// DTO for transcription result from AI services
    /// </summary>
    public class TranscriptionResultDto
    {
        /// <summary>
        /// The ID of the audio submission
        /// </summary>
        public Guid AudioSubmissionId { get; set; }
        
        /// <summary>
        /// The transcribed text
        /// </summary>
        [Required]
        public string TranscribedText { get; set; } = null!;
        
        /// <summary>
        /// Confidence score of the transcription (0.0-1.0)
        /// </summary>
        [Range(0.0, 1.0)]
        public double Confidence { get; set; }
        
        /// <summary>
        /// Duration of the audio in seconds
        /// </summary>
        public double DurationSeconds { get; set; }
        
        /// <summary>
        /// Language detected in the audio
        /// </summary>
        public string DetectedLanguage { get; set; } = null!;
        
        /// <summary>
        /// Processing timestamp
        /// </summary>
        public DateTimeOffset ProcessedAt { get; set; }
        
        /// <summary>
        /// Any errors encountered during processing
        /// </summary>
        public string? Errors { get; set; }
    }

    /// <summary>
    /// DTO for audio evaluation results
    /// </summary>
    public class AudioEvaluationDto
    {
        /// <summary>
        /// The ID of the audio submission
        /// </summary>
        public Guid AudioSubmissionId { get; set; }
        
        /// <summary>
        /// Communication clarity score (0-100)
        /// </summary>
        [Range(0, 100)]
        public int CommunicationScore { get; set; }
        
        /// <summary>
        /// Grammar and language proficiency score (0-100)
        /// </summary>
        [Range(0, 100)]
        public int GrammarScore { get; set; }
        
        /// <summary>
        /// Vocabulary richness score (0-100)
        /// </summary>
        [Range(0, 100)]
        public int VocabularyScore { get; set; }
        
        /// <summary>
        /// Overall fluency score (0-100)
        /// </summary>
        [Range(0, 100)]
        public int FluencyScore { get; set; }
        
        /// <summary>
        /// Content relevance to the question (0-100)
        /// </summary>
        [Range(0, 100)]
        public int ContentRelevanceScore { get; set; }
        
        /// <summary>
        /// Detailed feedback on the audio response
        /// </summary>
        public string DetailedFeedback { get; set; } = null!;
        
        /// <summary>
        /// Strengths identified in the response
        /// </summary>
        public List<string> Strengths { get; set; } = new();
        
        /// <summary>
        /// Areas for improvement
        /// </summary>
        public List<string> ImprovementAreas { get; set; } = new();
        
        /// <summary>
        /// Overall evaluation score (calculated from component scores)
        /// </summary>
        [Range(0, 100)]
        public int OverallScore { get; set; }
        
        /// <summary>
        /// Evaluation timestamp
        /// </summary>
        public DateTimeOffset EvaluatedAt { get; set; }
    }

    /// <summary>
    /// DTO for processing status tracking
    /// </summary>
    public class ProcessingStatusDto
    {
        /// <summary>
        /// The ID of the item being processed
        /// </summary>
        public Guid ItemId { get; set; }
        
        /// <summary>
        /// Type of processing (Transcription, VideoAnalysis, etc.)
        /// </summary>
        public string ProcessingType { get; set; } = null!;
        
        /// <summary>
        /// Current status of processing
        /// </summary>
        public ProcessingStatus Status { get; set; }
        
        /// <summary>
        /// Progress percentage (0-100)
        /// </summary>
        [Range(0, 100)]
        public int ProgressPercentage { get; set; }
        
        /// <summary>
        /// Current processing stage description
        /// </summary>
        public string CurrentStage { get; set; } = null!;
        
        /// <summary>
        /// Estimated time remaining in seconds
        /// </summary>
        public int? EstimatedTimeRemainingSeconds { get; set; }
        
        /// <summary>
        /// Processing start time
        /// </summary>
        public DateTimeOffset StartedAt { get; set; }
        
        /// <summary>
        /// Processing completion time (if completed)
        /// </summary>
        public DateTimeOffset? CompletedAt { get; set; }
        
        /// <summary>
        /// Error message if processing failed
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// Additional metadata about the processing
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Processing status enumeration
    /// </summary>
    public enum ProcessingStatus
    {
        /// <summary>
        /// Processing has been queued but not started
        /// </summary>
        Queued,
        
        /// <summary>
        /// Processing is currently in progress
        /// </summary>
        InProgress,
        
        /// <summary>
        /// Processing completed successfully
        /// </summary>
        Completed,
        
        /// <summary>
        /// Processing failed with an error
        /// </summary>
        Failed,
        
        /// <summary>
        /// Processing was cancelled
        /// </summary>
        Cancelled,
        
        /// <summary>
        /// Processing timed out
        /// </summary>
        TimedOut
    }
}