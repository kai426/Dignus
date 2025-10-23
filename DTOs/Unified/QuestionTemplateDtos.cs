using Dignus.Data.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Dignus.Candidate.Back.DTOs.Unified;

/// <summary>
/// Question template DTO (for admin view - includes answer reference)
/// ⚠️ SECURITY: Only for admin/recruiter access
/// </summary>
public class QuestionTemplateDto
{
    public Guid Id { get; set; }
    public TestType TestType { get; set; }
    public string QuestionText { get; set; } = null!;
    public string? OptionsJson { get; set; }
    public bool AllowMultipleAnswers { get; set; }
    public int? MaxAnswersAllowed { get; set; }
    public string DifficultyLevel { get; set; } = "medium";
    public decimal PointValue { get; set; }
    public int? EstimatedTimeSeconds { get; set; }
    public string[]? CategoryTags { get; set; }
    public bool IsActive { get; set; }
    public int Version { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    // Answer indicator (not the actual answer)
    public bool HasAnswer { get; set; }
    public DateTimeOffset? AnswerLastUpdated { get; set; }
}

/// <summary>
/// Request to create a new question template
/// ⚠️ ADMIN ONLY
/// </summary>
public class CreateQuestionTemplateRequest
{
    [Required(ErrorMessage = "TestType is required")]
    [EnumDataType(typeof(TestType), ErrorMessage = "Invalid test type")]
    public TestType TestType { get; set; }

    [Required(ErrorMessage = "Question text is required")]
    [MinLength(10, ErrorMessage = "Question text must be at least 10 characters")]
    [MaxLength(2000, ErrorMessage = "Question text cannot exceed 2000 characters")]
    public string QuestionText { get; set; } = null!;

    [MaxLength(5000, ErrorMessage = "Options JSON cannot exceed 5000 characters")]
    public string? OptionsJson { get; set; }

    public bool AllowMultipleAnswers { get; set; }

    [Range(1, 10, ErrorMessage = "Max answers allowed must be between 1 and 10")]
    public int? MaxAnswersAllowed { get; set; }

    [Required]
    [RegularExpression("^(easy|medium|hard)$", ErrorMessage = "Difficulty must be easy, medium, or hard")]
    public string DifficultyLevel { get; set; } = "medium";

    [Required]
    [Range(0.25, 100, ErrorMessage = "Point value must be between 0.25 and 100")]
    public decimal PointValue { get; set; } = 1.00m;

    [Range(10, 3600, ErrorMessage = "Estimated time must be between 10 and 3600 seconds")]
    public int? EstimatedTimeSeconds { get; set; }

    [MaxLength(500, ErrorMessage = "Category tags cannot exceed 500 characters")]
    public string[]? CategoryTags { get; set; }

    // Answer data (stored separately in QuestionAnswer table)
    [MaxLength(2000, ErrorMessage = "Correct answer cannot exceed 2000 characters")]
    public string? CorrectAnswer { get; set; }

    [MaxLength(5000, ErrorMessage = "Expected answer guide cannot exceed 5000 characters")]
    public string? ExpectedAnswerGuideJson { get; set; }

    [MaxLength(5000, ErrorMessage = "Acceptable variations cannot exceed 5000 characters")]
    public string? AcceptableVariationsJson { get; set; }

    public Guid? CreatedByRecruiterId { get; set; }
}

/// <summary>
/// Request to update an existing question template
/// ⚠️ ADMIN ONLY
/// </summary>
public class UpdateQuestionTemplateRequest
{
    [Required]
    public Guid Id { get; set; }

    [MinLength(10, ErrorMessage = "Question text must be at least 10 characters")]
    [MaxLength(2000, ErrorMessage = "Question text cannot exceed 2000 characters")]
    public string? QuestionText { get; set; }

    [MaxLength(5000, ErrorMessage = "Options JSON cannot exceed 5000 characters")]
    public string? OptionsJson { get; set; }

    public bool? AllowMultipleAnswers { get; set; }

    [Range(1, 10, ErrorMessage = "Max answers allowed must be between 1 and 10")]
    public int? MaxAnswersAllowed { get; set; }

    [RegularExpression("^(easy|medium|hard)$", ErrorMessage = "Difficulty must be easy, medium, or hard")]
    public string? DifficultyLevel { get; set; }

    [Range(0.25, 100, ErrorMessage = "Point value must be between 0.25 and 100")]
    public decimal? PointValue { get; set; }

    [Range(10, 3600, ErrorMessage = "Estimated time must be between 10 and 3600 seconds")]
    public int? EstimatedTimeSeconds { get; set; }

    [MaxLength(500, ErrorMessage = "Category tags cannot exceed 500 characters")]
    public string[]? CategoryTags { get; set; }

    // Answer updates (optional)
    [MaxLength(2000, ErrorMessage = "Correct answer cannot exceed 2000 characters")]
    public string? CorrectAnswer { get; set; }

    [MaxLength(5000, ErrorMessage = "Expected answer guide cannot exceed 5000 characters")]
    public string? ExpectedAnswerGuideJson { get; set; }

    [MaxLength(5000, ErrorMessage = "Acceptable variations cannot exceed 5000 characters")]
    public string? AcceptableVariationsJson { get; set; }

    public Guid? UpdatedByRecruiterId { get; set; }
}

/// <summary>
/// Detailed question template DTO with answer (for admin editing)
/// ⚠️ SECURITY: ADMIN ONLY - Contains correct answers
/// </summary>
public class QuestionTemplateDetailDto
{
    public Guid Id { get; set; }
    public TestType TestType { get; set; }
    public string QuestionText { get; set; } = null!;
    public string? OptionsJson { get; set; }
    public bool AllowMultipleAnswers { get; set; }
    public int? MaxAnswersAllowed { get; set; }
    public string DifficultyLevel { get; set; } = "medium";
    public decimal PointValue { get; set; }
    public int? EstimatedTimeSeconds { get; set; }
    public string[]? CategoryTags { get; set; }
    public bool IsActive { get; set; }
    public int Version { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? CreatedByRecruiterId { get; set; }

    // Answer details (only included for admin)
    public QuestionAnswerDto? Answer { get; set; }
}

/// <summary>
/// Question answer DTO (admin only)
/// ⚠️ SECURITY: NEVER expose to candidates
/// </summary>
public class QuestionAnswerDto
{
    public string? CorrectAnswer { get; set; }
    public string? ExpectedAnswerGuideJson { get; set; }
    public string? AcceptableVariationsJson { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid? UpdatedByRecruiterId { get; set; }
}
