using Dignus.Candidate.Back.DTOs;

namespace Dignus.Candidate.Back.Services.Interfaces
{
    /// <summary>
    /// Service interface for evaluation management operations
    /// </summary>
    public interface IEvaluationService
    {
        /// <summary>
        /// Retrieves an evaluation by candidate identifier
        /// </summary>
        /// <param name="candidateId">The candidate identifier</param>
        /// <returns>The evaluation DTO if found, null otherwise</returns>
        Task<EvaluationDto?> GetEvaluationByCandidateIdAsync(Guid candidateId);

        /// <summary>
        /// Retrieves an evaluation by its identifier
        /// </summary>
        /// <param name="evaluationId">The evaluation identifier</param>
        /// <returns>The evaluation DTO if found, null otherwise</returns>
        Task<EvaluationDto?> GetEvaluationByIdAsync(Guid evaluationId);

        /// <summary>
        /// Creates a new evaluation for a candidate
        /// </summary>
        /// <param name="createEvaluationDto">The evaluation creation data</param>
        /// <returns>The created evaluation DTO</returns>
        Task<EvaluationDto> CreateEvaluationAsync(CreateEvaluationDto createEvaluationDto);

        /// <summary>
        /// Updates an existing evaluation
        /// </summary>
        /// <param name="evaluationId">The evaluation identifier</param>
        /// <param name="updateEvaluationDto">The evaluation update data</param>
        /// <returns>The updated evaluation DTO if successful, null if not found</returns>
        Task<EvaluationDto?> UpdateEvaluationAsync(Guid evaluationId, UpdateEvaluationDto updateEvaluationDto);

        /// <summary>
        /// Automatically generates an evaluation based on completed tests
        /// </summary>
        /// <param name="candidateId">The candidate identifier</param>
        /// <returns>The generated evaluation DTO</returns>
        Task<EvaluationDto> GenerateAutomaticEvaluationAsync(Guid candidateId);

        /// <summary>
        /// Updates AI analysis for an evaluation
        /// </summary>
        /// <param name="evaluationId">The evaluation identifier</param>
        /// <param name="aiAnalysis">The AI analysis text</param>
        /// <returns>True if successful, false if evaluation not found</returns>
        Task<bool> UpdateAiAnalysisAsync(Guid evaluationId, string aiAnalysis);

        /// <summary>
        /// Adds recruiter notes to an evaluation
        /// </summary>
        /// <param name="evaluationId">The evaluation identifier</param>
        /// <param name="recruiterId">The recruiter identifier</param>
        /// <param name="notes">The recruiter notes</param>
        /// <returns>True if successful, false if evaluation not found</returns>
        Task<bool> AddRecruiterNotesAsync(Guid evaluationId, Guid recruiterId, string notes);

        /// <summary>
        /// Calculates overall score from individual test scores
        /// </summary>
        /// <param name="candidateId">The candidate identifier</param>
        /// <returns>The calculated overall score</returns>
        Task<decimal> CalculateOverallScoreAsync(Guid candidateId);

        /// <summary>
        /// Checks if a candidate is ready for evaluation (all tests completed)
        /// </summary>
        /// <param name="candidateId">The candidate identifier</param>
        /// <returns>True if ready for evaluation, false otherwise</returns>
        Task<bool> IsReadyForEvaluationAsync(Guid candidateId);

        /// <summary>
        /// Retrieves all evaluations with pagination and filtering
        /// </summary>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">The number of evaluations per page</param>
        /// <param name="minScore">Minimum overall score filter</param>
        /// <param name="maxScore">Maximum overall score filter</param>
        /// <returns>A list of evaluation DTOs and total count</returns>
        Task<(IEnumerable<EvaluationDto> Evaluations, int TotalCount)> GetEvaluationsAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            decimal? minScore = null, 
            decimal? maxScore = null);
    }
}