using Dignus.Candidate.Back.DTOs.Admin;
using Dignus.Data.Models.Enums;

namespace Dignus.Candidate.Back.Services.Admin;

public interface IQuestionGroupAdminService
{
    Task<List<QuestionGroupSummaryDto>> GetQuestionGroupsAsync(TestType? testType, bool includeInactive);
    Task<QuestionGroupDetailDto?> GetQuestionGroupDetailsAsync(Guid groupId);
    Task<AdminOperationResult> CreateQuestionGroupAsync(CreateQuestionGroupRequest request);
    Task<AdminOperationResult> UpdateQuestionGroupAsync(Guid groupId, UpdateQuestionGroupRequest request);
    Task<AdminOperationResult> UpdateQuestionAsync(Guid groupId, Guid questionId, UpdateQuestionRequest request);
    Task<AdminOperationResult> ReorderQuestionsAsync(Guid groupId, ReorderQuestionsRequest request);
    Task<AdminOperationResult> ActivateGroupAsync(Guid groupId);
    Task<AdminOperationResult> DeactivateGroupAsync(Guid groupId);
    Task<AdminOperationResult> DeleteGroupAsync(Guid groupId);
}

public class AdminOperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public string? ErrorCode { get; set; }
    public Guid? GroupId { get; set; }
}
