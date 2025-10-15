using Dignus.Candidate.Back.DTOs;

namespace Dignus.Candidate.Back.Services.Interfaces
{
    /// <summary>
    /// Service interface for media file management operations
    /// </summary>
    public interface IMediaService
    {
        /// <summary>
        /// Uploads an audio file for a candidate
        /// </summary>
        /// <param name="uploadAudioDto">The audio upload data</param>
        /// <returns>The created audio submission DTO</returns>
        Task<AudioSubmissionDto> UploadAudioAsync(UploadAudioDto uploadAudioDto);

        /// <summary>
        /// Uploads a video file for a candidate
        /// </summary>
        /// <param name="uploadVideoDto">The video upload data</param>
        /// <returns>The created video interview DTO</returns>
        Task<VideoInterviewDto> UploadVideoAsync(UploadVideoDto uploadVideoDto);

        /// <summary>
        /// Retrieves audio submissions for a candidate
        /// </summary>
        /// <param name="candidateId">The candidate identifier</param>
        /// <returns>A list of audio submission DTOs</returns>
        Task<IEnumerable<AudioSubmissionDto>> GetAudioSubmissionsByCandidateIdAsync(Guid candidateId);

        /// <summary>
        /// Retrieves video interviews for a candidate
        /// </summary>
        /// <param name="candidateId">The candidate identifier</param>
        /// <returns>A list of video interview DTOs</returns>
        Task<IEnumerable<VideoInterviewDto>> GetVideoInterviewsByCandidateIdAsync(Guid candidateId);

        /// <summary>
        /// Retrieves an audio submission by its identifier
        /// </summary>
        /// <param name="audioId">The audio submission identifier</param>
        /// <param name="candidateId">The candidate identifier for security validation</param>
        /// <returns>The audio submission DTO if found and owned by candidate, null otherwise</returns>
        Task<AudioSubmissionDto?> GetAudioSubmissionByIdAsync(Guid audioId, Guid candidateId);

        /// <summary>
        /// Retrieves a video interview by its identifier
        /// </summary>
        /// <param name="videoId">The video interview identifier</param>
        /// <param name="candidateId">The candidate identifier for security validation</param>
        /// <returns>The video interview DTO if found and owned by candidate, null otherwise</returns>
        Task<VideoInterviewDto?> GetVideoInterviewByIdAsync(Guid videoId, Guid candidateId);

        /// <summary>
        /// Deletes an audio submission
        /// </summary>
        /// <param name="audioId">The audio submission identifier</param>
        /// <param name="candidateId">The candidate identifier for security validation</param>
        /// <returns>True if successful, false if not found or not owned by candidate</returns>
        Task<bool> DeleteAudioSubmissionAsync(Guid audioId, Guid candidateId);

        /// <summary>
        /// Deletes a video interview
        /// </summary>
        /// <param name="videoId">The video interview identifier</param>
        /// <param name="candidateId">The candidate identifier for security validation</param>
        /// <returns>True if successful, false if not found or not owned by candidate</returns>
        Task<bool> DeleteVideoInterviewAsync(Guid videoId, Guid candidateId);

        /// <summary>
        /// Validates if a file is a supported audio format
        /// </summary>
        /// <param name="file">The file to validate</param>
        /// <returns>True if valid audio format, false otherwise</returns>
        Task<bool> IsValidAudioFileAsync(IFormFile file);

        /// <summary>
        /// Validates if a file is a supported video format
        /// </summary>
        /// <param name="file">The file to validate</param>
        /// <returns>True if valid video format, false otherwise</returns>
        Task<bool> IsValidVideoFileAsync(IFormFile file);

        /// <summary>
        /// Gets the maximum allowed file size for audio uploads
        /// </summary>
        /// <returns>Maximum file size in bytes</returns>
        long GetMaxAudioFileSize();

        /// <summary>
        /// Gets the maximum allowed file size for video uploads
        /// </summary>
        /// <returns>Maximum file size in bytes</returns>
        long GetMaxVideoFileSize();

        /// <summary>
        /// Updates transcription for an audio submission
        /// </summary>
        /// <param name="audioId">The audio submission identifier</param>
        /// <param name="transcription">The transcription text</param>
        /// <returns>True if successful, false if audio not found</returns>
        Task<bool> UpdateAudioTranscriptionAsync(Guid audioId, string transcription);

        /// <summary>
        /// Updates analysis data for a video interview
        /// </summary>
        /// <param name="videoId">The video interview identifier</param>
        /// <param name="transcription">The transcription text</param>
        /// <param name="behavioralAnalysis">The behavioral analysis</param>
        /// <param name="communicationScore">The communication score</param>
        /// <returns>True if successful, false if video not found</returns>
        Task<bool> UpdateVideoAnalysisAsync(Guid videoId, string? transcription, string? behavioralAnalysis, decimal? communicationScore);

        /// <summary>
        /// Uploads video answer for Math test question
        /// </summary>
        /// <param name="testId">Math test ID</param>
        /// <param name="questionNumber">Question number (1 or 2)</param>
        /// <param name="candidateId">Candidate ID</param>
        /// <param name="videoFile">Video file</param>
        /// <returns>Upload result</returns>
        Task<object> UploadMathQuestionVideoAsync(Guid testId, int questionNumber, Guid candidateId, IFormFile videoFile);
    }
}