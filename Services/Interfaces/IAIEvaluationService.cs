using Dignus.Candidate.Back.DTOs;

namespace Dignus.Candidate.Back.Services.Interfaces
{
    /// <summary>
    /// Service interface for comprehensive AI evaluation and scoring
    /// </summary>
    public interface IAIEvaluationService
    {
        /// <summary>
        /// Generates a comprehensive evaluation report for a candidate
        /// </summary>
        /// <param name="candidateId">The ID of the candidate</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Complete evaluation report</returns>
        Task<CandidateEvaluationDto> GenerateComprehensiveEvaluationAsync(Guid candidateId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Evaluates multiple choice test answers using AI
        /// </summary>
        /// <param name="testId">The ID of the test</param>
        /// <param name="answers">The submitted test answers</param>
        /// <param name="testType">The type of test (Math, Portuguese, Psychology)</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Test evaluation with AI-enhanced scoring</returns>
        Task<TestEvaluationDto> EvaluateTestAnswersAsync(Guid testId, IEnumerable<TestAnswerDto> answers, string testType, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Generates personalized feedback based on all evaluations
        /// </summary>
        /// <param name="candidateId">The ID of the candidate</param>
        /// <param name="evaluationResults">Combined evaluation results</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Personalized feedback and recommendations</returns>
        Task<PersonalizedFeedbackDto> GeneratePersonalizedFeedbackAsync(Guid candidateId, CandidateEvaluationDto evaluationResults, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Calculates an overall candidate score based on all assessments
        /// </summary>
        /// <param name="candidateId">The ID of the candidate</param>
        /// <param name="weights">Scoring weights for different assessment types</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Overall candidate scoring</returns>
        Task<OverallCandidateScoreDto> CalculateOverallScoreAsync(Guid candidateId, ScoringWeightsDto? weights = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Compares candidate performance against benchmarks
        /// </summary>
        /// <param name="candidateId">The ID of the candidate</param>
        /// <param name="jobRole">Target job role for comparison</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Benchmark comparison results</returns>
        Task<BenchmarkComparisonDto> CompareAgainstBenchmarksAsync(Guid candidateId, string? jobRole = null, CancellationToken cancellationToken = default);
    }
}