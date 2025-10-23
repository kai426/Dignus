using Dignus.Candidate.Back.DTOs.Unified;
using Dignus.Data.Models.Enums;

namespace Dignus.Candidate.Back.Services.Interfaces;

/// <summary>
/// Service for managing video responses to test questions (Portuguese reading, Interview questions)
/// </summary>
public interface IVideoResponseService
{
    /// <summary>
    /// Uploads a video response for a specific test question
    /// </summary>
    /// <param name="request">Video upload request with file data</param>
    /// <returns>Video response DTO with blob URL</returns>
    Task<VideoResponseDto> UploadVideoResponseAsync(UploadVideoRequest request);

    /// <summary>
    /// Gets all video responses for a test
    /// </summary>
    /// <param name="testId">Test instance ID</param>
    /// <param name="candidateId">Candidate ID for IDOR protection</param>
    /// <returns>List of video responses</returns>
    Task<List<VideoResponseDto>> GetTestVideoResponsesAsync(Guid testId, Guid candidateId);

    /// <summary>
    /// Gets a specific video response by ID
    /// </summary>
    /// <param name="videoResponseId">Video response ID</param>
    /// <param name="candidateId">Candidate ID for IDOR protection</param>
    /// <returns>Video response DTO</returns>
    Task<VideoResponseDto> GetVideoResponseAsync(Guid videoResponseId, Guid candidateId);

    /// <summary>
    /// Deletes a video response (before test submission)
    /// </summary>
    /// <param name="videoResponseId">Video response ID</param>
    /// <param name="candidateId">Candidate ID for IDOR protection</param>
    Task DeleteVideoResponseAsync(Guid videoResponseId, Guid candidateId);

    /// <summary>
    /// Validates video file before upload
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <param name="contentType">MIME type</param>
    /// <param name="fileSizeBytes">File size in bytes</param>
    /// <returns>Validation result with error message if invalid</returns>
    Task<(bool IsValid, string ErrorMessage)> ValidateVideoFileAsync(string fileName, string contentType, long fileSizeBytes);

    /// <summary>
    /// Gets a secure, time-limited URL for viewing a video
    /// </summary>
    /// <param name="videoResponseId">Video response ID</param>
    /// <param name="candidateId">Candidate ID for IDOR protection</param>
    /// <param name="expirationMinutes">URL expiration time (default 60 minutes)</param>
    /// <returns>Secure URL with SAS token</returns>
    Task<string> GetSecureVideoUrlAsync(Guid videoResponseId, Guid candidateId, int expirationMinutes = 60);
}
