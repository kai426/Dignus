using Dignus.Candidate.Back.DTOs;

namespace Dignus.Candidate.Back.Services.Interfaces
{
    /// <summary>
    /// Service interface for Portuguese question management operations
    /// </summary>
    public interface IPortugueseQuestionService
    {
        /// <summary>
        /// Get Portuguese questions with pagination and optional search
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="searchTerm">Optional search term</param>
        /// <returns>Questions and total count</returns>
        Task<(IEnumerable<QuestionDto> questions, int totalCount)> GetQuestionsAsync(
            int pageNumber, int pageSize, string? searchTerm = null);

        /// <summary>
        /// Get Portuguese question by ID
        /// </summary>
        /// <param name="id">Question ID</param>
        /// <returns>Question DTO or null</returns>
        Task<QuestionDto?> GetQuestionByIdAsync(string id);

        /// <summary>
        /// Create new Portuguese question
        /// </summary>
        /// <param name="createQuestionDto">Question creation data</param>
        /// <returns>Created question DTO</returns>
        Task<QuestionDto> CreateQuestionAsync(CreatePortugueseQuestionDto createQuestionDto);

        /// <summary>
        /// Update Portuguese question
        /// </summary>
        /// <param name="id">Question ID</param>
        /// <param name="updateQuestionDto">Question update data</param>
        /// <returns>Updated question DTO or null</returns>
        Task<QuestionDto?> UpdateQuestionAsync(string id, UpdatePortugueseQuestionDto updateQuestionDto);

        /// <summary>
        /// Delete Portuguese question
        /// </summary>
        /// <param name="id">Question ID</param>
        /// <returns>True if deleted, false if not found</returns>
        Task<bool> DeleteQuestionAsync(string id);

        /// <summary>
        /// Bulk import Portuguese questions
        /// </summary>
        /// <param name="questions">Questions to import</param>
        /// <returns>Import result summary</returns>
        Task<object> BulkImportQuestionsAsync(List<CreatePortugueseQuestionDto> questions);
    }
}