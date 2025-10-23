using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Dignus.Candidate.Back.Controllers
{
    /// <summary>
    /// Controller for evaluation and reporting operations
    /// </summary>
    [ApiController]
    [Route("api/evaluation")]
    //TODO: Remove after testing - Authorization temporarily disabled for manual testing
    // [Authorize] // Re-enabled for evaluation system integration testing
    [Produces("application/json")]
    public class EvaluationController : ControllerBase
    {
        private readonly IEvaluationEngineService _evaluationEngineService;
        private readonly IReportGenerationService _reportGenerationService;
        private readonly IBenchmarkService _benchmarkService;
        private readonly ILogger<EvaluationController> _logger;

        public EvaluationController(
            IEvaluationEngineService evaluationEngineService,
            IReportGenerationService reportGenerationService,
            IBenchmarkService benchmarkService,
            ILogger<EvaluationController> logger)
        {
            _evaluationEngineService = evaluationEngineService;
            _reportGenerationService = reportGenerationService;
            _benchmarkService = benchmarkService;
            _logger = logger;
        }

        /// <summary>
        /// Gets comprehensive evaluation for a candidate
        /// </summary>
        /// <param name="candidateId">Candidate identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Comprehensive candidate evaluation</returns>
        [HttpGet("candidates/{candidateId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CandidateEvaluationDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CandidateEvaluationDto>> GetCandidateEvaluation(
            Guid candidateId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                // IDOR protection - ensure user can access this candidate's data
                if (!await IsAuthorizedForCandidate(candidateId))
                {
                    _logger.LogWarning("Unauthorized access attempt to candidate {CandidateId}", candidateId);
                    return StatusCode(403, "Access denied for this candidate evaluation");
                }

                var evaluation = await _evaluationEngineService.EvaluateCandidateAsync(candidateId, cancellationToken);
                return Ok(evaluation);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Candidate not found: {CandidateId}", candidateId);
                return NotFound($"Candidate not found: {candidateId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting evaluation for candidate {CandidateId}", candidateId);
                return StatusCode(500, "An error occurred while retrieving the candidate evaluation");
            }
        }


        /// <summary>
        /// Refreshes evaluation for a candidate
        /// </summary>
        /// <param name="candidateId">Candidate identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated candidate evaluation</returns>
        [HttpPost("candidates/{candidateId:guid}/refresh")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CandidateEvaluationDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CandidateEvaluationDto>> RefreshCandidateEvaluation(
            Guid candidateId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!await IsAuthorizedForCandidate(candidateId))
                {
                    return StatusCode(403, "Access denied for this candidate evaluation");
                }

                var evaluation = await _evaluationEngineService.RefreshEvaluationAsync(candidateId, cancellationToken);
                
                _logger.LogInformation("Refreshed evaluation for candidate {CandidateId}", candidateId);
                
                return Ok(evaluation);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Candidate not found: {CandidateId}", candidateId);
                return NotFound($"Candidate not found: {candidateId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing evaluation for candidate {CandidateId}", candidateId);
                return StatusCode(500, "An error occurred while refreshing the candidate evaluation");
            }
        }

        /// <summary>
        /// Gets evaluation status for a candidate
        /// </summary>
        /// <param name="candidateId">Candidate identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Evaluation status</returns>
        [HttpGet("candidates/{candidateId:guid}/status")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EvaluationStatus))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EvaluationStatus>> GetEvaluationStatus(
            Guid candidateId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!await IsAuthorizedForCandidate(candidateId))
                {
                    return StatusCode(403, "Access denied for this candidate evaluation");
                }

                var status = await _evaluationEngineService.GetEvaluationStatusAsync(candidateId, cancellationToken);
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting evaluation status for candidate {CandidateId}", candidateId);
                return StatusCode(500, "An error occurred while retrieving the evaluation status");
            }
        }












        /// <summary>
        /// Health check endpoint for evaluation service
        /// </summary>
        /// <returns>Health status</returns>
        [HttpGet("health")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            return Ok(new 
            { 
                status = "healthy", 
                timestamp = DateTimeOffset.UtcNow,
                service = "Evaluation Service"
            });
        }

        #region Private Helper Methods

        /// <summary>
        /// Checks if current user is authorized to access candidate data
        /// </summary>
        /// <param name="candidateId">Candidate identifier</param>
        /// <returns>True if authorized, false otherwise</returns>
        private Task<bool> IsAuthorizedForCandidate(Guid candidateId)
        {
            // IDOR protection logic
            var currentCandidateId = GetCurrentCandidateId();
            var isRecruiter = User.HasClaim("scope", "recruiter.access");
            
            // Recruiters can access any candidate, candidates can only access their own data
            if (isRecruiter || (currentCandidateId.HasValue && currentCandidateId.Value == candidateId))
            {
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }

        /// <summary>
        /// Gets the current candidate ID from user claims
        /// </summary>
        /// <returns>Candidate ID if found, null otherwise</returns>
        private Guid? GetCurrentCandidateId()
        {
            var candidateIdClaim = User.FindFirst("candidate_id")?.Value;
            if (Guid.TryParse(candidateIdClaim, out var candidateId))
            {
                return candidateId;
            }
            return null;
        }

        #endregion
    }

    /// <summary>
    /// DTO for batch report request
    /// </summary>
    public class BatchReportRequestDto
    {
        /// <summary>
        /// List of candidate IDs to generate reports for
        /// </summary>
        [Required]
        public List<Guid> CandidateIds { get; set; } = new();

        /// <summary>
        /// Type of report to generate for all candidates
        /// </summary>
        public ReportType ReportType { get; set; } = ReportType.Summary;
    }
}