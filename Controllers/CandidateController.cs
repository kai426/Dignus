using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;
using Dignus.Data.Models;
using Azure.Storage.Blobs;
using System.ComponentModel.DataAnnotations;

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
        private readonly BlobServiceClient _blobServiceClient;

        public CandidateController(
            ICandidateService candidateService,
            ILogger<CandidateController> logger,
            BlobServiceClient blobServiceClient)
        {
            _candidateService = candidateService;
            _logger = logger;
            _blobServiceClient = blobServiceClient;
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
        /// Update candidate PCD (Person with Disability) status
        /// </summary>
        /// <param name="id">Candidate ID</param>
        /// <param name="request">PCD status update request</param>
        /// <returns>Updated candidate</returns>
        [HttpPatch("{id:guid}/pcd")]
        public async Task<ActionResult<CandidateDto>> UpdateCandidatePCDStatus(Guid id, [FromBody] UpdatePCDRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Request body is required");
                }

                var updateDto = new UpdateCandidateDto { IsPCD = request.IsPCD };
                var candidate = await _candidateService.UpdateCandidateAsync(id, updateDto);

                if (candidate == null)
                {
                    return NotFound($"Candidate with ID {id} not found");
                }

                return Ok(candidate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating candidate PCD status {CandidateId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update candidate foreigner status and country of origin
        /// </summary>
        /// <param name="id">Candidate ID</param>
        /// <param name="request">Foreigner status update request</param>
        /// <returns>Updated candidate</returns>
        [HttpPatch("{id:guid}/foreigner")]
        public async Task<ActionResult<CandidateDto>> UpdateCandidateForeignerStatus(Guid id, [FromBody] UpdateForeignerRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Request body is required");
                }

                // If the candidate is marked as foreigner, country of origin should be provided
                if (request.IsForeigner == true && string.IsNullOrWhiteSpace(request.CountryOfOrigin))
                {
                    return BadRequest("CountryOfOrigin is required when IsForeigner is true");
                }

                var updateDto = new UpdateCandidateDto
                {
                    IsForeigner = request.IsForeigner,
                    CountryOfOrigin = request.IsForeigner == true ? request.CountryOfOrigin : null
                };

                var candidate = await _candidateService.UpdateCandidateAsync(id, updateDto);

                if (candidate == null)
                {
                    return NotFound($"Candidate with ID {id} not found");
                }

                return Ok(candidate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating candidate foreigner status {CandidateId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Upload PCD supporting document for a candidate
        /// </summary>
        /// <param name="id">Candidate ID</param>
        /// <param name="document">PCD document file (PDF or DOCX, max 10MB)</param>
        /// <returns>Updated candidate with document information</returns>
        [HttpPost("{id:guid}/pcd-document")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<CandidateDto>> UploadPCDDocument(Guid id, IFormFile document)
        {
            try
            {
                if (document == null || document.Length == 0)
                {
                    return BadRequest(new { error = "Document file is required" });
                }

                // Validate file size (max 10MB)
                const long maxFileSize = 10 * 1024 * 1024; // 10MB
                if (document.Length > maxFileSize)
                {
                    return BadRequest(new { error = $"File size exceeds maximum allowed size of 10MB" });
                }

                // Validate file extension
                var extension = Path.GetExtension(document.FileName)?.ToLowerInvariant();
                var allowedExtensions = new[] { ".pdf", ".docx" };
                if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { error = "Invalid file format. Only PDF and DOCX files are allowed" });
                }

                // Validate MIME type
                var allowedMimeTypes = new[] {
                    "application/pdf",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                };
                if (!allowedMimeTypes.Contains(document.ContentType))
                {
                    return BadRequest(new { error = "Invalid file type. Only PDF and DOCX files are allowed" });
                }

                // Get candidate to verify it exists
                var candidate = await _candidateService.GetCandidateByIdAsync(id);
                if (candidate == null)
                {
                    return NotFound($"Candidate with ID {id} not found");
                }

                // Upload to Azure Blob Storage
                var containerName = "pcd-documents";
                var blobName = $"candidate-{id}/pcd-document-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{extension}";

                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();

                var blobClient = containerClient.GetBlobClient(blobName);

                using (var stream = document.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                var blobUrl = blobClient.Uri.ToString();

                // Update candidate with document information through the service
                var candidateEntity = await _candidateService.UpdateCandidateDocumentAsync(
                    id,
                    blobUrl,
                    document.FileName
                );

                if (candidateEntity == null)
                {
                    return NotFound($"Candidate with ID {id} not found");
                }

                _logger.LogInformation("PCD document uploaded successfully for candidate {CandidateId}", id);

                return Ok(candidateEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading PCD document for candidate {CandidateId}", id);
                return StatusCode(500, new { error = "Internal server error while uploading document" });
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

        /// <summary>
        /// Request model for updating candidate PCD status
        /// </summary>
        public class UpdatePCDRequest
        {
            /// <summary>
            /// Indicates if the candidate is a Person with Disability (PCD)
            /// </summary>
            public bool IsPCD { get; set; }
        }

        /// <summary>
        /// Request model for updating candidate foreigner status
        /// </summary>
        public class UpdateForeignerRequest
        {
            /// <summary>
            /// Indicates if the candidate is a foreigner (not Brazilian)
            /// </summary>
            public bool IsForeigner { get; set; }

            /// <summary>
            /// Country of origin (required if IsForeigner is true)
            /// ISO 3166-1 alpha-2 code (e.g., "US", "AR", "CO") or full country name
            /// </summary>
            [StringLength(100)]
            public string? CountryOfOrigin { get; set; }
        }
    }
}