using Dignus.Candidate.Back.DTOs;

namespace Dignus.Candidate.Back.Services.Interfaces
{
    /// <summary>
    /// Service interface for AI-powered audio transcription
    /// </summary>
    public interface IAITranscriptionService
    {
        /// <summary>
        /// Transcribes an audio file using AI services
        /// </summary>
        /// <param name="audioSubmissionId">The ID of the audio submission to transcribe</param>
        /// <param name="audioUrl">The URL of the audio file in blob storage</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>The transcription result</returns>
        Task<TranscriptionResultDto> TranscribeAudioAsync(Guid audioSubmissionId, string audioUrl, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Processes transcription with evaluation and scoring
        /// </summary>
        /// <param name="audioSubmissionId">The ID of the audio submission</param>
        /// <param name="transcription">The raw transcription text</param>
        /// <param name="testType">The type of test (Portuguese, etc.)</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Evaluation results with scoring</returns>
        Task<AudioEvaluationDto> EvaluateTranscriptionAsync(Guid audioSubmissionId, string transcription, string testType, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the status of an ongoing transcription process
        /// </summary>
        /// <param name="audioSubmissionId">The ID of the audio submission</param>
        /// <returns>Processing status information</returns>
        Task<ProcessingStatusDto> GetTranscriptionStatusAsync(Guid audioSubmissionId);
        
        /// <summary>
        /// Cancels an ongoing transcription process
        /// </summary>
        /// <param name="audioSubmissionId">The ID of the audio submission</param>
        /// <returns>True if successfully cancelled, false otherwise</returns>
        Task<bool> CancelTranscriptionAsync(Guid audioSubmissionId);
    }
}