using Dignus.Candidate.Back.DTOs;

namespace Dignus.Candidate.Back.Services.Interfaces
{
    /// <summary>
    /// Service interface for evaluation engine operations
    /// </summary>
    public interface IEvaluationEngineService
    {
        /// <summary>
        /// Performs comprehensive evaluation of a candidate
        /// </summary>
        /// <param name="candidateId">Candidate identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Comprehensive candidate evaluation</returns>
        Task<CandidateEvaluationDto> EvaluateCandidateAsync(Guid candidateId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Evaluates individual test results
        /// </summary>
        /// <param name="testId">Test identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Test evaluation result</returns>
        Task<TestEvaluationDto> EvaluateTestAsync(Guid testId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Calculates competency-based evaluations
        /// </summary>
        /// <param name="candidateId">Candidate identifier</param>
        /// <param name="competencies">List of competencies to evaluate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of competency evaluations</returns>
        Task<List<CompetencyEvaluationDto>> EvaluateCompetenciesAsync(Guid candidateId, List<string> competencies, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs behavioral evaluation from video analysis
        /// </summary>
        /// <param name="videoInterviewId">Video interview identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Behavioral evaluation result</returns>
        Task<BehavioralEvaluationDto> EvaluateBehavioralAsync(Guid videoInterviewId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Calculates overall candidate score based on multiple factors
        /// </summary>
        /// <param name="candidateId">Candidate identifier</param>
        /// <param name="weights">Scoring weights for different components</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Overall score and breakdown</returns>
        Task<decimal> CalculateOverallScoreAsync(Guid candidateId, Dictionary<string, decimal>? weights = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Re-evaluates candidate after new test completion
        /// </summary>
        /// <param name="candidateId">Candidate identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated evaluation</returns>
        Task<CandidateEvaluationDto> RefreshEvaluationAsync(Guid candidateId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets evaluation status for a candidate
        /// </summary>
        /// <param name="candidateId">Candidate identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Evaluation status information</returns>
        Task<EvaluationStatus> GetEvaluationStatusAsync(Guid candidateId, CancellationToken cancellationToken = default);
    }
}