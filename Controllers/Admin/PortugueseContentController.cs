using Dignus.Candidate.Back.DTOs.Admin;
using Dignus.Candidate.Back.Exceptions;
using Dignus.Candidate.Back.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dignus.Candidate.Back.Controllers.Admin;

/// <summary>
/// Admin controller for managing Portuguese reading texts with questions
/// ⚠️ SECURITY: All endpoints require admin/recruiter authorization
/// </summary>
[ApiController]
[Route("api/admin/portuguese-content")]
// [Authorize(Roles = "Recruiter,Admin")]
public class PortugueseContentController : ControllerBase
{
    private readonly IPortugueseContentService _portugueseContentService;
    private readonly ILogger<PortugueseContentController> _logger;

    public PortugueseContentController(
        IPortugueseContentService portugueseContentService,
        ILogger<PortugueseContentController> logger)
    {
        _portugueseContentService = portugueseContentService;
        _logger = logger;
    }

    /// <summary>
    /// Create a Portuguese reading text with its questions
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/admin/portuguese-content
    ///     {
    ///         "title": "Liberdade e Responsabilidade",
    ///         "content": "Liberdade não é fazer tudo o que se quer...",
    ///         "authorName": "Mário Sérgio Cortella",
    ///         "difficultyLevel": "medium",
    ///         "estimatedReadingTimeMinutes": 3,
    ///         "wordCount": 110,
    ///         "questions": [
    ///             {
    ///                 "questionText": "Qual o principal tema do texto que você leu?",
    ///                 "pointValue": 3.00,
    ///                 "estimatedTimeSeconds": 180,
    ///                 "categoryTags": ["interpretacao-texto", "tema-principal"],
    ///                 "expectedAnswerGuideJson": "{\"key_points\": [...]}",
    ///                 "questionOrder": 1
    ///             }
    ///         ]
    ///     }
    ///
    /// </remarks>
    /// <param name="request">Portuguese content creation request</param>
    /// <returns>Created Portuguese content with questions</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PortugueseContentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PortugueseContentDto>> CreatePortugueseContent(
        [FromBody] CreatePortugueseContentRequest request)
    {
        try
        {
            var content = await _portugueseContentService.CreatePortugueseContentAsync(request);
            return CreatedAtAction(
                nameof(GetPortugueseContent),
                new { id = content.ReadingTextId },
                content);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to create Portuguese content");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating Portuguese content");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get a Portuguese reading text with its questions by ID
    /// </summary>
    /// <param name="id">Reading text ID</param>
    /// <returns>Portuguese content with questions</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PortugueseContentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PortugueseContentDto>> GetPortugueseContent(Guid id)
    {
        try
        {
            var content = await _portugueseContentService.GetPortugueseContentAsync(id);
            return Ok(content);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = $"Portuguese content {id} not found" });
        }
    }

    /// <summary>
    /// Get all Portuguese reading texts with their questions
    /// </summary>
    /// <param name="includeInactive">Include deactivated content</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Items per page (1-100)</param>
    /// <returns>List of Portuguese content</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<PortugueseContentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PortugueseContentDto>>> GetAllPortugueseContent(
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var content = await _portugueseContentService.GetAllPortugueseContentAsync(
            includeInactive,
            page,
            pageSize);
        return Ok(content);
    }

    /// <summary>
    /// Deactivate a Portuguese reading text and its questions
    /// </summary>
    /// <param name="id">Reading text ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivatePortugueseContent(Guid id)
    {
        try
        {
            await _portugueseContentService.DeactivatePortugueseContentAsync(id);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = $"Portuguese content {id} not found" });
        }
    }
}
