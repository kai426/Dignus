using Dignus.Candidate.Back.DTOs.Unified;
using Dignus.Data.Models.Enums;

namespace Dignus.Candidate.Back.Services.Interfaces;

/// <summary>
/// Service for managing question templates (admin operations)
/// ⚠️ SECURITY: All methods should be restricted to admin/recruiter roles
/// </summary>
public interface IQuestionTemplateService
{
    /// <summary>
    /// Create a new question template with answer
    /// </summary>
    /// <param name="request">Question creation request</param>
    /// <returns>Created question template</returns>
    Task<QuestionTemplateDto> CreateQuestionAsync(CreateQuestionTemplateRequest request);

    /// <summary>
    /// Update an existing question template
    /// </summary>
    /// <param name="request">Question update request</param>
    /// <returns>Updated question template</returns>
    Task<QuestionTemplateDto> UpdateQuestionAsync(UpdateQuestionTemplateRequest request);

    /// <summary>
    /// Get question by ID (without answer)
    /// </summary>
    /// <param name="id">Question template ID</param>
    /// <returns>Question template DTO</returns>
    Task<QuestionTemplateDto> GetQuestionByIdAsync(Guid id);

    /// <summary>
    /// Get question by ID with answer details (admin only)
    /// </summary>
    /// <param name="id">Question template ID</param>
    /// <returns>Question template with answer</returns>
    Task<QuestionTemplateDetailDto> GetQuestionWithAnswerAsync(Guid id);

    /// <summary>
    /// Get all questions for a test type (paginated)
    /// </summary>
    /// <param name="testType">Type of test</param>
    /// <param name="includeInactive">Include deactivated questions</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Paginated list of questions</returns>
    Task<PaginatedResult<QuestionTemplateDto>> GetQuestionsByTestTypeAsync(
        TestType testType,
        bool includeInactive = false,
        int page = 1,
        int pageSize = 50);

    /// <summary>
    /// Get questions by difficulty level
    /// </summary>
    /// <param name="testType">Type of test</param>
    /// <param name="difficulty">Difficulty level (easy/medium/hard)</param>
    /// <returns>List of questions</returns>
    Task<List<QuestionTemplateDto>> GetQuestionsByDifficultyAsync(TestType testType, string difficulty);

    /// <summary>
    /// Get questions by category tag
    /// </summary>
    /// <param name="testType">Type of test</param>
    /// <param name="category">Category name</param>
    /// <returns>List of questions</returns>
    Task<List<QuestionTemplateDto>> GetQuestionsByCategoryAsync(TestType testType, string category);

    /// <summary>
    /// Deactivate a question (soft delete)
    /// </summary>
    /// <param name="id">Question template ID</param>
    Task DeactivateQuestionAsync(Guid id);

    /// <summary>
    /// Reactivate a previously deactivated question
    /// </summary>
    /// <param name="id">Question template ID</param>
    Task ReactivateQuestionAsync(Guid id);

    /// <summary>
    /// Get statistics about questions in the bank
    /// </summary>
    /// <param name="testType">Type of test (optional)</param>
    /// <returns>Question bank statistics</returns>
    Task<QuestionBankStatistics> GetQuestionBankStatisticsAsync(TestType? testType = null);
}

/// <summary>
/// Paginated result wrapper
/// </summary>
public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

/// <summary>
/// Question bank statistics
/// </summary>
public class QuestionBankStatistics
{
    public int TotalQuestions { get; set; }
    public int ActiveQuestions { get; set; }
    public int InactiveQuestions { get; set; }
    public Dictionary<string, int> QuestionsByDifficulty { get; set; } = new();
    public Dictionary<TestType, int> QuestionsByTestType { get; set; } = new();
    public Dictionary<string, int> QuestionsByCategory { get; set; } = new();
}
