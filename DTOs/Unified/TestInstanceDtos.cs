using Dignus.Data.Models.Enums;
using System.ComponentModel.DataAnnotations;
using TestStatus = Dignus.Data.Models.TestStatus;

namespace Dignus.Candidate.Back.DTOs.Unified;

/// <summary>
/// Unified test instance DTO (replaces separate test DTOs)
/// </summary>
public class TestInstanceDto
{
    public Guid Id { get; set; }
    public TestType TestType { get; set; }
    public Guid CandidateId { get; set; }
    public TestStatus Status { get; set; }
    public decimal? Score { get; set; }
    public decimal? RawScore { get; set; }
    public decimal? MaxPossibleScore { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int? DurationSeconds { get; set; }

    // Test-type-specific data
    public Guid? PortugueseReadingTextId { get; set; }
    public PortugueseReadingTextDto? PortugueseReadingText { get; set; }

    // Questions (snapshotted)
    public List<QuestionSnapshotDto> Questions { get; set; } = new();

    // Responses
    public List<VideoResponseDto> VideoResponses { get; set; } = new();
    public List<QuestionResponseDto> QuestionResponses { get; set; } = new();

    // Metadata (for VisualRetention, etc.)
    public string? MetadataJson { get; set; }
}

/// <summary>
/// Request to create a new test
/// </summary>
public class CreateTestInstanceRequest
{
    [Required(ErrorMessage = "CandidateId is required")]
    public Guid CandidateId { get; set; }

    [Required(ErrorMessage = "TestType is required")]
    [EnumDataType(typeof(TestType), ErrorMessage = "Invalid test type")]
    public TestType TestType { get; set; }

    [RegularExpression("^(easy|medium|hard)$", ErrorMessage = "Difficulty must be easy, medium, or hard")]
    public string DifficultyLevel { get; set; } = "medium";
}

/// <summary>
/// Question snapshot DTO (never includes correct answers)
/// ⚠️ SECURITY: CorrectAnswerSnapshot must NEVER be included
/// </summary>
public class QuestionSnapshotDto
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = null!;
    public string? OptionsJson { get; set; }
    public bool AllowMultipleAnswers { get; set; }
    public int? MaxAnswersAllowed { get; set; }
    public int QuestionOrder { get; set; }
    public decimal PointValue { get; set; }
    public int? EstimatedTimeSeconds { get; set; }

    // ⚠️ NEVER include:
    // - CorrectAnswerSnapshot
    // - ExpectedAnswerGuideSnapshot
}

/// <summary>
/// Video response DTO (unified for all video-based tests)
/// </summary>
public class VideoResponseDto
{
    public Guid Id { get; set; }
    public Guid? QuestionSnapshotId { get; set; }
    public int QuestionNumber { get; set; }
    public VideoResponseType? ResponseType { get; set; } // For Portuguese: Reading/QuestionAnswer
    public string BlobUrl { get; set; } = null!;
    public long FileSizeBytes { get; set; }
    public DateTimeOffset UploadedAt { get; set; }

    // AI Analysis (populated by external AI agent)
    public string? Score { get; set; }
    public string? Feedback { get; set; }
    public string? Verdict { get; set; }
    public DateTimeOffset? AnalyzedAt { get; set; }
}

/// <summary>
/// Multiple choice response DTO (for Psychology/VisualRetention)
/// </summary>
public class QuestionResponseDto
{
    public Guid Id { get; set; }
    public Guid QuestionSnapshotId { get; set; }
    public List<string> SelectedAnswers { get; set; } = new(); // Parsed from JSON
    public int? ResponseTimeMs { get; set; }
    public DateTimeOffset AnsweredAt { get; set; }

    // Auto-grading result (shown after submission)
    public bool? IsCorrect { get; set; }
    public decimal? PointsEarned { get; set; }
}

/// <summary>
/// Request to submit test answers
/// </summary>
public class SubmitTestRequest
{
    [Required]
    public Guid TestId { get; set; }

    [Required]
    public Guid CandidateId { get; set; }

    [MinLength(1, ErrorMessage = "At least one answer is required")]
    public List<QuestionAnswerSubmission> Answers { get; set; } = new();
}

public class QuestionAnswerSubmission
{
    [Required]
    public Guid QuestionSnapshotId { get; set; }

    [MinLength(1, ErrorMessage = "At least one answer must be selected")]
    [MaxLength(5, ErrorMessage = "Maximum 5 answers allowed")]
    public List<string> SelectedAnswers { get; set; } = new();

    [Range(0, int.MaxValue, ErrorMessage = "Response time cannot be negative")]
    public int? ResponseTimeMs { get; set; }
}

/// <summary>
/// Test submission result
/// </summary>
public class TestSubmissionResultDto
{
    public Guid TestId { get; set; }
    public TestStatus Status { get; set; }
    public decimal? Score { get; set; }
    public decimal? RawScore { get; set; }
    public decimal? MaxPossibleScore { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public int? DurationSeconds { get; set; }
    public string Message { get; set; } = "Test submitted successfully";
}

/// <summary>
/// Test status information
/// </summary>
public class TestStatusDto
{
    public Guid TestId { get; set; }
    public TestStatus Status { get; set; }
    public int TotalQuestions { get; set; }
    public int QuestionsAnswered { get; set; }
    public int VideosUploaded { get; set; }
    public int VideosRequired { get; set; }
    public bool CanStart { get; set; }
    public bool CanSubmit { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public int? RemainingTimeSeconds { get; set; }
}

/// <summary>
/// Request to upload a video response
/// </summary>
public class UploadVideoRequest
{
    [Required]
    public Guid TestId { get; set; }

    [Required]
    public Guid CandidateId { get; set; }

    public Guid? QuestionSnapshotId { get; set; }

    [Required]
    [Range(1, 100, ErrorMessage = "Question number must be between 1 and 100")]
    public int QuestionNumber { get; set; }

    public VideoResponseType? ResponseType { get; set; }

    [Required(ErrorMessage = "Video file is required")]
    public IFormFile VideoFile { get; set; } = null!;
}

/// <summary>
/// Portuguese reading text DTO (kept from old schema)
/// </summary>
public class PortugueseReadingTextDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? AuthorName { get; set; }
    public int? EstimatedReadingTimeMinutes { get; set; }
}
