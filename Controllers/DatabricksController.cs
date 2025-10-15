using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dignus.Candidate.Back.Controllers
{
    /// <summary>
    /// Controller for Databricks integration and Gupy data synchronization
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // TEMPORARILY DISABLED
    public class DatabricksController : ControllerBase
    {
        private readonly IDatabricksIntegrationService _databricksService;
        private readonly ILogger<DatabricksController> _logger;

        public DatabricksController(
            IDatabricksIntegrationService databricksService,
            ILogger<DatabricksController> logger)
        {
            _databricksService = databricksService;
            _logger = logger;
        }

        /// <summary>
        /// Start synchronization of data from Gupy via Databricks (candidates and jobs)
        /// </summary>
        /// <param name="batchSize">Number of records to process in each batch (default: 1000)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Synchronization status information</returns>
        [HttpPost("sync")]
        public async Task<ActionResult<SyncStatusDto>> SynchronizeData(
            [FromQuery] int batchSize = 1000,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting data synchronization with batch size {BatchSize}", batchSize);

                if (batchSize <= 0 || batchSize > 10000)
                {
                    return BadRequest("Batch size must be between 1 and 10000");
                }

                // Sync both candidates and jobs in sequence
                var candidateResult = await _databricksService.SynchronizeCandidatesAsync(batchSize, cancellationToken);
                var jobResult = await _databricksService.SynchronizeJobsAsync(cancellationToken);

                // Return combined status
                var combinedResult = new SyncStatusDto
                {
                    SyncId = Guid.NewGuid(),
                    Type = SyncType.Full,
                    Status = (candidateResult.Status == SyncStatus.Completed && jobResult.Status == SyncStatus.Completed) 
                        ? SyncStatus.Completed 
                        : SyncStatus.Running,
                    TotalRecords = candidateResult.TotalRecords + jobResult.TotalRecords,
                    ProcessedRecords = candidateResult.ProcessedRecords + jobResult.ProcessedRecords,
                    StartedAt = DateTimeOffset.UtcNow,
                    ErrorMessage = candidateResult.ErrorMessage ?? jobResult.ErrorMessage
                };
                
                return Ok(combinedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting data synchronization");
                return StatusCode(500, new { message = "Failed to start data synchronization", error = ex.Message });
            }
        }
    }
}