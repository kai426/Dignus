using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;

namespace Dignus.Candidate.Back.Controllers
{
    /// <summary>
    /// Controller for Portuguese test questions management (CRUD operations)
    /// </summary>
    [ApiController]
    [Route("api/portuguese-questions")]
    [Authorize] // Requires authentication for admin operations
    [Produces("application/json")]
    public class PortugueseQuestionsController : ControllerBase
    {
        private readonly IPortugueseQuestionService _questionService;
        private readonly ILogger<PortugueseQuestionsController> _logger;

        public PortugueseQuestionsController(
            IPortugueseQuestionService questionService,
            ILogger<PortugueseQuestionsController> logger)
        {
            _questionService = questionService;
            _logger = logger;
        }

        /// <summary>
        /// Get all Portuguese questions with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <param name="searchTerm">Search term for question text</param>
        /// <returns>Paginated list of Portuguese questions</returns>
        [HttpGet]
        public async Task<ActionResult<object>> GetPortugueseQuestions(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var (questions, totalCount) = await _questionService.GetQuestionsAsync(
                    pageNumber, pageSize, searchTerm);

                var result = new
                {
                    Data = questions,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Portuguese questions");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get Portuguese question by ID
        /// </summary>
        /// <param name="id">Question ID</param>
        /// <returns>Portuguese question details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<QuestionDto>> GetPortugueseQuestion(string id)
        {
            try
            {
                var question = await _questionService.GetQuestionByIdAsync(id);
                if (question == null)
                {
                    return NotFound($"Portuguese question with ID {id} not found");
                }

                return Ok(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Portuguese question {QuestionId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create new Portuguese question
        /// </summary>
        /// <param name="createQuestionDto">Question creation data</param>
        /// <returns>Created question</returns>
        [HttpPost]
        public async Task<ActionResult<QuestionDto>> CreatePortugueseQuestion(
            [FromBody] CreatePortugueseQuestionDto createQuestionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var question = await _questionService.CreateQuestionAsync(createQuestionDto);
                return CreatedAtAction(nameof(GetPortugueseQuestion),
                    new { id = question.Id }, question);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating Portuguese question");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Portuguese question");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update Portuguese question
        /// </summary>
        /// <param name="id">Question ID</param>
        /// <param name="updateQuestionDto">Question update data</param>
        /// <returns>Updated question</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<QuestionDto>> UpdatePortugueseQuestion(
            string id, [FromBody] UpdatePortugueseQuestionDto updateQuestionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var question = await _questionService.UpdateQuestionAsync(id, updateQuestionDto);
                if (question == null)
                {
                    return NotFound($"Portuguese question with ID {id} not found");
                }

                return Ok(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Portuguese question {QuestionId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete Portuguese question
        /// </summary>
        /// <param name="id">Question ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePortugueseQuestion(string id)
        {
            try
            {
                var success = await _questionService.DeleteQuestionAsync(id);
                if (!success)
                {
                    return NotFound($"Portuguese question with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Portuguese question {QuestionId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Bulk import Portuguese questions
        /// </summary>
        /// <param name="questions">List of questions to import</param>
        /// <returns>Import result summary</returns>
        [HttpPost("bulk-import")]
        public async Task<ActionResult<object>> BulkImportQuestions(
            [FromBody] List<CreatePortugueseQuestionDto> questions)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _questionService.BulkImportQuestionsAsync(questions);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk import of Portuguese questions");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}