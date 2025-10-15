using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;

namespace Dignus.Candidate.Back.Controllers
{
    /// <summary>
    /// Controller for questionnaire operations
    /// </summary>
    [ApiController]
    [Route("api/tests/questionnaire")]
    // [Authorize] // TEMPORARILY DISABLED FOR TESTING
    [Produces("application/json")]
    public class QuestionnaireController : ControllerBase
    {
        private readonly IQuestionnaireService _questionnaireService;
        private readonly ILogger<QuestionnaireController> _logger;

        public QuestionnaireController(
            IQuestionnaireService questionnaireService,
            ILogger<QuestionnaireController> logger)
        {
            _questionnaireService = questionnaireService;
            _logger = logger;
        }

        /// <summary>
        /// Get questionnaire structure (all sections and questions) - Returns ALL fixed psychology questions
        /// </summary>
        /// <returns>Complete questionnaire structure with fixed questions</returns>
        [HttpGet]
        public async Task<ActionResult<QuestionnaireDto>> GetQuestionnaire()
        {
            try
            {
                // Get ALL psychology questions (fixed set)
                var questionnaire = await _questionnaireService.GetCompleteQuestionnaireAsync();
                return Ok(questionnaire);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting complete questionnaire structure");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get questionnaire progress for candidate
        /// </summary>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>Progress information</returns>
        [HttpGet("candidate/{candidateId:guid}/progress")]
        public async Task<ActionResult<QuestionnaireProgressDto>> GetProgress(Guid candidateId)
        {
            try
            {
                var progress = await _questionnaireService.GetProgressAsync(candidateId);
                return Ok(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting questionnaire progress for candidate {CandidateId}", candidateId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Initialize questionnaire for candidate
        /// </summary>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>Initial progress</returns>
        [HttpPost("candidate/{candidateId:guid}/initialize")]
        public async Task<ActionResult<QuestionnaireProgressDto>> Initialize(Guid candidateId)
        {
            try
            {
                var canStart = await _questionnaireService.CanStartQuestionnaireAsync(candidateId);
                if (!canStart)
                {
                    return BadRequest("Candidate cannot start questionnaire at this time");
                }

                var progress = await _questionnaireService.InitializeQuestionnaireAsync(candidateId);
                return Ok(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing questionnaire for candidate {CandidateId}", candidateId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get specific section
        /// </summary>
        /// <param name="sectionId">Section ID (0-8)</param>
        /// <returns>Section details</returns>
        [HttpGet("sections/{sectionId:int}")]
        public async Task<ActionResult<QuestionnaireSectionDto>> GetSection(int sectionId)
        {
            try
            {
                if (sectionId < 0 || sectionId > 8)
                {
                    return BadRequest("Section ID must be between 0 and 8");
                }

                var section = await _questionnaireService.GetSectionAsync(sectionId);
                if (section == null)
                {
                    return NotFound($"Section {sectionId} not found");
                }

                return Ok(section);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting section {SectionId}", sectionId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Save section responses
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="request">Section responses</param>
        /// <returns>Success status</returns>
        [HttpPost("sections/{sectionId:int}/responses")]
        public async Task<ActionResult> SaveSectionResponse(
            int sectionId, 
            [FromBody] SaveSectionResponseDto request)
        {
            try
            {
                if (sectionId < 0 || sectionId > 8)
                {
                    return BadRequest("Section ID must be between 0 and 8");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _questionnaireService.SaveSectionResponseAsync(
                    request.CandidateId, 
                    sectionId, 
                    request.Responses);

                if (!success)
                {
                    return BadRequest("Failed to save section responses");
                }

                return Ok(new { message = "Section responses saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving responses for section {SectionId}, candidate {CandidateId}", 
                    sectionId, request.CandidateId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Submit completed questionnaire
        /// </summary>
        /// <param name="submission">Complete questionnaire responses</param>
        /// <returns>Success status</returns>
        [HttpPost("submit")]
        public async Task<ActionResult> SubmitQuestionnaire([FromBody] SubmitQuestionnaireDto submission)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _questionnaireService.SubmitQuestionnaireAsync(submission);
                if (!success)
                {
                    return BadRequest("Failed to submit questionnaire");
                }

                return Ok(new { message = "Questionnaire submitted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting questionnaire for candidate {CandidateId}", submission.CandidateId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Check if candidate can start questionnaire
        /// </summary>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>Boolean indicating if questionnaire can be started</returns>
        [HttpGet("candidate/{candidateId:guid}/can-start")]
        public async Task<ActionResult<bool>> CanStartQuestionnaire(Guid candidateId)
        {
            try
            {
                var canStart = await _questionnaireService.CanStartQuestionnaireAsync(candidateId);
                return Ok(canStart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if candidate {CandidateId} can start questionnaire", candidateId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Submit all psychology test responses at once
        /// </summary>
        /// <param name="submission">Complete response set</param>
        /// <returns>Success status</returns>
        [HttpPost("submit-psychology-responses")]
        public async Task<ActionResult> SubmitPsychologyResponses([FromBody] PsychologyTestSubmissionDto submission)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Process fixed question responses (A, B, C, D, E format)
                var success = await _questionnaireService.ProcessFixedResponsesAsync(submission);
                if (!success)
                {
                    return BadRequest("Failed to process psychology test responses");
                }

                return Ok(new { message = "Psychology test responses submitted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting psychology test responses for candidate {CandidateId}",
                    submission.CandidateId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}