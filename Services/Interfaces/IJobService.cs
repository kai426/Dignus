using Dignus.Candidate.Back.DTOs;

namespace Dignus.Candidate.Back.Services.Interfaces
{
    /// <summary>
    /// Service interface for job management operations
    /// </summary>
    public interface IJobService
    {
        /// <summary>
        /// Search and filter jobs with pagination
        /// </summary>
        Task<PagedResult<JobListingDto>> SearchJobsAsync(JobSearchRequest request);

        /// <summary>
        /// Get job by ID
        /// </summary>
        Task<JobListingDto?> GetJobByIdAsync(string id);

        /// <summary>
        /// Get job statistics for dashboard
        /// </summary>
        Task<JobStatisticsDto> GetJobStatisticsAsync();

        /// <summary>
        /// Apply to a job
        /// </summary>
        Task<bool> ApplyToJobAsync(string jobId, ApplyToJobDto request);

        /// <summary>
        /// Get candidates who applied to a specific job
        /// </summary>
        Task<PagedResult<JobListingDto>> GetJobCandidatesAsync(string jobId, int page = 1, int pageSize = 10);
    }
}