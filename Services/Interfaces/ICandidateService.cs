using Dignus.Candidate.Back.DTOs;

namespace Dignus.Candidate.Back.Services.Interfaces
{
    /// <summary>
    /// Service interface for candidate management operations
    /// </summary>
    public interface ICandidateService
    {
        /// <summary>
        /// Retrieves a candidate by their unique identifier
        /// </summary>
        /// <param name="id">The candidate's unique identifier</param>
        /// <returns>The candidate DTO if found, null otherwise</returns>
        Task<CandidateDto?> GetCandidateByIdAsync(Guid id);

        /// <summary>
        /// Retrieves a candidate by their CPF
        /// </summary>
        /// <param name="cpf">The candidate's CPF</param>
        /// <returns>The candidate DTO if found, null otherwise</returns>
        Task<CandidateDto?> GetCandidateByCpfAsync(string cpf);

        /// <summary>
        /// Retrieves a candidate by their email address
        /// </summary>
        /// <param name="email">The candidate's email address</param>
        /// <returns>The candidate DTO if found, null otherwise</returns>
        Task<CandidateDto?> GetCandidateByEmailAsync(string email);

        /// <summary>
        /// Retrieves all candidates with pagination
        /// </summary>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">The number of candidates per page</param>
        /// <returns>A list of candidate DTOs</returns>
        Task<(IEnumerable<CandidateDto> Candidates, int TotalCount)> GetCandidatesAsync(int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Creates a new candidate
        /// </summary>
        /// <param name="createCandidateDto">The candidate creation data</param>
        /// <returns>The created candidate DTO</returns>
        Task<CandidateDto> CreateCandidateAsync(CreateCandidateDto createCandidateDto);

        /// <summary>
        /// Updates an existing candidate
        /// </summary>
        /// <param name="id">The candidate's unique identifier</param>
        /// <param name="updateCandidateDto">The candidate update data</param>
        /// <returns>The updated candidate DTO if successful, null if candidate not found</returns>
        Task<CandidateDto?> UpdateCandidateAsync(Guid id, UpdateCandidateDto updateCandidateDto);

        /// <summary>
        /// Updates a candidate's status
        /// </summary>
        /// <param name="id">The candidate's unique identifier</param>
        /// <param name="status">The new status</param>
        /// <returns>True if successful, false if candidate not found</returns>
        Task<bool> UpdateCandidateStatusAsync(Guid id, Data.Models.CandidateStatus status);

        /// <summary>
        /// Deletes a candidate
        /// </summary>
        /// <param name="id">The candidate's unique identifier</param>
        /// <returns>True if successful, false if candidate not found</returns>
        Task<bool> DeleteCandidateAsync(Guid id);

        /// <summary>
        /// Checks if a candidate exists by CPF
        /// </summary>
        /// <param name="cpf">The candidate's CPF</param>
        /// <returns>True if candidate exists, false otherwise</returns>
        Task<bool> CandidateExistsByCpfAsync(string cpf);

        /// <summary>
        /// Checks if a candidate exists by email
        /// </summary>
        /// <param name="email">The candidate's email</param>
        /// <returns>True if candidate exists, false otherwise</returns>
        Task<bool> CandidateExistsByEmailAsync(string email);

        /// <summary>
        /// Gets the progress of a candidate across all tests
        /// </summary>
        /// <param name="candidateId">The candidate's unique identifier</param>
        /// <returns>Progress DTO with completion percentage and test status</returns>
        Task<ProgressDto?> GetCandidateProgressAsync(Guid candidateId);

        /// <summary>
        /// Gets job information for a candidate
        /// </summary>
        /// <param name="candidateId">The candidate's unique identifier</param>
        /// <returns>Job information DTO if found, null otherwise</returns>
        Task<CandidateJobDto?> GetCandidateJobAsync(Guid candidateId);
    }
}