using System.ComponentModel.DataAnnotations;
using Dignus.Data.Models.Enums;

namespace Dignus.Candidate.Back.DTOs.Admin;

/// <summary>
/// Summary DTO for listing question groups
/// </summary>
public class QuestionGroupSummaryDto
{
    public Guid Id { get; set; }
    public string TestType { get; set; } = null!;
    public string GroupName { get; set; } = null!;
    public string? Description { get; set; }
    public string? DifficultyLevel { get; set; }
    public bool IsActive { get; set; }
    public int QuestionCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>
/// Detailed DTO with all questions
/// </summary>
public class QuestionGroupDetailDto
{
    public Guid Id { get; set; }
    public string TestType { get; set; } = null!;
    public string GroupName { get; set; } = null!;
    public string? Description { get; set; }
    public string? DifficultyLevel { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public List<QuestionDetailDto> Questions { get; set; } = new();
}

public class QuestionDetailDto
{
    public Guid Id { get; set; }
    public int GroupOrder { get; set; }
    public string QuestionText { get; set; } = null!;
    public decimal PointValue { get; set; }
    public int? EstimatedTimeSeconds { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Request to create new question group
/// </summary>
public class CreateQuestionGroupRequest
{
    [Required]
    public TestType TestType { get; set; }

    [Required]
    [StringLength(200)]
    public string GroupName { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(20)]
    public string? DifficultyLevel { get; set; }

    public bool IsActive { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateQuestionDto> Questions { get; set; } = new();
}

public class CreateQuestionDto
{
    [Required]
    [Range(1, 100)]
    public int GroupOrder { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string QuestionText { get; set; } = null!;

    [Range(0.1, 10.0)]
    public decimal PointValue { get; set; } = 1.0m;

    [Range(10, 3600)]
    public int? EstimatedTimeSeconds { get; set; }
}

/// <summary>
/// Request to update question group metadata
/// </summary>
public class UpdateQuestionGroupRequest
{
    [Required]
    [StringLength(200)]
    public string GroupName { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(20)]
    public string? DifficultyLevel { get; set; }
}

/// <summary>
/// Request to update individual question
/// </summary>
public class UpdateQuestionRequest
{
    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string QuestionText { get; set; } = null!;

    [Range(0.1, 10.0)]
    public decimal PointValue { get; set; }

    [Range(10, 3600)]
    public int? EstimatedTimeSeconds { get; set; }
}

/// <summary>
/// Request to reorder questions
/// </summary>
public class ReorderQuestionsRequest
{
    [Required]
    public List<QuestionOrderDto> QuestionOrder { get; set; } = new();
}

public class QuestionOrderDto
{
    [Required]
    public Guid QuestionId { get; set; }

    [Required]
    [Range(1, 100)]
    public int NewOrder { get; set; }
}

/// <summary>
/// Response for list endpoint
/// </summary>
public class GetQuestionGroupsResponse
{
    public List<QuestionGroupSummaryDto> Groups { get; set; } = new();
}

/// <summary>
/// Response for create endpoint
/// </summary>
public class CreateGroupResponse
{
    public Guid Id { get; set; }
    public string Message { get; set; } = null!;
}
