using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;
using Dignus.Data.Models;

namespace Dignus.Candidate.Back.Controllers
{
    /// <summary>
    /// Controller for candidate management operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // TEMPORARILY DISABLED
    [Produces("application/json")]
    public class CandidateController : ControllerBase
    {
        private readonly ICandidateService _candidateService;
        private readonly ILogger<CandidateController> _logger;

        public CandidateController(
            ICandidateService candidateService,
            ILogger<CandidateController> logger)
        {
            _candidateService = candidateService;
            _logger = logger;
        }

        /// <summary>
        /// Get candidate by ID
        /// </summary>
        /// <param name="id">Candidate ID</param>
        /// <returns>Candidate information</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CandidateDto>> GetCandidate(Guid id)
        {
            try
            {
                var candidate = await _candidateService.GetCandidateByIdAsync(id);
                if (candidate == null)
                {
                    return NotFound($"Candidate with ID {id} not found");
                }

                return Ok(candidate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving candidate {CandidateId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new candidate
        /// </summary>
        /// <param name="createCandidateDto">Candidate creation data</param>
        /// <returns>Created candidate</returns>
        [HttpPost]
        public async Task<ActionResult<CandidateDto>> CreateCandidate([FromBody] CreateCandidateDto createCandidateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var candidate = await _candidateService.CreateCandidateAsync(createCandidateDto);
                return CreatedAtAction(nameof(GetCandidate), new { id = candidate.Id }, candidate);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating candidate");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating candidate");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update an existing candidate
        /// </summary>
        /// <param name="id">Candidate ID</param>
        /// <param name="updateCandidateDto">Candidate update data</param>
        /// <returns>Updated candidate</returns>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<CandidateDto>> UpdateCandidate(Guid id, [FromBody] UpdateCandidateDto updateCandidateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var candidate = await _candidateService.UpdateCandidateAsync(id, updateCandidateDto);
                if (candidate == null)
                {
                    return NotFound($"Candidate with ID {id} not found");
                }

                return Ok(candidate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating candidate {CandidateId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Search candidates with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for name or email</param>
        /// <returns>Paginated candidate list</returns>
        [HttpGet("search")]
        public async Task<ActionResult<object>> SearchCandidates(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var (candidates, totalCount) = await _candidateService.GetCandidatesAsync(pageNumber, pageSize);
                
                var result = new
                {
                    Data = candidates,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching candidates");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get candidate profile (simplified view for candidate frontend)
        /// </summary>
        /// <param name="id">Candidate ID</param>
        /// <returns>Candidate profile information</returns>
        [HttpGet("{id:guid}/profile")]
        public async Task<ActionResult<object>> GetCandidateProfile(Guid id)
        {
            try
            {
                var candidate = await _candidateService.GetCandidateByIdAsync(id);
                if (candidate == null)
                {
                    return NotFound($"Candidate with ID {id} not found");
                }

                // Return simplified profile for candidate frontend
                var profile = new
                {
                    candidate.Id,
                    candidate.Name,
                    candidate.Email,
                    candidate.Status
                };

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving candidate profile {CandidateId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update candidate status
        /// </summary>
        /// <param name="id">Candidate ID</param>
        /// <param name="request">Status update request</param>
        /// <returns>Updated candidate</returns>
        [HttpPatch("{id:guid}/status")]
        public async Task<ActionResult<CandidateDto>> UpdateCandidateStatus(Guid id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                if (request?.Status == null)
                {
                    return BadRequest("Status is required");
                }

                var updateDto = new UpdateCandidateDto { Status = request.Status };
                var candidate = await _candidateService.UpdateCandidateAsync(id, updateDto);
                
                if (candidate == null)
                {
                    return NotFound($"Candidate with ID {id} not found");
                }

                return Ok(candidate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating candidate status {CandidateId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get job information for a candidate
        /// </summary>
        /// <param name="id">Candidate ID</param>
        /// <returns>Job information for the candidate</returns>
        [HttpGet("{id:guid}/job")]
        public async Task<ActionResult<CandidateJobDto>> GetCandidateJob(Guid id)
        {
            try
            {
                var candidateJob = await _candidateService.GetCandidateJobAsync(id);
                if (candidateJob == null)
                {
                    return NotFound($"Job information not found for candidate {id}");
                }

                return Ok(candidateJob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving job for candidate {CandidateId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Request model for updating candidate status
        /// </summary>
        public class UpdateStatusRequest
        {
            public CandidateStatus Status { get; set; }
        }
    }
}