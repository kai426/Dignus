using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;
using Dignus.Candidate.Back.Exceptions;

namespace Dignus.Candidate.Back.Controllers
{
    /// <summary>
    /// Controller for job management operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    //TODO: Remove after testing - Authorization temporarily disabled for manual testing
    // [Authorize] // Re-enabled for job search integration
    [Produces("application/json")]
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;
        private readonly ILogger<JobController> _logger;

        public JobController(
            IJobService jobService,
            ILogger<JobController> logger)
        {
            _jobService = jobService;
            _logger = logger;
        }

        /// <summary>
        /// Search and filter jobs with pagination
        /// </summary>
        /// <param name="request">Search criteria and pagination parameters</param>
        /// <returns>Paginated list of jobs</returns>
        [HttpGet]
        public async Task<ActionResult<PagedResult<JobListingDto>>> GetJobs([FromQuery] JobSearchRequest request)
        {
            try
            {
                var jobs = await _jobService.SearchJobsAsync(request);
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving jobs with search criteria");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get job by ID
        /// </summary>
        /// <param name="id">Job ID</param>
        /// <returns>Job details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<JobListingDto>> GetJob(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest("Job ID is required");
                }

                var job = await _jobService.GetJobByIdAsync(id);
                if (job == null)
                {
                    return NotFound($"Job with ID {id} not found");
                }

                return Ok(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving job {JobId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get job statistics for dashboard
        /// </summary>
        /// <returns>Dashboard statistics</returns>
        [HttpGet("statistics")]
        public async Task<ActionResult<JobStatisticsDto>> GetJobStatistics()
        {
            try
            {
                var statistics = await _jobService.GetJobStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving job statistics");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Apply to a job
        /// </summary>
        /// <param name="id">Job ID</param>
        /// <param name="request">Application details</param>
        /// <returns>Success status</returns>
        [HttpPost("{id}/apply")]
        public async Task<ActionResult> ApplyToJob(string id, [FromBody] ApplyToJobDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest("Job ID is required");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _jobService.ApplyToJobAsync(id, request);
                if (!success)
                {
                    return BadRequest("Failed to apply to job");
                }

                return Ok(new { message = "Application submitted successfully" });
            }
            catch (JobApplicationException ex)
            {
                _logger.LogWarning(ex, "Job application failed: {ErrorCode} for job {JobId} and candidate {CandidateId}",
                    ex.ErrorCode, id, request.CandidateId);
                return BadRequest(new {
                    error = ex.ErrorCode,
                    message = ex.UserFriendlyMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying to job {JobId} for candidate {CandidateId}", id, request.CandidateId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get candidates who applied to a specific job (recruiter functionality)
        /// </summary>
        /// <param name="id">Job ID</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns>Paginated list of candidates</returns>
        [HttpGet("{id}/candidates")]
        //TODO: Remove after testing - Authorization temporarily disabled for manual testing
        // [Authorize(Policy = "RecruiterAccess")]
        public async Task<ActionResult<PagedResult<JobListingDto>>> GetJobCandidates(
            string id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest("Job ID is required");
                }

                if (page < 1 || pageSize < 1 || pageSize > 100)
                {
                    return BadRequest("Invalid pagination parameters");
                }

                var candidates = await _jobService.GetJobCandidatesAsync(id, page, pageSize);
                return Ok(candidates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving candidates for job {JobId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}