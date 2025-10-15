using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Dignus.Candidate.Back.Controllers
{
    /// <summary>
    /// Controller for media file management operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // TEMPORARILY DISABLED
    public class MediaController : ControllerBase
    {
        private readonly IMediaService _mediaService;
        private readonly ILogger<MediaController> _logger;

        public MediaController(IMediaService mediaService, ILogger<MediaController> logger)
        {
            _mediaService = mediaService;
            _logger = logger;
        }

        /// <summary>
        /// Uploads an audio file for a test
        /// </summary>
        /// <param name="uploadAudioDto">The audio upload data</param>
        /// <returns>The created audio submission</returns>
        [HttpPost("audio")]
        [ProducesResponseType(typeof(AudioSubmissionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AudioSubmissionDto>> UploadAudio([FromForm] UploadAudioDto uploadAudioDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _mediaService.UploadAudioAsync(uploadAudioDto);
                return CreatedAtAction(nameof(GetAudioSubmission), new { audioId = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid audio upload request");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading audio file");
                return StatusCode(500, "An error occurred while uploading the audio file");
            }
        }

        /// <summary>
        /// Uploads a video file for a test
        /// </summary>
        /// <param name="uploadVideoDto">The video upload data</param>
        /// <returns>The created video interview</returns>
        [HttpPost("video")]
        [ProducesResponseType(typeof(VideoInterviewDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VideoInterviewDto>> UploadVideo([FromForm] UploadVideoDto uploadVideoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _mediaService.UploadVideoAsync(uploadVideoDto);
                return CreatedAtAction(nameof(GetVideoInterview), new { videoId = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid video upload request");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading video file");
                return StatusCode(500, "An error occurred while uploading the video file");
            }
        }

        /// <summary>
        /// Gets audio submissions for the authenticated candidate
        /// </summary>
        /// <param name="candidateId">The candidate identifier</param>
        /// <returns>List of audio submissions</returns>
        [HttpGet("audio/candidate/{candidateId}")]
        [ProducesResponseType(typeof(IEnumerable<AudioSubmissionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AudioSubmissionDto>>> GetAudioSubmissions(Guid candidateId)
        {
            try
            {
                // IDOR protection - ensure user can only access their own data
                if (!await IsAuthorizedForCandidate(candidateId))
                {
                    return Forbid();
                }

                var audioSubmissions = await _mediaService.GetAudioSubmissionsByCandidateIdAsync(candidateId);
                return Ok(audioSubmissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audio submissions for candidate {CandidateId}", candidateId);
                return StatusCode(500, "An error occurred while retrieving audio submissions");
            }
        }

        /// <summary>
        /// Gets video interviews for the authenticated candidate
        /// </summary>
        /// <param name="candidateId">The candidate identifier</param>
        /// <returns>List of video interviews</returns>
        [HttpGet("video/candidate/{candidateId}")]
        [ProducesResponseType(typeof(IEnumerable<VideoInterviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<VideoInterviewDto>>> GetVideoInterviews(Guid candidateId)
        {
            try
            {
                // IDOR protection - ensure user can only access their own data
                if (!await IsAuthorizedForCandidate(candidateId))
                {
                    return Forbid();
                }

                var videoInterviews = await _mediaService.GetVideoInterviewsByCandidateIdAsync(candidateId);
                return Ok(videoInterviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving video interviews for candidate {CandidateId}", candidateId);
                return StatusCode(500, "An error occurred while retrieving video interviews");
            }
        }

        /// <summary>
        /// Gets a specific audio submission by ID
        /// </summary>
        /// <param name="audioId">The audio submission identifier</param>
        /// <returns>The audio submission if found</returns>
        [HttpGet("audio/{audioId}")]
        [ProducesResponseType(typeof(AudioSubmissionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AudioSubmissionDto>> GetAudioSubmission(Guid audioId)
        {
            try
            {
                var candidateId = GetCurrentCandidateId();
                if (candidateId == null)
                {
                    return Forbid();
                }

                var audioSubmission = await _mediaService.GetAudioSubmissionByIdAsync(audioId, candidateId.Value);
                if (audioSubmission == null)
                {
                    return NotFound();
                }

                return Ok(audioSubmission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audio submission {AudioId}", audioId);
                return StatusCode(500, "An error occurred while retrieving the audio submission");
            }
        }

        /// <summary>
        /// Gets a specific video interview by ID
        /// </summary>
        /// <param name="videoId">The video interview identifier</param>
        /// <returns>The video interview if found</returns>
        [HttpGet("video/{videoId}")]
        [ProducesResponseType(typeof(VideoInterviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VideoInterviewDto>> GetVideoInterview(Guid videoId)
        {
            try
            {
                var candidateId = GetCurrentCandidateId();
                if (candidateId == null)
                {
                    return Forbid();
                }

                var videoInterview = await _mediaService.GetVideoInterviewByIdAsync(videoId, candidateId.Value);
                if (videoInterview == null)
                {
                    return NotFound();
                }

                return Ok(videoInterview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving video interview {VideoId}", videoId);
                return StatusCode(500, "An error occurred while retrieving the video interview");
            }
        }

        /// <summary>
        /// Deletes an audio submission
        /// </summary>
        /// <param name="audioId">The audio submission identifier</param>
        /// <returns>Success if deleted, NotFound if not found or not owned by candidate</returns>
        [HttpDelete("audio/{audioId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteAudioSubmission(Guid audioId)
        {
            try
            {
                var candidateId = GetCurrentCandidateId();
                if (candidateId == null)
                {
                    return Forbid();
                }

                var success = await _mediaService.DeleteAudioSubmissionAsync(audioId, candidateId.Value);
                if (!success)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting audio submission {AudioId}", audioId);
                return StatusCode(500, "An error occurred while deleting the audio submission");
            }
        }

        /// <summary>
        /// Deletes a video interview
        /// </summary>
        /// <param name="videoId">The video interview identifier</param>
        /// <returns>Success if deleted, NotFound if not found or not owned by candidate</returns>
        [HttpDelete("video/{videoId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteVideoInterview(Guid videoId)
        {
            try
            {
                var candidateId = GetCurrentCandidateId();
                if (candidateId == null)
                {
                    return Forbid();
                }

                var success = await _mediaService.DeleteVideoInterviewAsync(videoId, candidateId.Value);
                if (!success)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting video interview {VideoId}", videoId);
                return StatusCode(500, "An error occurred while deleting the video interview");
            }
        }

        /// <summary>
        /// Gets upload configuration limits
        /// </summary>
        /// <returns>Upload limits and allowed formats</returns>
        [HttpGet("upload-limits")]
        [ProducesResponseType(typeof(UploadLimitsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<UploadLimitsDto> GetUploadLimits()
        {
            try
            {
                var limits = new UploadLimitsDto
                {
                    MaxAudioSizeBytes = _mediaService.GetMaxAudioFileSize(),
                    MaxVideoSizeBytes = _mediaService.GetMaxVideoFileSize()
                };

                return Ok(limits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upload limits");
                return StatusCode(500, "An error occurred while retrieving upload limits");
            }
        }

        private Guid? GetCurrentCandidateId()
        {
            var candidateIdClaim = User.FindFirst("candidateId")?.Value;
            if (candidateIdClaim != null && Guid.TryParse(candidateIdClaim, out var candidateId))
            {
                return candidateId;
            }
            return null;
        }

        private Task<bool> IsAuthorizedForCandidate(Guid candidateId)
        {
            var currentCandidateId = GetCurrentCandidateId();
            return Task.FromResult(currentCandidateId.HasValue && currentCandidateId.Value == candidateId);
        }
    }

    /// <summary>
    /// DTO for upload limits information
    /// </summary>
    public class UploadLimitsDto
    {
        /// <summary>
        /// Maximum audio file size in bytes
        /// </summary>
        public long MaxAudioSizeBytes { get; set; }

        /// <summary>
        /// Maximum video file size in bytes
        /// </summary>
        public long MaxVideoSizeBytes { get; set; }
    }
}