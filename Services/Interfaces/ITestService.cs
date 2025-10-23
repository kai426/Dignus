using Dignus.Candidate.Back.DTOs.Unified;
using Dignus.Data.Models.Enums;

namespace Dignus.Candidate.Back.Services.Interfaces;

/// <summary>
/// Unified test service interface for all test types
/// </summary>
public interface ITestService
{
    /// <summary>
    /// Creates a new test instance (generates questions via snapshotting)
    /// </summary>
    /// <param name="request">Test creation request with CandidateId and TestType</param>
    /// <returns>The created test with snapshotted questions</returns>
    Task<TestInstanceDto> CreateTestAsync(CreateTestInstanceRequest request);

    /// <summary>
    /// Gets a test by ID with IDOR protection (validates candidateId)
    /// </summary>
    /// <param name="testId">Test instance ID</param>
    /// <param name="candidateId">Candidate ID for security validation</param>
    /// <returns>The test DTO if found and owned by candidate</returns>
    Task<TestInstanceDto> GetTestByIdAsync(Guid testId, Guid candidateId);

    /// <summary>
    /// Gets all tests for a candidate, optionally filtered by test type
    /// </summary>
    /// <param name="candidateId">Candidate ID</param>
    /// <param name="filterByType">Optional test type filter</param>
    /// <returns>List of test instances</returns>
    Task<List<TestInstanceDto>> GetCandidateTestsAsync(Guid candidateId, TestType? filterByType = null);

    /// <summary>
    /// Starts a test (changes status to InProgress, sets StartedAt)
    /// </summary>
    /// <param name="testId">Test instance ID</param>
    /// <param name="candidateId">Candidate ID for security validation</param>
    /// <returns>The started test with questions (without correct answers)</returns>
    Task<TestInstanceDto> StartTestAsync(Guid testId, Guid candidateId);

    /// <summary>
    /// Submits test answers and performs auto-grading for objective questions
    /// </summary>
    /// <param name="request">Submission request with answers</param>
    /// <returns>Test submission result with score</returns>
    Task<TestSubmissionResultDto> SubmitTestAsync(SubmitTestRequest request);

    /// <summary>
    /// Gets questions for a test (without correct answers)
    /// </summary>
    /// <param name="testId">Test instance ID</param>
    /// <param name="candidateId">Candidate ID for security validation</param>
    /// <returns>List of question snapshots (safe for candidates)</returns>
    Task<List<QuestionSnapshotDto>> GetTestQuestionsAsync(Guid testId, Guid candidateId);

    /// <summary>
    /// Gets test status information (progress, can submit, remaining time)
    /// </summary>
    /// <param name="testId">Test instance ID</param>
    /// <param name="candidateId">Candidate ID for security validation</param>
    /// <returns>Test status DTO</returns>
    Task<TestStatusDto> GetTestStatusAsync(Guid testId, Guid candidateId);

    /// <summary>
    /// Checks if a candidate can start a test of a specific type
    /// (validates no active test of same type exists)
    /// </summary>
    /// <param name="candidateId">Candidate ID</param>
    /// <param name="testType">Test type to check</param>
    /// <returns>True if candidate can start the test</returns>
    Task<bool> CanStartTestAsync(Guid candidateId, TestType testType);

    /// <summary>
    /// Gets remaining time in seconds for a test (null if no time limit)
    /// </summary>
    /// <param name="testId">Test instance ID</param>
    /// <returns>Remaining seconds or null</returns>
    Task<int?> GetRemainingTimeAsync(Guid testId);
}