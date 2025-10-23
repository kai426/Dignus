using Dignus.Candidate.Back.DTOs.Admin;
using Dignus.Data.Repositories.Interfaces;
using Dignus.Data.Models.Enums;
using Dignus.Data.Models.Questions;
using Dignus.Data.Models.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dignus.Candidate.Back.Services.Admin;

public class QuestionGroupAdminService : IQuestionGroupAdminService
{
    private readonly ITestQuestionGroupRepository _groupRepo;
    private readonly IQuestionTemplateRepository _questionRepo;
    private readonly ILogger<QuestionGroupAdminService> _logger;
    private readonly TimeProvider _timeProvider;

    // Question count requirements per test type
    private static readonly Dictionary<TestType, int> RequiredQuestionCounts = new()
    {
        { TestType.Portuguese, 3 },
        { TestType.Math, 2 },
        { TestType.Interview, 5 }
    };

    public QuestionGroupAdminService(
        ITestQuestionGroupRepository groupRepo,
        IQuestionTemplateRepository questionRepo,
        ILogger<QuestionGroupAdminService> logger,
        TimeProvider timeProvider)
    {
        _groupRepo = groupRepo;
        _questionRepo = questionRepo;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task<List<QuestionGroupSummaryDto>> GetQuestionGroupsAsync(
        TestType? testType,
        bool includeInactive)
    {
        IEnumerable<TestQuestionGroup> groups;

        if (testType.HasValue)
        {
            groups = await _groupRepo.GetByTestTypeAsync(testType.Value, !includeInactive);
        }
        else
        {
            groups = await _groupRepo.GetAllAsync();
            if (!includeInactive)
            {
                groups = groups.Where(g => g.IsActive);
            }
        }

        return groups.Select(g => new QuestionGroupSummaryDto
        {
            Id = g.Id,
            TestType = g.TestType.ToString(),
            GroupName = g.GroupName,
            Description = g.Description,
            DifficultyLevel = g.DifficultyLevel,
            IsActive = g.IsActive,
            QuestionCount = g.Questions?.Count ?? 0,
            CreatedAt = g.CreatedAt,
            UpdatedAt = g.UpdatedAt
        }).ToList();
    }

    public async Task<QuestionGroupDetailDto?> GetQuestionGroupDetailsAsync(Guid groupId)
    {
        var group = await _groupRepo.GetByIdWithQuestionsAsync(groupId);

        if (group == null)
        {
            return null;
        }

        return new QuestionGroupDetailDto
        {
            Id = group.Id,
            TestType = group.TestType.ToString(),
            GroupName = group.GroupName,
            Description = group.Description,
            DifficultyLevel = group.DifficultyLevel,
            IsActive = group.IsActive,
            CreatedAt = group.CreatedAt,
            UpdatedAt = group.UpdatedAt,
            Questions = group.Questions
                .OrderBy(q => q.GroupOrder ?? int.MaxValue)
                .Select(q => new QuestionDetailDto
                {
                    Id = q.Id,
                    GroupOrder = q.GroupOrder ?? 0,
                    QuestionText = q.QuestionText,
                    PointValue = q.PointValue,
                    EstimatedTimeSeconds = q.EstimatedTimeSeconds,
                    IsActive = q.IsActive
                })
                .ToList()
        };
    }

    public async Task<AdminOperationResult> CreateQuestionGroupAsync(CreateQuestionGroupRequest request)
    {
        try
        {
            // Validate question count
            if (!RequiredQuestionCounts.TryGetValue(request.TestType, out var requiredCount))
            {
                return new AdminOperationResult
                {
                    Success = false,
                    ErrorCode = "UNSUPPORTED_TEST_TYPE",
                    Message = $"Test type {request.TestType} is not supported"
                };
            }

            if (request.Questions.Count != requiredCount)
            {
                return new AdminOperationResult
                {
                    Success = false,
                    ErrorCode = "INVALID_QUESTION_COUNT",
                    Message = $"{request.TestType} test requires exactly {requiredCount} questions, but {request.Questions.Count} were provided"
                };
            }

            // Validate unique GroupOrder
            var orders = request.Questions.Select(q => q.GroupOrder).ToList();
            if (orders.Distinct().Count() != orders.Count)
            {
                return new AdminOperationResult
                {
                    Success = false,
                    ErrorCode = "DUPLICATE_GROUP_ORDER",
                    Message = "Each question must have a unique GroupOrder"
                };
            }

            // Validate GroupOrder sequence (must be 1, 2, 3, ... or similar)
            var sortedOrders = orders.OrderBy(o => o).ToList();
            for (int i = 0; i < sortedOrders.Count; i++)
            {
                if (sortedOrders[i] <= 0)
                {
                    return new AdminOperationResult
                    {
                        Success = false,
                        ErrorCode = "INVALID_GROUP_ORDER",
                        Message = "GroupOrder must be a positive number"
                    };
                }
            }

            // Create group
            var group = new TestQuestionGroup
            {
                Id = Guid.NewGuid(),
                TestType = request.TestType,
                GroupName = request.GroupName,
                Description = request.Description,
                DifficultyLevel = request.DifficultyLevel,
                IsActive = request.IsActive,
                CreatedAt = _timeProvider.GetUtcNow()
            };

            // Create questions
            var questions = request.Questions.Select(q => new QuestionTemplate
            {
                Id = Guid.NewGuid(),
                TestQuestionGroupId = group.Id,
                GroupOrder = q.GroupOrder,
                QuestionText = q.QuestionText,
                PointValue = q.PointValue,
                EstimatedTimeSeconds = q.EstimatedTimeSeconds,
                IsActive = true,
                CreatedAt = _timeProvider.GetUtcNow()
            }).ToList();

            group.Questions = questions;

            await _groupRepo.CreateAsync(group);
            await _groupRepo.SaveChangesAsync();

            _logger.LogInformation("Created question group {GroupId} for test type {TestType} with {QuestionCount} questions",
                group.Id, request.TestType, questions.Count);

            return new AdminOperationResult
            {
                Success = true,
                Message = "Question group created successfully",
                GroupId = group.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating question group for test type {TestType}", request.TestType);
            return new AdminOperationResult
            {
                Success = false,
                ErrorCode = "INTERNAL_ERROR",
                Message = "Failed to create question group. Please try again."
            };
        }
    }

    public async Task<AdminOperationResult> UpdateQuestionGroupAsync(
        Guid groupId,
        UpdateQuestionGroupRequest request)
    {
        try
        {
            var group = await _groupRepo.GetByIdAsync(groupId);

            if (group == null)
            {
                return new AdminOperationResult
                {
                    Success = false,
                    ErrorCode = "GROUP_NOT_FOUND",
                    Message = "Question group not found"
                };
            }

            group.GroupName = request.GroupName;
            group.Description = request.Description;
            group.DifficultyLevel = request.DifficultyLevel;
            group.UpdatedAt = _timeProvider.GetUtcNow();

            await _groupRepo.UpdateAsync(group);
            await _groupRepo.SaveChangesAsync();

            _logger.LogInformation("Updated question group {GroupId} metadata", groupId);

            return new AdminOperationResult
            {
                Success = true,
                Message = "Question group updated successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating question group {GroupId}", groupId);
            return new AdminOperationResult
            {
                Success = false,
                ErrorCode = "INTERNAL_ERROR",
                Message = "Failed to update question group"
            };
        }
    }

    public async Task<AdminOperationResult> UpdateQuestionAsync(
        Guid groupId,
        Guid questionId,
        UpdateQuestionRequest request)
    {
        try
        {
            var group = await _groupRepo.GetByIdWithQuestionsAsync(groupId);

            if (group == null)
            {
                return new AdminOperationResult
                {
                    Success = false,
                    ErrorCode = "GROUP_NOT_FOUND",
                    Message = "Question group not found"
                };
            }

            var question = group.Questions.FirstOrDefault(q => q.Id == questionId);

            if (question == null)
            {
                return new AdminOperationResult
                {
                    Success = false,
                    ErrorCode = "QUESTION_NOT_FOUND",
                    Message = "Question not found in this group"
                };
            }

            question.QuestionText = request.QuestionText;
            question.PointValue = request.PointValue;
            question.EstimatedTimeSeconds = request.EstimatedTimeSeconds;
            question.UpdatedAt = _timeProvider.GetUtcNow();

            await _questionRepo.UpdateAsync(question);
            await _questionRepo.SaveAsync();

            // Update group's UpdatedAt timestamp
            group.UpdatedAt = _timeProvider.GetUtcNow();
            await _groupRepo.UpdateAsync(group);
            await _groupRepo.SaveChangesAsync();

            _logger.LogInformation("Updated question {QuestionId} in group {GroupId}", questionId, groupId);

            return new AdminOperationResult
            {
                Success = true,
                Message = "Question updated successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating question {QuestionId} in group {GroupId}", questionId, groupId);
            return new AdminOperationResult
            {
                Success = false,
                ErrorCode = "INTERNAL_ERROR",
                Message = "Failed to update question"
            };
        }
    }

    public async Task<AdminOperationResult> ReorderQuestionsAsync(
        Guid groupId,
        ReorderQuestionsRequest request)
    {
        try
        {
            var group = await _groupRepo.GetByIdWithQuestionsAsync(groupId);

            if (group == null)
            {
                return new AdminOperationResult
                {
                    Success = false,
                    ErrorCode = "GROUP_NOT_FOUND",
                    Message = "Question group not found"
                };
            }

            // Validate all question IDs exist in group
            var questionIds = group.Questions.Select(q => q.Id).ToHashSet();
            var requestIds = request.QuestionOrder.Select(qo => qo.QuestionId).ToHashSet();

            if (!requestIds.SetEquals(questionIds))
            {
                return new AdminOperationResult
                {
                    Success = false,
                    ErrorCode = "INVALID_QUESTION_IDS",
                    Message = "All questions in the group must be included in the reorder request"
                };
            }

            // Validate unique new orders
            var newOrders = request.QuestionOrder.Select(qo => qo.NewOrder).ToList();
            if (newOrders.Distinct().Count() != newOrders.Count)
            {
                return new AdminOperationResult
                {
                    Success = false,
                    ErrorCode = "DUPLICATE_ORDER",
                    Message = "Each question must have a unique order"
                };
            }

            // Apply new orders
            foreach (var orderDto in request.QuestionOrder)
            {
                var question = group.Questions.First(q => q.Id == orderDto.QuestionId);
                question.GroupOrder = orderDto.NewOrder;
                question.UpdatedAt = _timeProvider.GetUtcNow();
            }

            await _questionRepo.UpdateRangeAsync(group.Questions);
            await _questionRepo.SaveAsync();

            group.UpdatedAt = _timeProvider.GetUtcNow();
            await _groupRepo.UpdateAsync(group);
            await _groupRepo.SaveChangesAsync();

            _logger.LogInformation("Reordered questions in group {GroupId}", groupId);

            return new AdminOperationResult
            {
                Success = true,
                Message = "Questions reordered successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering questions in group {GroupId}", groupId);
            return new AdminOperationResult
            {
                Success = false,
                ErrorCode = "INTERNAL_ERROR",
                Message = "Failed to reorder questions"
            };
        }
    }

    public async Task<AdminOperationResult> ActivateGroupAsync(Guid groupId)
    {
        try
        {
            var group = await _groupRepo.GetByIdAsync(groupId);

            if (group == null)
            {
                return new AdminOperationResult
                {
                    Success = false,
                    ErrorCode = "GROUP_NOT_FOUND",
                    Message = "Question group not found"
                };
            }

            // Deactivate all other groups of this test type
            await _groupRepo.DeactivateAllGroupsForTestTypeAsync(group.TestType);

            // Activate this group
            group.IsActive = true;
            group.UpdatedAt = _timeProvider.GetUtcNow();
            await _groupRepo.UpdateAsync(group);
            await _groupRepo.SaveChangesAsync();

            _logger.LogInformation("Activated question group {GroupId} for test type {TestType}",
                groupId, group.TestType);

            return new AdminOperationResult
            {
                Success = true,
                Message = "Question group activated. All other groups for this test type have been deactivated."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating group {GroupId}", groupId);
            return new AdminOperationResult
            {
                Success = false,
                ErrorCode = "INTERNAL_ERROR",
                Message = "Failed to activate question group"
            };
        }
    }

    public async Task<AdminOperationResult> DeactivateGroupAsync(Guid groupId)
    {
        try
        {
            var group = await _groupRepo.GetByIdAsync(groupId);

            if (group == null)
            {
                return new AdminOperationResult
                {
                    Success = false,
                    ErrorCode = "GROUP_NOT_FOUND",
                    Message = "Question group not found"
                };
            }

            // Check if this is the only active group for this test type
            var activeGroups = await _groupRepo.GetByTestTypeAsync(group.TestType, activeOnly: true);
            if (activeGroups.Count() == 1 && activeGroups.First().Id == groupId)
            {
                return new AdminOperationResult
                {
                    Success = false,
                    ErrorCode = "CANNOT_DEACTIVATE_ONLY_GROUP",
                    Message = "Cannot deactivate the only active group for this test type. Create or activate another group first."
                };
            }

            group.IsActive = false;
            group.UpdatedAt = _timeProvider.GetUtcNow();
            await _groupRepo.UpdateAsync(group);
            await _groupRepo.SaveChangesAsync();

            _logger.LogInformation("Deactivated question group {GroupId}", groupId);

            return new AdminOperationResult
            {
                Success = true,
                Message = "Question group deactivated"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating group {GroupId}", groupId);
            return new AdminOperationResult
            {
                Success = false,
                ErrorCode = "INTERNAL_ERROR",
                Message = "Failed to deactivate question group"
            };
        }
    }

    public async Task<AdminOperationResult> DeleteGroupAsync(Guid groupId)
    {
        try
        {
            var group = await _groupRepo.GetByIdAsync(groupId);

            if (group == null)
            {
                return new AdminOperationResult
                {
                    Success = false,
                    ErrorCode = "GROUP_NOT_FOUND",
                    Message = "Question group not found"
                };
            }

            // Check if group is used in any tests
            var isUsedInTests = await _groupRepo.IsGroupUsedInTestsAsync(groupId);
            if (isUsedInTests)
            {
                return new AdminOperationResult
                {
                    Success = false,
                    ErrorCode = "GROUP_IN_USE",
                    Message = "Cannot delete group because it is referenced by existing tests. Deactivate it instead."
                };
            }

            await _groupRepo.DeleteAsync(groupId);
            await _groupRepo.SaveChangesAsync();

            _logger.LogInformation("Deleted question group {GroupId}", groupId);

            return new AdminOperationResult
            {
                Success = true,
                Message = "Question group deleted successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group {GroupId}", groupId);
            return new AdminOperationResult
            {
                Success = false,
                ErrorCode = "INTERNAL_ERROR",
                Message = "Failed to delete question group"
            };
        }
    }
}
