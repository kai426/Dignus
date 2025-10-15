using Dignus.Candidate.Back.DTOs;

namespace Dignus.Candidate.Back.Services.Interfaces
{
    /// <summary>
    /// Service interface for test management operations
    /// </summary>
    public interface ITestService
    {
        /// <summary>
        /// Creates a new test for a candidate
        /// </summary>
        /// <param name="createTestDto">The test creation data</param>
        /// <returns>The created test DTO</returns>
        Task<BaseTestDto> CreateTestAsync(CreateTestDto createTestDto);

        /// <summary>
        /// Starts a test for a candidate
        /// </summary>
        /// <param name="testId">The test identifier</param>
        /// <param name="candidateId">The candidate identifier</param>
        /// <returns>The started test DTO with questions</returns>
        Task<BaseTestDto?> StartTestAsync(Guid testId, Guid candidateId);

        /// <summary>
        /// Submits answers for a test
        /// </summary>
        /// <param name="submitTestDto">The test submission data</param>
        /// <param name="candidateId">The candidate identifier</param>
        /// <returns>The updated test DTO with scores</returns>
        Task<BaseTestDto?> SubmitTestAsync(SubmitTestDto submitTestDto, Guid candidateId);

        /// <summary>
        /// Retrieves a test by its identifier
        /// </summary>
        /// <param name="testId">The test identifier</param>
        /// <param name="candidateId">The candidate identifier for security validation</param>
        /// <returns>The test DTO if found and owned by candidate, null otherwise</returns>
        Task<BaseTestDto?> GetTestByIdAsync(Guid testId, Guid candidateId);

        /// <summary>
        /// Retrieves all tests for a candidate
        /// </summary>
        /// <param name="candidateId">The candidate identifier</param>
        /// <returns>A list of test DTOs</returns>
        Task<IEnumerable<BaseTestDto>> GetTestsByCandidateIdAsync(Guid candidateId);

        /// <summary>
        /// Retrieves random questions for a test type
        /// </summary>
        /// <param name="testType">The type of test (Math, Portuguese, Psychology)</param>
        /// <param name="questionCount">Number of questions to retrieve</param>
        /// <returns>A list of question DTOs</returns>
        Task<IEnumerable<QuestionDto>> GetRandomQuestionsAsync(string testType, int questionCount = 10);

        /// <summary>
        /// Calculates the score for a test based on answers
        /// </summary>
        /// <param name="testId">The test identifier</param>
        /// <returns>The calculated score</returns>
        Task<decimal> CalculateTestScoreAsync(Guid testId);

        /// <summary>
        /// Checks if a candidate can start a specific test type
        /// </summary>
        /// <param name="candidateId">The candidate identifier</param>
        /// <param name="testType">The type of test</param>
        /// <returns>True if candidate can start the test, false otherwise</returns>
        Task<bool> CanStartTestAsync(Guid candidateId, string testType);

        /// <summary>
        /// Gets the status of all tests for a candidate
        /// </summary>
        /// <param name="candidateId">The candidate identifier</param>
        /// <returns>Dictionary with test types and their completion status</returns>
        Task<Dictionary<string, bool>> GetTestStatusAsync(Guid candidateId);

        /// <summary>
        /// Gets available tests for a candidate, creating new ones if none exist
        /// </summary>
        /// <param name="candidateId">The candidate identifier</param>
        /// <returns>A list of available test DTOs</returns>
        Task<IEnumerable<BaseTestDto>> GetOrCreateTestsForCandidateAsync(Guid candidateId);

        /// <summary>
        /// Gets or creates a specific test type for a candidate
        /// </summary>
        /// <param name="candidateId">The candidate identifier</param>
        /// <param name="testType">The test type (portuguese, math, psychology, visualretention)</param>
        /// <returns>The test DTO for the specified type</returns>
        Task<BaseTestDto?> GetOrCreateTestAsync(Guid candidateId, string testType);

        /// <summary>
        /// Checks if a test has timed out based on StartedAt and configured timeout
        /// </summary>
        /// <param name="testId">The test identifier</param>
        /// <returns>True if the test has timed out, false otherwise</returns>
        Task<bool> IsTestTimedOutAsync(Guid testId);

        /// <summary>
        /// Gets the remaining time for a test in minutes
        /// </summary>
        /// <param name="testId">The test identifier</param>
        /// <returns>Remaining time in minutes, 0 if timed out, null if test not found</returns>
        Task<int?> GetRemainingTimeAsync(Guid testId);
    }
}