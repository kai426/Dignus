using Dignus.Candidate.Back.DTOs.Admin;
using Dignus.Candidate.Back.Services.Admin;
using Dignus.Data.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dignus.Candidate.Back.Controllers.Admin;

/// <summary>
/// Admin API for managing test question groups
/// </summary>
[ApiController]
[Route("api/admin/question-groups")]
// [Authorize(Roles = "Admin,Recruiter")]
[Produces("application/json")]
public class AdminQuestionGroupsController : ControllerBase
{
    private readonly IQuestionGroupAdminService _adminService;
    private readonly ILogger<AdminQuestionGroupsController> _logger;

    public AdminQuestionGroupsController(
        IQuestionGroupAdminService adminService,
        ILogger<AdminQuestionGroupsController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    /// <summary>
    /// List all question groups, optionally filtered by test type
    /// </summary>
    /// <param name="testType">Optional: Filter by test type (Portuguese, Math, Interview)</param>
    /// <param name="includeInactive">Include inactive groups (default: false)</param>
    [HttpGet]
    [ProducesResponseType(typeof(GetQuestionGroupsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetQuestionGroupsResponse>> GetQuestionGroups(
        [FromQuery] TestType? testType = null,
        [FromQuery] bool includeInactive = false)
    {
        var groups = await _adminService.GetQuestionGroupsAsync(testType, includeInactive);
        return Ok(new GetQuestionGroupsResponse { Groups = groups });
    }

    /// <summary>
    /// Get detailed information about a specific question group
    /// </summary>
    /// <param name="groupId">The ID of the question group</param>
    [HttpGet("{groupId}")]
    [ProducesResponseType(typeof(QuestionGroupDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestionGroupDetailDto>> GetQuestionGroupDetails(Guid groupId)
    {
        var group = await _adminService.GetQuestionGroupDetailsAsync(groupId);

        if (group == null)
        {
            return NotFound(new ErrorResponse
            {
                Error = "GROUP_NOT_FOUND",
                Message = "Question group not found"
            });
        }

        return Ok(group);
    }

    /// <summary>
    /// Create a new question group with questions
    /// </summary>
    /// <param name="request">Question group creation request</param>
    [HttpPost]
    [ProducesResponseType(typeof(CreateGroupResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateGroupResponse>> CreateQuestionGroup(
        [FromBody] CreateQuestionGroupRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _adminService.CreateQuestionGroupAsync(request);

        if (!result.Success)
        {
            return BadRequest(new ErrorResponse
            {
                Error = result.ErrorCode ?? "ERROR",
                Message = result.Message
            });
        }

        return CreatedAtAction(
            nameof(GetQuestionGroupDetails),
            new { groupId = result.GroupId },
            new CreateGroupResponse
            {
                Id = result.GroupId!.Value,
                Message = result.Message
            });
    }

    /// <summary>
    /// Update question group metadata (name, description, difficulty)
    /// </summary>
    /// <param name="groupId">The ID of the question group</param>
    /// <param name="request">Update request</param>
    [HttpPut("{groupId}")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateQuestionGroup(
        Guid groupId,
        [FromBody] UpdateQuestionGroupRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _adminService.UpdateQuestionGroupAsync(groupId, request);

        if (!result.Success)
        {
            return BadRequest(new ErrorResponse
            {
                Error = result.ErrorCode ?? "ERROR",
                Message = result.Message
            });
        }

        return Ok(new SuccessResponse
        {
            Success = true,
            Message = result.Message
        });
    }

    /// <summary>
    /// Update a specific question in a group
    /// </summary>
    /// <param name="groupId">The ID of the question group</param>
    /// <param name="questionId">The ID of the question to update</param>
    /// <param name="request">Update request</param>
    [HttpPut("{groupId}/questions/{questionId}")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateQuestion(
        Guid groupId,
        Guid questionId,
        [FromBody] UpdateQuestionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _adminService.UpdateQuestionAsync(groupId, questionId, request);

        if (!result.Success)
        {
            return BadRequest(new ErrorResponse
            {
                Error = result.ErrorCode ?? "ERROR",
                Message = result.Message
            });
        }

        return Ok(new SuccessResponse
        {
            Success = true,
            Message = result.Message
        });
    }

    /// <summary>
    /// Reorder questions within a group
    /// </summary>
    /// <param name="groupId">The ID of the question group</param>
    /// <param name="request">Reorder request with new question orders</param>
    [HttpPatch("{groupId}/questions/reorder")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ReorderQuestions(
        Guid groupId,
        [FromBody] ReorderQuestionsRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _adminService.ReorderQuestionsAsync(groupId, request);

        if (!result.Success)
        {
            return BadRequest(new ErrorResponse
            {
                Error = result.ErrorCode ?? "ERROR",
                Message = result.Message
            });
        }

        return Ok(new SuccessResponse
        {
            Success = true,
            Message = result.Message
        });
    }

    /// <summary>
    /// Activate a question group (deactivates all others of the same test type)
    /// </summary>
    /// <param name="groupId">The ID of the question group to activate</param>
    [HttpPatch("{groupId}/activate")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ActivateGroup(Guid groupId)
    {
        var result = await _adminService.ActivateGroupAsync(groupId);

        if (!result.Success)
        {
            return BadRequest(new ErrorResponse
            {
                Error = result.ErrorCode ?? "ERROR",
                Message = result.Message
            });
        }

        return Ok(new SuccessResponse
        {
            Success = true,
            Message = result.Message
        });
    }

    /// <summary>
    /// Deactivate a question group (soft delete)
    /// </summary>
    /// <param name="groupId">The ID of the question group to deactivate</param>
    [HttpPatch("{groupId}/deactivate")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeactivateGroup(Guid groupId)
    {
        var result = await _adminService.DeactivateGroupAsync(groupId);

        if (!result.Success)
        {
            return BadRequest(new ErrorResponse
            {
                Error = result.ErrorCode ?? "ERROR",
                Message = result.Message
            });
        }

        return Ok(new SuccessResponse
        {
            Success = true,
            Message = result.Message
        });
    }

    /// <summary>
    /// Permanently delete a question group (only if not used in any tests)
    /// </summary>
    /// <param name="groupId">The ID of the question group to delete</param>
    [HttpDelete("{groupId}")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteGroup(Guid groupId)
    {
        var result = await _adminService.DeleteGroupAsync(groupId);

        if (!result.Success)
        {
            return BadRequest(new ErrorResponse
            {
                Error = result.ErrorCode ?? "ERROR",
                Message = result.Message
            });
        }

        return Ok(new SuccessResponse
        {
            Success = true,
            Message = result.Message
        });
    }
}

/// <summary>
/// Standard success response
/// </summary>
public class SuccessResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
}

/// <summary>
/// Standard error response
/// </summary>
public class ErrorResponse
{
    public string Error { get; set; } = null!;
    public string Message { get; set; } = null!;
}
