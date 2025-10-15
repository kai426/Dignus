using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;

namespace Dignus.Candidate.Back.Controllers
{
    /// <summary>
    /// Controller for test management operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // TEMPORARILY DISABLED
    [Produces("application/json")]
    public class TestController : ControllerBase
    {
        private readonly ITestService _testService;
        private readonly IMediaService _mediaService;
        private readonly ILogger<TestController> _logger;

        public TestController(
            ITestService testService,
            IMediaService mediaService,
            ILogger<TestController> logger)
        {
            _testService = testService;
            _mediaService = mediaService;
            _logger = logger;
        }

        /// <summary>
        /// Get test by ID for a specific candidate
        /// </summary>
        /// <param name="testId">Test ID</param>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>Test information</returns>
        [HttpGet("{testId:guid}")]
        public async Task<ActionResult<BaseTestDto>> GetTest(Guid testId, [FromQuery] Guid candidateId)
        {
            try
            {
                var test = await _testService.GetTestByIdAsync(testId, candidateId);
                if (test == null)
                {
                    return NotFound($"Test with ID {testId} not found for candidate {candidateId}");
                }

                return Ok(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving test {TestId} for candidate {CandidateId}", testId, candidateId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get or create specific test type for candidate
        /// </summary>
        /// <param name="testType">Test type (portuguese, math, psychology, visualretention)</param>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>Test instance with questions</returns>
        [HttpGet("{testType}/candidate/{candidateId:guid}")]
        public async Task<ActionResult<BaseTestDto>> GetOrCreateTestByType(string testType, Guid candidateId)
        {
            try
            {
                // Validate test type
                var validTypes = new[] { "portuguese", "math", "psychology", "visualretention" };
                if (!validTypes.Contains(testType.ToLower()))
                {
                    return BadRequest($"Invalid test type. Valid types: {string.Join(", ", validTypes)}");
                }

                var test = await _testService.GetOrCreateTestAsync(candidateId, testType);
                if (test == null)
                {
                    return NotFound($"Unable to create or retrieve {testType} test for candidate {candidateId}");
                }

                return Ok(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {TestType} test for candidate {CandidateId}", testType, candidateId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get all tests for a candidate, creating new ones if none exist
        /// </summary>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>List of tests for the candidate</returns>
        [HttpGet("candidate/{candidateId:guid}")]
        public async Task<ActionResult<IEnumerable<BaseTestDto>>> GetTestsByCandidate(Guid candidateId)
        {
            try
            {
                var tests = await _testService.GetOrCreateTestsForCandidateAsync(candidateId);
                return Ok(tests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tests for candidate {CandidateId}", candidateId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new test for a candidate
        /// </summary>
        /// <param name="createTestDto">Test creation data</param>
        /// <returns>Created test</returns>
        [HttpPost]
        public async Task<ActionResult<BaseTestDto>> CreateTest([FromBody] CreateTestDto createTestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var test = await _testService.CreateTestAsync(createTestDto);
                return CreatedAtAction(nameof(GetTest), new { testId = test.Id, candidateId = test.CandidateId }, test);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating test");
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument while creating test");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Start a test for a candidate
        /// </summary>
        /// <param name="testId">Test ID</param>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>Started test with questions</returns>
        [HttpPost("{testId:guid}/start")]
        public async Task<ActionResult<BaseTestDto>> StartTest(Guid testId, [FromBody] Guid candidateId)
        {
            try
            {
                var test = await _testService.StartTestAsync(testId, candidateId);
                if (test == null)
                {
                    return NotFound($"Test with ID {testId} not found for candidate {candidateId}");
                }

                return Ok(test);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while starting test");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting test {TestId} for candidate {CandidateId}", testId, candidateId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Submit test answers
        /// </summary>
        /// <param name="testId">Test ID</param>
        /// <param name="submitTestDto">Test submission data</param>
        /// <returns>Submitted test with score</returns>
        [HttpPost("{testId:guid}/submit")]
        public async Task<ActionResult<BaseTestDto>> SubmitTest(Guid testId, [FromBody] SubmitTestDto submitTestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (testId != submitTestDto.TestId)
                {
                    return BadRequest("Test ID in URL does not match Test ID in request body");
                }

                // For simplicity, we'll extract candidateId from the test submission
                // In a real scenario, this would come from JWT claims
                var candidateId = submitTestDto.CandidateId;

                var test = await _testService.SubmitTestAsync(submitTestDto, candidateId);
                if (test == null)
                {
                    return NotFound($"Test with ID {testId} not found for candidate {candidateId}");
                }

                return Ok(test);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while submitting test");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting test {TestId}", testId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get random questions for a test type
        /// </summary>
        /// <param name="testType">Test type (Math, Portuguese, Psychology)</param>
        /// <param name="questionCount">Number of questions to retrieve</param>
        /// <returns>List of random questions</returns>
        [HttpGet("questions/{testType}")]
        public async Task<ActionResult<IEnumerable<QuestionDto>>> GetRandomQuestions(
            string testType, 
            [FromQuery] int questionCount = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(testType))
                {
                    return BadRequest("Test type is required");
                }

                if (questionCount < 1 || questionCount > 50)
                {
                    return BadRequest("Question count must be between 1 and 50");
                }

                var questions = await _testService.GetRandomQuestionsAsync(testType, questionCount);
                return Ok(questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving questions for test type {TestType}", testType);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get test status for a candidate
        /// </summary>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>Test completion status for each test type</returns>
        [HttpGet("candidate/{candidateId:guid}/status")]
        public async Task<ActionResult<Dictionary<string, bool>>> GetTestStatus(Guid candidateId)
        {
            try
            {
                var status = await _testService.GetTestStatusAsync(candidateId);
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving test status for candidate {CandidateId}", candidateId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Check if candidate can start a specific test type
        /// </summary>
        /// <param name="candidateId">Candidate ID</param>
        /// <param name="testType">Test type</param>
        /// <returns>Boolean indicating if test can be started</returns>
        [HttpGet("candidate/{candidateId:guid}/can-start/{testType}")]
        public async Task<ActionResult<bool>> CanStartTest(Guid candidateId, string testType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(testType))
                {
                    return BadRequest("Test type is required");
                }

                var canStart = await _testService.CanStartTestAsync(candidateId, testType);
                return Ok(canStart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if candidate {CandidateId} can start test {TestType}", candidateId, testType);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Check if a test has timed out
        /// </summary>
        /// <param name="testId">Test ID</param>
        /// <returns>Boolean indicating if test has timed out</returns>
        [HttpGet("{testId:guid}/timeout")]
        public async Task<ActionResult<bool>> IsTestTimedOut(Guid testId)
        {
            try
            {
                var isTimedOut = await _testService.IsTestTimedOutAsync(testId);
                return Ok(isTimedOut);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking timeout for test {TestId}", testId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get remaining time for a test in minutes
        /// </summary>
        /// <param name="testId">Test ID</param>
        /// <returns>Remaining time in minutes, null if test not found or not in progress</returns>
        [HttpGet("{testId:guid}/remaining-time")]
        public async Task<ActionResult<object>> GetRemainingTime(Guid testId)
        {
            try
            {
                var remainingTime = await _testService.GetRemainingTimeAsync(testId);
                if (remainingTime == null)
                {
                    return NotFound($"Test with ID {testId} not found or not in progress");
                }

                return Ok(new { remainingMinutes = remainingTime.Value });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting remaining time for test {TestId}", testId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Upload video answer for Math test question
        /// </summary>
        /// <param name="testId">Math test ID</param>
        /// <param name="questionNumber">Question number (1 or 2)</param>
        /// <param name="candidateId">Candidate ID</param>
        /// <param name="videoFile">Video file</param>
        /// <returns>Upload result</returns>
        [HttpPost("{testId:guid}/math/question/{questionNumber:int}/video")]
        public async Task<ActionResult<object>> UploadMathQuestionVideo(
            Guid testId,
            int questionNumber,
            [FromQuery] Guid candidateId,
            IFormFile videoFile)
        {
            try
            {
                if (videoFile == null)
                {
                    return BadRequest("Video file is required");
                }

                if (questionNumber < 1 || questionNumber > 2)
                {
                    return BadRequest("Question number must be 1 or 2");
                }

                var result = await _mediaService.UploadMathQuestionVideoAsync(testId, questionNumber, candidateId, videoFile);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for Math test video upload");
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access for Math test video upload");
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for Math test video upload");
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading Math test video for test {TestId}, question {QuestionNumber}, candidate {CandidateId}",
                    testId, questionNumber, candidateId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}