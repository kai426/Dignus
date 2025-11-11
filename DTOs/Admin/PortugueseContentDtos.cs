using System.ComponentModel.DataAnnotations;

namespace Dignus.Candidate.Back.DTOs.Admin;

/// <summary>
/// Request to create Portuguese reading text with its questions
/// </summary>
public class CreatePortugueseContentRequest
{
    [Required(ErrorMessage = "Title is required")]
    [MinLength(5, ErrorMessage = "Title must be at least 5 characters")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Content is required")]
    [MinLength(50, ErrorMessage = "Content must be at least 50 characters")]
    [MaxLength(10000, ErrorMessage = "Content cannot exceed 10000 characters")]
    public string Content { get; set; } = null!;

    [MaxLength(200, ErrorMessage = "Author name cannot exceed 200 characters")]
    public string? AuthorName { get; set; }

    [MaxLength(500, ErrorMessage = "Source attribution cannot exceed 500 characters")]
    public string? SourceAttribution { get; set; }

    [Required]
    [RegularExpression("^(easy|medium|hard)$", ErrorMessage = "Difficulty must be easy, medium, or hard")]
    public string DifficultyLevel { get; set; } = "medium";

    [Range(1, 30, ErrorMessage = "Estimated reading time must be between 1 and 30 minutes")]
    public int EstimatedReadingTimeMinutes { get; set; } = 3;

    [Range(0, 10000, ErrorMessage = "Word count must be between 0 and 10000")]
    public int? WordCount { get; set; }

    /// <summary>
    /// Portuguese video questions for this reading text
    /// Should typically have 3 questions
    /// </summary>
    [Required(ErrorMessage = "Questions are required")]
    [MinLength(1, ErrorMessage = "At least 1 question is required")]
    [MaxLength(10, ErrorMessage = "Maximum 10 questions allowed")]
    public List<CreatePortugueseQuestionRequest> Questions { get; set; } = new();
}

/// <summary>
/// Portuguese video question (tied to reading text)
/// </summary>
public class CreatePortugueseQuestionRequest
{
    [Required(ErrorMessage = "Question text is required")]
    [MinLength(10, ErrorMessage = "Question text must be at least 10 characters")]
    [MaxLength(2000, ErrorMessage = "Question text cannot exceed 2000 characters")]
    public string QuestionText { get; set; } = null!;

    [Required]
    [Range(0.25, 100, ErrorMessage = "Point value must be between 0.25 and 100")]
    public decimal PointValue { get; set; } = 3.00m;

    [Range(30, 600, ErrorMessage = "Estimated time must be between 30 and 600 seconds")]
    public int? EstimatedTimeSeconds { get; set; } = 180;

    [MaxLength(500, ErrorMessage = "Category tags cannot exceed 500 characters")]
    public string[]? CategoryTags { get; set; }

    /// <summary>
    /// Grading guide JSON for evaluating video responses
    /// Example: {"key_points": ["point1", "point2"], "scoring": {"excellent": "5 pts", "good": "3-4 pts"}}
    /// </summary>
    [MaxLength(5000, ErrorMessage = "Expected answer guide cannot exceed 5000 characters")]
    public string? ExpectedAnswerGuideJson { get; set; }

    /// <summary>
    /// Order of this question (1, 2, 3, etc.)
    /// </summary>
    [Range(1, 10, ErrorMessage = "Question order must be between 1 and 10")]
    public int QuestionOrder { get; set; }
}

/// <summary>
/// Response DTO for created Portuguese content
/// </summary>
public class PortugueseContentDto
{
    public Guid ReadingTextId { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? AuthorName { get; set; }
    public string DifficultyLevel { get; set; } = null!;
    public int EstimatedReadingTimeMinutes { get; set; }
    public int? WordCount { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public List<PortugueseQuestionDto> Questions { get; set; } = new();
}

/// <summary>
/// Portuguese question DTO
/// </summary>
public class PortugueseQuestionDto
{
    public Guid QuestionId { get; set; }
    public string QuestionText { get; set; } = null!;
    public decimal PointValue { get; set; }
    public int? EstimatedTimeSeconds { get; set; }
    public string[]? CategoryTags { get; set; }
    public int QuestionOrder { get; set; }
    public bool IsActive { get; set; }
}
