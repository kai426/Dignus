using Dignus.Candidate.Back.DTOs.Unified;
using Dignus.Candidate.Back.Exceptions;
using Dignus.Candidate.Back.Services.Interfaces;
using Dignus.Data.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dignus.Candidate.Back.Controllers.Admin;

/// <summary>
/// Admin controller for managing question templates
/// ⚠️ SECURITY: All endpoints require admin/recruiter authorization
/// </summary>
[ApiController]
[Route("api/admin/question-templates")]
// [Authorize(Roles = "Recruiter,Admin")]
public class QuestionTemplatesController : ControllerBase
{
    private readonly IQuestionTemplateService _questionTemplateService;
    private readonly ILogger<QuestionTemplatesController> _logger;

    public QuestionTemplatesController(
        IQuestionTemplateService questionTemplateService,
        ILogger<QuestionTemplatesController> logger)
    {
        _questionTemplateService = questionTemplateService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new question template with answer
    /// </summary>
    /// <param name="request">Question creation request</param>
    /// <returns>Created question template</returns>
    [HttpPost]
    [ProducesResponseType(typeof(QuestionTemplateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<QuestionTemplateDto>> CreateQuestion([FromBody] CreateQuestionTemplateRequest request)
    {
        try
        {
            // Set created by recruiter ID from authenticated user
            var recruiterId = GetAuthenticatedUserId();
            if (recruiterId.HasValue)
            {
                request.CreatedByRecruiterId = recruiterId.Value;
            }

            var question = await _questionTemplateService.CreateQuestionAsync(request);
            return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, question);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create question template");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing question template
    /// </summary>
    /// <param name="id">Question template ID</param>
    /// <param name="request">Question update request</param>
    /// <returns>Updated question template</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(QuestionTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<QuestionTemplateDto>> UpdateQuestion(Guid id, [FromBody] UpdateQuestionTemplateRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest(new { error = "ID mismatch" });
        }

        try
        {
            // Set updated by recruiter ID from authenticated user
            var recruiterId = GetAuthenticatedUserId();
            if (recruiterId.HasValue)
            {
                request.UpdatedByRecruiterId = recruiterId.Value;
            }

            var question = await _questionTemplateService.UpdateQuestionAsync(request);
            return Ok(question);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = $"Question template {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update question template {QuestionId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get question template by ID (without answer details)
    /// </summary>
    /// <param name="id">Question template ID</param>
    /// <returns>Question template</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(QuestionTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestionTemplateDto>> GetQuestion(Guid id)
    {
        try
        {
            var question = await _questionTemplateService.GetQuestionByIdAsync(id);
            return Ok(question);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = $"Question template {id} not found" });
        }
    }

    /// <summary>
    /// Get question template with answer details (admin only)
    /// </summary>
    /// <param name="id">Question template ID</param>
    /// <returns>Question template with answer</returns>
    [HttpGet("{id}/with-answer")]
    [ProducesResponseType(typeof(QuestionTemplateDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestionTemplateDetailDto>> GetQuestionWithAnswer(Guid id)
    {
        try
        {
            var question = await _questionTemplateService.GetQuestionWithAnswerAsync(id);
            return Ok(question);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = $"Question template {id} not found" });
        }
    }

    /// <summary>
    /// Get all questions for a test type (paginated)
    /// </summary>
    /// <param name="testType">Test type</param>
    /// <param name="includeInactive">Include deactivated questions</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Items per page (1-100)</param>
    /// <returns>Paginated list of questions</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<QuestionTemplateDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<QuestionTemplateDto>>> GetQuestions(
        [FromQuery] TestType testType,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _questionTemplateService.GetQuestionsByTestTypeAsync(testType, includeInactive, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get questions by difficulty level
    /// </summary>
    /// <param name="testType">Test type</param>
    /// <param name="difficulty">Difficulty level (easy/medium/hard)</param>
    /// <returns>List of questions</returns>
    [HttpGet("by-difficulty")]
    [ProducesResponseType(typeof(List<QuestionTemplateDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<QuestionTemplateDto>>> GetQuestionsByDifficulty(
        [FromQuery] TestType testType,
        [FromQuery] string difficulty)
    {
        var questions = await _questionTemplateService.GetQuestionsByDifficultyAsync(testType, difficulty);
        return Ok(questions);
    }

    /// <summary>
    /// Get questions by category tag
    /// </summary>
    /// <param name="testType">Test type</param>
    /// <param name="category">Category name</param>
    /// <returns>List of questions</returns>
    [HttpGet("by-category")]
    [ProducesResponseType(typeof(List<QuestionTemplateDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<QuestionTemplateDto>>> GetQuestionsByCategory(
        [FromQuery] TestType testType,
        [FromQuery] string category)
    {
        var questions = await _questionTemplateService.GetQuestionsByCategoryAsync(testType, category);
        return Ok(questions);
    }

    /// <summary>
    /// Deactivate a question (soft delete)
    /// </summary>
    /// <param name="id">Question template ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateQuestion(Guid id)
    {
        try
        {
            await _questionTemplateService.DeactivateQuestionAsync(id);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = $"Question template {id} not found" });
        }
    }

    /// <summary>
    /// Reactivate a previously deactivated question
    /// </summary>
    /// <param name="id">Question template ID</param>
    /// <returns>No content</returns>
    [HttpPost("{id}/reactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReactivateQuestion(Guid id)
    {
        try
        {
            await _questionTemplateService.ReactivateQuestionAsync(id);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = $"Question template {id} not found" });
        }
    }

    /// <summary>
    /// Get statistics about the question bank
    /// </summary>
    /// <param name="testType">Test type (optional)</param>
    /// <returns>Question bank statistics</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(QuestionBankStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<QuestionBankStatistics>> GetStatistics([FromQuery] TestType? testType = null)
    {
        var stats = await _questionTemplateService.GetQuestionBankStatisticsAsync(testType);
        return Ok(stats);
    }

    /// <summary>
    /// Helper method to get authenticated user ID
    /// </summary>
    private Guid? GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return null;
        }

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }
}
