using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;

namespace Dignus.Candidate.Back.Controllers
{
    /// <summary>
    /// Controller for Visual Retention Test operations with fixed questions
    /// </summary>
    [ApiController]
    [Route("api/tests/visual-retention")]
    // [Authorize] // TEMPORARILY DISABLED
    [Produces("application/json")]
    public class VisualRetentionTestController : ControllerBase
    {
        private readonly IVisualRetentionTestService _visualRetentionService;
        private readonly ILogger<VisualRetentionTestController> _logger;

        public VisualRetentionTestController(
            IVisualRetentionTestService visualRetentionService,
            ILogger<VisualRetentionTestController> logger)
        {
            _visualRetentionService = visualRetentionService;
            _logger = logger;
        }

        /// <summary>
        /// Get or create visual retention test for candidate
        /// </summary>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>Visual retention test with all fixed questions</returns>
        [HttpGet("candidate/{candidateId:guid}")]
        public async Task<ActionResult<VisualRetentionTestDto>> GetVisualRetentionTest(Guid candidateId)
        {
            try
            {
                var test = await _visualRetentionService.GetOrCreateVisualRetentionTestAsync(candidateId);
                if (test == null)
                {
                    return NotFound($"Unable to create or retrieve visual retention test for candidate {candidateId}");
                }

                return Ok(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting visual retention test for candidate {CandidateId}", candidateId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get visual retention test by ID
        /// </summary>
        /// <param name="testId">Test ID</param>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>Visual retention test details</returns>
        [HttpGet("{testId:guid}")]
        public async Task<ActionResult<VisualRetentionTestDto>> GetVisualRetentionTestById(
            Guid testId,
            [FromQuery] Guid candidateId)
        {
            try
            {
                var test = await _visualRetentionService.GetVisualRetentionTestByIdAsync(testId, candidateId);
                if (test == null)
                {
                    return NotFound($"Visual retention test with ID {testId} not found for candidate {candidateId}");
                }

                return Ok(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting visual retention test {TestId} for candidate {CandidateId}", testId, candidateId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Start visual retention test (marks as in progress)
        /// </summary>
        /// <param name="testId">Test ID</param>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>Started test</returns>
        [HttpPost("{testId:guid}/start")]
        public async Task<ActionResult<VisualRetentionTestDto>> StartVisualRetentionTest(
            Guid testId,
            [FromBody] Guid candidateId)
        {
            try
            {
                var test = await _visualRetentionService.StartVisualRetentionTestAsync(testId, candidateId);
                if (test == null)
                {
                    return NotFound($"Visual retention test with ID {testId} not found for candidate {candidateId}");
                }

                return Ok(test);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while starting visual retention test");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting visual retention test {TestId} for candidate {CandidateId}", testId, candidateId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Submit visual retention test responses (fixed answers only - A, B, C, D, E)
        /// </summary>
        /// <param name="submission">Test responses</param>
        /// <returns>Success status</returns>
        [HttpPost("submit")]
        public async Task<ActionResult> SubmitVisualRetentionTest([FromBody] VisualRetentionSubmissionDto submission)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _visualRetentionService.SubmitTestResponsesAsync(submission);
                if (!success)
                {
                    return BadRequest("Failed to submit visual retention test");
                }

                return Ok(new { message = "Visual retention test submitted successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument while submitting visual retention test");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting visual retention test for candidate {CandidateId}",
                    submission.CandidateId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get visual retention test status for candidate
        /// </summary>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>Test status information</returns>
        [HttpGet("candidate/{candidateId:guid}/status")]
        public async Task<ActionResult<object>> GetVisualRetentionTestStatus(Guid candidateId)
        {
            try
            {
                var status = await _visualRetentionService.GetTestStatusAsync(candidateId);
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting visual retention test status for candidate {CandidateId}", candidateId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Check if candidate can start visual retention test
        /// </summary>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>Boolean indicating if test can be started</returns>
        [HttpGet("candidate/{candidateId:guid}/can-start")]
        public async Task<ActionResult<bool>> CanStartVisualRetentionTest(Guid candidateId)
        {
            try
            {
                var canStart = await _visualRetentionService.CanStartTestAsync(candidateId);
                return Ok(canStart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if candidate {CandidateId} can start visual retention test", candidateId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}