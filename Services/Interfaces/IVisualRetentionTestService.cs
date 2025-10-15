using Dignus.Candidate.Back.DTOs;

namespace Dignus.Candidate.Back.Services.Interfaces
{
    /// <summary>
    /// Service interface for Visual Retention Test operations with fixed questions
    /// </summary>
    public interface IVisualRetentionTestService
    {
        /// <summary>
        /// Get or create visual retention test for candidate with all fixed questions
        /// </summary>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>Visual retention test with all fixed questions</returns>
        Task<VisualRetentionTestDto?> GetOrCreateVisualRetentionTestAsync(Guid candidateId);

        /// <summary>
        /// Get visual retention test by ID
        /// </summary>
        /// <param name="testId">Test ID</param>
        /// <param name="candidateId">Candidate ID for security validation</param>
        /// <returns>Visual retention test if found and owned by candidate</returns>
        Task<VisualRetentionTestDto?> GetVisualRetentionTestByIdAsync(Guid testId, Guid candidateId);

        /// <summary>
        /// Start visual retention test (mark as in progress)
        /// </summary>
        /// <param name="testId">Test ID</param>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>Started test</returns>
        Task<VisualRetentionTestDto?> StartVisualRetentionTestAsync(Guid testId, Guid candidateId);

        /// <summary>
        /// Submit visual retention test responses (A, B, C, D, E format)
        /// </summary>
        /// <param name="submission">Test submission with fixed responses</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> SubmitTestResponsesAsync(VisualRetentionSubmissionDto submission);

        /// <summary>
        /// Get visual retention test status for candidate
        /// </summary>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>Test status information</returns>
        Task<object> GetTestStatusAsync(Guid candidateId);

        /// <summary>
        /// Check if candidate can start visual retention test
        /// </summary>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>True if test can be started, false otherwise</returns>
        Task<bool> CanStartTestAsync(Guid candidateId);
    }
}