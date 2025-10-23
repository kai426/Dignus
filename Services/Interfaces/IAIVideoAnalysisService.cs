using Dignus.Candidate.Back.DTOs;

namespace Dignus.Candidate.Back.Services.Interfaces
{
    /// <summary>
    /// Service interface for AI-powered video analysis
    /// </summary>
    public interface IAIVideoAnalysisService
    {
        /// <summary>
        /// Analyzes a video interview using AI services
        /// </summary>
        /// <param name="videoInterviewId">The ID of the video interview to analyze</param>
        /// <param name="videoUrl">The URL of the video file in blob storage</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>The video analysis result</returns>
        Task<VideoAnalysisResultDto> AnalyzeVideoAsync(Guid videoInterviewId, string videoUrl, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Evaluates video analysis results with behavioral scoring
        /// </summary>
        /// <param name="videoInterviewId">The ID of the video interview</param>
        /// <param name="analysisResult">The raw analysis results</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Behavioral evaluation with scores</returns>
        Task<BehavioralEvaluationDto> EvaluateBehavioralAnalysisAsync(Guid videoInterviewId, VideoAnalysisResultDto analysisResult, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the status of an ongoing video analysis process
        /// </summary>
        /// <param name="videoInterviewId">The ID of the video interview</param>
        /// <returns>Processing status information</returns>
        Task<ProcessingStatusDto> GetAnalysisStatusAsync(Guid videoInterviewId);
        
        /// <summary>
        /// Cancels an ongoing video analysis process
        /// </summary>
        /// <param name="videoInterviewId">The ID of the video interview</param>
        /// <returns>True if successfully cancelled, false otherwise</returns>
        Task<bool> CancelAnalysisAsync(Guid videoInterviewId);
        
        /// <summary>
        /// Extracts key frames from video for analysis
        /// </summary>
        /// <param name="videoUrl">The URL of the video file</param>
        /// <param name="frameCount">Number of frames to extract</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>URLs of extracted frame images</returns>
        Task<IEnumerable<string>> ExtractKeyFramesAsync(string videoUrl, int frameCount = 5, CancellationToken cancellationToken = default);
    }
}