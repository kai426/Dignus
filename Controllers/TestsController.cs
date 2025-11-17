using Dignus.Candidate.Back.DTOs.Unified;
using Dignus.Candidate.Back.Exceptions;
using Dignus.Candidate.Back.Services.Interfaces;
using Dignus.Data.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dignus.Candidate.Back.Controllers;

/// <summary>
/// Unified test controller for all test types (v2 API)
/// </summary>
[ApiController]
[Route("api/v2/tests")]
// [Authorize]
public class TestsController : ControllerBase
{
    private readonly ITestService _testService;
    private readonly IVideoResponseService _videoResponseService;
    private readonly IQuestionResponseService _questionResponseService;
    private readonly ILogger<TestsController> _logger;

    public TestsController(
        ITestService testService,
        IVideoResponseService videoResponseService,
        IQuestionResponseService questionResponseService,
        ILogger<TestsController> logger)
    {
        _testService = testService;
        _videoResponseService = videoResponseService;
        _questionResponseService = questionResponseService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new test for a candidate (any type)
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v2/tests
    ///     {
    ///         "candidateId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///         "testType": 1,
    ///         "difficultyLevel": "medium"
    ///     }
    ///
    /// Test Types:
    /// - 0 = Portuguese
    /// - 1 = Math
    /// - 2 = Psychology
    /// - 3 = VisualRetention
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(TestInstanceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TestInstanceDto>> CreateTest(
        [FromBody] CreateTestInstanceRequest request)
    {
        try
        {
            // IDOR Protection: Ensure authenticated user matches candidateId
            if (!IsAuthorizedForCandidate(request.CandidateId))
            {
                return Forbid();
            }

            var test = await _testService.CreateTestAsync(request);
            return CreatedAtAction(nameof(GetTest), new { testId = test.Id }, test);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create test for candidate {CandidateId}", request.CandidateId);
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get a test by ID
    /// </summary>
    [HttpGet("{testId}")]
    [ProducesResponseType(typeof(TestInstanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TestInstanceDto>> GetTest(
        Guid testId,
        [FromQuery] Guid candidateId)
    {
        if (!IsAuthorizedForCandidate(candidateId))
        {
            return Forbid();
        }

        try
        {
            var test = await _testService.GetTestByIdAsync(testId, candidateId);
            return Ok(test);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = $"Test {testId} not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Get all tests for a candidate (optionally filter by type)
    /// </summary>
    [HttpGet("candidate/{candidateId}")]
    [ProducesResponseType(typeof(List<TestInstanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<TestInstanceDto>>> GetCandidateTests(
        Guid candidateId,
        [FromQuery] TestType? testType = null)
    {
        if (!IsAuthorizedForCandidate(candidateId))
        {
            return Forbid();
        }

        var tests = await _testService.GetCandidateTestsAsync(candidateId, testType);
        return Ok(tests);
    }

    /// <summary>
    /// Get candidate progress across all test types (v2)
    /// </summary>
    /// <remarks>
    /// Returns comprehensive progress information including:
    /// - Overall completion percentage
    /// - Number of completed tests vs total tests
    /// - Individual test progress with status, score, and completion date
    ///
    /// Test statuses:
    /// - NotStarted: Test has not been created/started yet
    /// - InProgress: Test was started but not yet submitted
    /// - Completed: Test was submitted (includes Submitted, UnderReview, Reviewed statuses)
    /// </remarks>
    [HttpGet("candidate/{candidateId}/progress")]
    [ProducesResponseType(typeof(DTOs.ProgressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DTOs.ProgressDto>> GetCandidateProgress(Guid candidateId)
    {
        if (!IsAuthorizedForCandidate(candidateId))
        {
            return Forbid();
        }

        try
        {
            // Get all test instances for this candidate
            var tests = await _testService.GetCandidateTestsAsync(candidateId, null);

            // Define all test types (5 total)
            var allTestTypes = new[]
            {
                TestType.Portuguese,
                TestType.Math,
                TestType.Psychology,
                TestType.VisualRetention,
                TestType.Interview
            };

            // Calculate progress for each test type
            var testProgress = new Dictionary<string, DTOs.TestProgressDto>();
            int completedCount = 0;

            foreach (var testType in allTestTypes)
            {
                var testInstance = tests.FirstOrDefault(t => t.TestType == testType);

                if (testInstance == null)
                {
                    // Test not started yet
                    testProgress[testType.ToString()] = new DTOs.TestProgressDto
                    {
                        TestType = testType.ToString(),
                        Status = "NotStarted",
                        IsCompleted = false,
                        Score = null,
                        CompletedAt = null
                    };
                }
                else
                {
                    // Determine if test is completed
                    bool isCompleted = testInstance.Status == Data.Models.TestStatus.Submitted ||
                                     testInstance.Status == Data.Models.TestStatus.Approved ||
                                     testInstance.Status == Data.Models.TestStatus.Rejected;

                    if (isCompleted)
                    {
                        completedCount++;
                    }

                    testProgress[testType.ToString()] = new DTOs.TestProgressDto
                    {
                        TestType = testType.ToString(),
                        Status = testInstance.Status.ToString(),
                        IsCompleted = isCompleted,
                        Score = testInstance.Score,
                        CompletedAt = testInstance.CompletedAt?.DateTime
                    };
                }
            }

            // Calculate completion percentage
            decimal completionPercentage = allTestTypes.Length > 0
                ? (decimal)completedCount / allTestTypes.Length * 100
                : 0;

            var progress = new DTOs.ProgressDto
            {
                CandidateId = candidateId,
                CompletionPercentage = completionPercentage,
                CompletedTests = completedCount,
                TotalTests = allTestTypes.Length,
                TestProgress = testProgress
            };

            return Ok(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving progress for candidate {CandidateId}", candidateId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Start a test (changes status from NotStarted to InProgress)
    /// </summary>
    [HttpPost("{testId}/start")]
    [ProducesResponseType(typeof(TestInstanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TestInstanceDto>> StartTest(
        Guid testId,
        [FromQuery] Guid candidateId)
    {
        if (!IsAuthorizedForCandidate(candidateId))
        {
            return Forbid();
        }

        try
        {
            var test = await _testService.StartTestAsync(testId, candidateId);
            return Ok(test);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = $"Test {testId} not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Submit test answers
    /// </summary>
    [HttpPost("{testId}/submit")]
    [ProducesResponseType(typeof(TestSubmissionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TestSubmissionResultDto>> SubmitTest(
        Guid testId,
        [FromBody] SubmitTestRequest request)
    {
        if (testId != request.TestId)
        {
            return BadRequest(new { error = "Test ID mismatch" });
        }

        if (!IsAuthorizedForCandidate(request.CandidateId))
        {
            return Forbid();
        }

        try
        {
            var result = await _testService.SubmitTestAsync(request);
            return Ok(result);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = $"Test {testId} not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in SubmitTest for TestId {TestId}", testId);
            return StatusCode(500, new {
                error = "Internal server error during test submission",
                message = ex.Message,
                type = ex.GetType().Name,
                stackTrace = ex.StackTrace
            });
        }
    }

    /// <summary>
    /// Get test questions (snapshotted, no correct answers)
    /// </summary>
    [HttpGet("{testId}/questions")]
    [ProducesResponseType(typeof(List<QuestionSnapshotDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<QuestionSnapshotDto>>> GetTestQuestions(
        Guid testId,
        [FromQuery] Guid candidateId)
    {
        if (!IsAuthorizedForCandidate(candidateId))
        {
            return Forbid();
        }

        try
        {
            var questions = await _testService.GetTestQuestionsAsync(testId, candidateId);

            // Security check in development: ensure no correct answers are exposed
#if DEBUG
            foreach (var q in questions)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(q);
                if (json.Contains("CorrectAnswer", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogCritical("⚠️ SECURITY VIOLATION: Correct answer exposed in question {QuestionId}!", q.Id);
                    throw new InvalidOperationException("Security violation: Correct answer exposed!");
                }
            }
#endif

            return Ok(questions);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = $"Test {testId} not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Get test status (progress, remaining time, etc.)
    /// </summary>
    [HttpGet("{testId}/status")]
    [ProducesResponseType(typeof(TestStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TestStatusDto>> GetTestStatus(
        Guid testId,
        [FromQuery] Guid candidateId)
    {
        if (!IsAuthorizedForCandidate(candidateId))
        {
            return Forbid();
        }

        try
        {
            var status = await _testService.GetTestStatusAsync(testId, candidateId);
            return Ok(status);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = $"Test {testId} not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Check if a candidate can start a specific test type
    /// </summary>
    [HttpGet("candidate/{candidateId}/can-start/{testType}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<bool>> CanStartTest(
        Guid candidateId,
        TestType testType)
    {
        if (!IsAuthorizedForCandidate(candidateId))
        {
            return Forbid();
        }

        var canStart = await _testService.CanStartTestAsync(candidateId, testType);
        return Ok(canStart);
    }

    // ============================================================
    // VIDEO RESPONSE ENDPOINTS
    // ============================================================

    /// <summary>
    /// Upload a video response for a test question (Portuguese/Interview tests)
    /// </summary>
    [HttpPost("{testId}/videos")]
    [ProducesResponseType(typeof(VideoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<VideoResponseDto>> UploadVideo(
        Guid testId,
        [FromQuery] Guid candidateId,
        [FromForm] UploadVideoRequest request)
    {
        // Populate TestId and CandidateId from URL path and query parameters
        request.TestId = testId;
        request.CandidateId = candidateId;

        if (!IsAuthorizedForCandidate(request.CandidateId))
        {
            return Forbid();
        }

        try
        {
            var videoResponse = await _videoResponseService.UploadVideoResponseAsync(request);
            return CreatedAtAction(nameof(GetVideoResponse),
                new { testId, videoId = videoResponse.Id, candidateId },
                videoResponse);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all video responses for a test
    /// </summary>
    [HttpGet("{testId}/videos")]
    [ProducesResponseType(typeof(List<VideoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<VideoResponseDto>>> GetTestVideos(
        Guid testId,
        [FromQuery] Guid candidateId)
    {
        if (!IsAuthorizedForCandidate(candidateId))
        {
            return Forbid();
        }

        try
        {
            var videos = await _videoResponseService.GetTestVideoResponsesAsync(testId, candidateId);
            return Ok(videos);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = $"Test {testId} not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Get a specific video response by ID
    /// </summary>
    [HttpGet("{testId}/videos/{videoId}")]
    [ProducesResponseType(typeof(VideoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VideoResponseDto>> GetVideoResponse(
        Guid testId,
        Guid videoId,
        [FromQuery] Guid candidateId)
    {
        if (!IsAuthorizedForCandidate(candidateId))
        {
            return Forbid();
        }

        try
        {
            var video = await _videoResponseService.GetVideoResponseAsync(videoId, candidateId);
            return Ok(video);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = $"Video response {videoId} not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Get a secure, time-limited URL for viewing a video
    /// </summary>
    [HttpGet("{testId}/videos/{videoId}/url")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> GetSecureVideoUrl(
        Guid testId,
        Guid videoId,
        [FromQuery] Guid candidateId,
        [FromQuery] int expirationMinutes = 60)
    {
        if (!IsAuthorizedForCandidate(candidateId))
        {
            return Forbid();
        }

        try
        {
            var secureUrl = await _videoResponseService.GetSecureVideoUrlAsync(videoId, candidateId, expirationMinutes);
            return Ok(new { url = secureUrl });
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = $"Video response {videoId} not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Delete a video response (before test submission)
    /// </summary>
    [HttpDelete("{testId}/videos/{videoId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteVideo(
        Guid testId,
        Guid videoId,
        [FromQuery] Guid candidateId)
    {
        if (!IsAuthorizedForCandidate(candidateId))
        {
            return Forbid();
        }

        try
        {
            await _videoResponseService.DeleteVideoResponseAsync(videoId, candidateId);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = $"Video response {videoId} not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ============================================================
    // QUESTION RESPONSE ENDPOINTS (Multiple Choice)
    // ============================================================

    /// <summary>
    /// Submit or update multiple choice answers for a test (batch operation)
    /// </summary>
    [HttpPost("{testId}/answers")]
    [ProducesResponseType(typeof(List<QuestionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<QuestionResponseDto>>> SubmitAnswers(
        Guid testId,
        [FromBody] List<QuestionAnswerSubmission> answers,
        [FromQuery] Guid candidateId)
    {
        if (!IsAuthorizedForCandidate(candidateId))
        {
            return Forbid();
        }

        try
        {
            var responses = await _questionResponseService.SubmitAnswersAsync(testId, candidateId, answers);
            return Ok(responses);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = $"Test {testId} not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all question responses for a test
    /// </summary>
    [HttpGet("{testId}/answers")]
    [ProducesResponseType(typeof(List<QuestionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<QuestionResponseDto>>> GetTestAnswers(
        Guid testId,
        [FromQuery] Guid candidateId)
    {
        if (!IsAuthorizedForCandidate(candidateId))
        {
            return Forbid();
        }

        try
        {
            var responses = await _questionResponseService.GetTestResponsesAsync(testId, candidateId);
            return Ok(responses);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = $"Test {testId} not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Update a specific question response (before test submission)
    /// </summary>
    [HttpPut("{testId}/answers/{responseId}")]
    [ProducesResponseType(typeof(QuestionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestionResponseDto>> UpdateAnswer(
        Guid testId,
        Guid responseId,
        [FromBody] QuestionAnswerSubmission updatedAnswer,
        [FromQuery] Guid candidateId)
    {
        if (!IsAuthorizedForCandidate(candidateId))
        {
            return Forbid();
        }

        try
        {
            var response = await _questionResponseService.UpdateResponseAsync(responseId, candidateId, updatedAnswer);
            return Ok(response);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = $"Question response {responseId} not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a question response (before test submission)
    /// </summary>
    [HttpDelete("{testId}/answers/{responseId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteAnswer(
        Guid testId,
        Guid responseId,
        [FromQuery] Guid candidateId)
    {
        if (!IsAuthorizedForCandidate(candidateId))
        {
            return Forbid();
        }

        try
        {
            await _questionResponseService.DeleteResponseAsync(responseId, candidateId);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = $"Question response {responseId} not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Helper method for IDOR protection
    /// Verifies that the authenticated user is authorized to access data for the specified candidate
    /// </summary>
    /// <returns>true if authorized, false otherwise</returns>
    private bool IsAuthorizedForCandidate(Guid candidateId)
    {
        // Development mode: If user is not authenticated, allow all access
        // This happens when DisableAuthentication is set to true in config
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogDebug("IDOR protection bypassed: Development mode (no authentication)");
            return true;
        }

        // Production mode: Verify authenticated user can only access their own data
        var authenticatedUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(authenticatedUserId))
        {
            _logger.LogWarning("IDOR protection: No NameIdentifier claim found in token");
            return false;
        }

        if (!Guid.TryParse(authenticatedUserId, out var parsedUserId))
        {
            _logger.LogWarning("IDOR protection: Failed to parse NameIdentifier claim '{UserId}' as Guid", authenticatedUserId);
            return false;
        }

        var isAuthorized = parsedUserId == candidateId;

        if (!isAuthorized)
        {
            _logger.LogWarning(
                "IDOR protection: User {AuthenticatedUserId} attempted to access candidate {RequestedCandidateId}",
                parsedUserId, candidateId);
        }

        return isAuthorized;
    }
}
