using System.ComponentModel.DataAnnotations;

namespace Dignus.Candidate.Back.DTOs
{
    /// <summary>
    /// DTO for Audio Submission
    /// </summary>
    public class AudioSubmissionDto
    {
        public Guid Id { get; set; }
        
        [Required]
        public string BlobUrl { get; set; } = null!;
        
        public Data.Models.AudioSubmissionType Type { get; set; }
        
        public DateTimeOffset? SubmittedAt { get; set; }
        
        public Guid TestId { get; set; }
    }

    /// <summary>
    /// DTO for Video Interview
    /// </summary>
    public class VideoInterviewDto
    {
        public Guid Id { get; set; }
        
        [Required]
        public string BlobUrl { get; set; } = null!;
        
        public decimal? Score { get; set; }
        
        public string? Feedback { get; set; }
        
        public TimeSpan? Duration { get; set; }
        
        public Data.Models.TestStatus Status { get; set; }
        
        public DateTimeOffset SubmittedAt { get; set; }
        
        public DateTimeOffset? AnalyzedAt { get; set; }
        
        public string? Verdict { get; set; }
        
        public Guid TestId { get; set; }
    }

    /// <summary>
    /// DTO for uploading audio file
    /// </summary>
    public class UploadAudioDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;
        
        [Required]
        public Guid TestId { get; set; }
        
        public Data.Models.AudioSubmissionType Type { get; set; }
    }

    /// <summary>
    /// DTO for uploading video file
    /// </summary>
    public class UploadVideoDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;
        
        [Required] 
        public Guid TestId { get; set; }
    }
}