using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dignus.Candidate.Back.Controllers
{
    /// <summary>
    /// Controller for managing Gupy data synchronization from Databricks
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GupySyncController : ControllerBase
    {
        private readonly IDatabricksSqlService _databricksSqlService;
        private readonly ILogger<GupySyncController> _logger;

        public GupySyncController(
            IDatabricksSqlService databricksSqlService,
            ILogger<GupySyncController> logger)
        {
            _databricksSqlService = databricksSqlService;
            _logger = logger;
        }

        /// <summary>
        /// Syncs in-progress applications from Gupy (via Databricks) to local database
        /// </summary>
        /// <remarks>
        /// This endpoint:
        /// 1. Queries gupy_aplicacoes table for applications with STATUS = 'inProgress' from the latest extraction
        /// 2. Gets candidate IDs from those applications
        /// 3. Queries gupy_candidatos table for candidate details
        /// 4. Creates or updates candidate and job records in local database
        ///
        /// Sample response:
        /// ```json
        /// {
        ///   "totalApplications": 150,
        ///   "candidatesFound": 145,
        ///   "candidatesCreated": 20,
        ///   "candidatesUpdated": 125,
        ///   "jobsCreated": 5,
        ///   "jobsUpdated": 10,
        ///   "errors": [],
        ///   "processedAt": "2025-01-04T10:30:00Z"
        /// }
        /// ```
        /// </remarks>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Sync operation results</returns>
        /// <response code="200">Sync completed successfully (may contain errors for individual records)</response>
        /// <response code="500">Sync operation failed</response>
        [HttpPost("sync")]
        // [Authorize] // TODO: Enable authorization and require admin/recruiter role
        [ProducesResponseType(typeof(GupySyncResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GupySyncResponseDto>> SyncInProgressApplications(
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting Gupy sync operation via API request");

            try
            {
                var result = await _databricksSqlService.SyncInProgressApplicationsAsync(cancellationToken);

                if (result.Errors.Any())
                {
                    _logger.LogWarning(
                        "Sync completed with {ErrorCount} errors. Created: {Created}, Updated: {Updated}",
                        result.Errors.Count,
                        result.CandidatesCreated,
                        result.CandidatesUpdated);
                }
                else
                {
                    _logger.LogInformation(
                        "Sync completed successfully. Created: {Created}, Updated: {Updated}",
                        result.CandidatesCreated,
                        result.CandidatesUpdated);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sync operation failed with exception");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        error = "SYNC_FAILED",
                        message = "Falha ao sincronizar dados do Gupy",
                        details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Gets in-progress applications from Gupy without syncing to database
        /// </summary>
        /// <remarks>
        /// This endpoint only queries Databricks and returns the raw application data
        /// without creating or updating any records in the local database.
        /// Useful for previewing data before sync.
        /// </remarks>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of in-progress applications</returns>
        /// <response code="200">Applications retrieved successfully</response>
        /// <response code="500">Query failed</response>
        [HttpGet("applications/in-progress")]
        // [Authorize] // TODO: Enable authorization and require admin/recruiter role
        [ProducesResponseType(typeof(List<GupyAplicacaoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<GupyAplicacaoDto>>> GetInProgressApplications(
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting in-progress applications from Databricks");

            try
            {
                var applications = await _databricksSqlService.GetInProgressApplicationsAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} in-progress applications", applications.Count);

                return Ok(applications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve in-progress applications");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        error = "QUERY_FAILED",
                        message = "Falha ao consultar aplicações do Gupy",
                        details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Gets candidate details by IDs from Gupy (via Databricks)
        /// </summary>
        /// <remarks>
        /// This endpoint queries the gupy_candidatos table for specific candidate IDs
        /// without syncing to the database. Useful for previewing candidate data.
        ///
        /// Sample request:
        /// ```
        /// GET /api/gupysync/candidates?ids=cand123&amp;ids=cand456&amp;ids=cand789
        /// ```
        /// </remarks>
        /// <param name="ids">List of candidate IDs to fetch</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of candidate details</returns>
        /// <response code="200">Candidates retrieved successfully</response>
        /// <response code="400">No candidate IDs provided</response>
        /// <response code="500">Query failed</response>
        [HttpGet("candidates")]
        // [Authorize] // TODO: Enable authorization and require admin/recruiter role
        [ProducesResponseType(typeof(List<GupyCandidatoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<GupyCandidatoDto>>> GetCandidatesByIds(
            [FromQuery] List<string> ids,
            CancellationToken cancellationToken = default)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest(new
                {
                    error = "MISSING_IDS",
                    message = "Pelo menos um ID de candidato deve ser fornecido"
                });
            }

            _logger.LogInformation("Getting {Count} candidates from Databricks", ids.Count);

            try
            {
                var candidates = await _databricksSqlService.GetCandidatesByIdsAsync(ids, cancellationToken);

                _logger.LogInformation("Retrieved {Count} candidates", candidates.Count);

                return Ok(candidates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve candidates");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        error = "QUERY_FAILED",
                        message = "Falha ao consultar candidatos do Gupy",
                        details = ex.Message
                    });
            }
        }
    }
}