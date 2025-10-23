using AutoMapper;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;
using Dignus.Candidate.Back.Exceptions;
using Dignus.Data.Models;
using Dignus.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dignus.Candidate.Back.Services
{
    /// <summary>
    /// Service for job management operations
    /// </summary>
    public class JobService : IJobService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<JobService> _logger;

        public JobService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<JobService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<JobListingDto>> SearchJobsAsync(JobSearchRequest request)
        {
            try
            {
                _logger.LogInformation("Searching jobs with criteria: {SearchQuery}, Company: {Company}, Status: {Status}",
                    request.SearchQuery, request.Company, request.Status);

                // Get all jobs first, then apply filters in memory
                // Note: This is a simplified approach - for large datasets, consider implementing custom repository methods
                var allJobs = await _unitOfWork.Jobs.GetAllAsync();
                var filteredJobs = allJobs.AsEnumerable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(request.SearchQuery))
                {
                    var searchLower = request.SearchQuery.ToLower();
                    filteredJobs = filteredJobs.Where(j => j.Name.ToLower().Contains(searchLower) ||
                                                         (j.Description?.ToLower().Contains(searchLower) ?? false) ||
                                                         (j.Company?.ToLower().Contains(searchLower) ?? false));
                }

                // Apply company filter
                if (!string.IsNullOrWhiteSpace(request.Company))
                {
                    filteredJobs = filteredJobs.Where(j => j.Company == request.Company);
                }

                // Apply status filter
                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    filteredJobs = filteredJobs.Where(j => j.Status == request.Status);
                }

                // Apply location filter
                if (!string.IsNullOrWhiteSpace(request.Location))
                {
                    filteredJobs = filteredJobs.Where(j => j.Location == request.Location);
                }

                // Apply sorting
                filteredJobs = request.SortBy.ToLower() switch
                {
                    "title" => request.SortDirection.ToLower() == "asc"
                        ? filteredJobs.OrderBy(j => j.Name)
                        : filteredJobs.OrderByDescending(j => j.Name),
                    _ => request.SortDirection.ToLower() == "asc"
                        ? filteredJobs.OrderBy(j => j.PublishedAt)
                        : filteredJobs.OrderByDescending(j => j.PublishedAt)
                };

                var totalCount = filteredJobs.Count();
                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                // Apply pagination
                var pagedJobs = filteredJobs
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                // Map to DTOs
                var jobDtos = _mapper.Map<List<JobListingDto>>(pagedJobs);

                return new PagedResult<JobListingDto>
                {
                    Data = jobDtos,
                    Pagination = new PaginationInfo
                    {
                        Page = request.Page,
                        PageSize = request.PageSize,
                        TotalPages = totalPages,
                        TotalCount = totalCount,
                        HasNextPage = request.Page < totalPages,
                        HasPreviousPage = request.Page > 1
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching jobs");
                throw;
            }
        }

        public async Task<JobListingDto?> GetJobByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("Getting job by ID: {JobId}", id);

                if (!Guid.TryParse(id, out var jobGuid))
                {
                    _logger.LogWarning("Invalid job ID format: {JobId}", id);
                    return null;
                }

                var job = await _unitOfWork.Jobs.GetByIdAsync(jobGuid);

                if (job == null)
                {
                    _logger.LogWarning("Job not found: {JobId}", id);
                    return null;
                }

                return _mapper.Map<JobListingDto>(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job by ID: {JobId}", id);
                throw;
            }
        }

        public async Task<JobStatisticsDto> GetJobStatisticsAsync()
        {
            try
            {
                _logger.LogInformation("Getting job statistics");

                var totalOpenJobs = await _unitOfWork.Jobs.CountAsync(j => j.Status == "Publicado");

                // For now, mock application statistics since JobApplications table integration is pending
                var totalApplicationsReceived = 5962;

                var jobsAboutToExpire = await _unitOfWork.Jobs.CountAsync(j =>
                    j.Status == "Publicado" &&
                    j.ExpiresAt.HasValue &&
                    j.ExpiresAt.Value <= DateTime.UtcNow.AddDays(7));

                var frozenJobs = await _unitOfWork.Jobs.CountAsync(j => j.Status == "Congelado" || j.Status == "Pausado");

                return new JobStatisticsDto
                {
                    TotalOpenJobs = totalOpenJobs,
                    TotalApplicationsReceived = totalApplicationsReceived,
                    JobsAboutToExpire = jobsAboutToExpire,
                    FrozenJobs = frozenJobs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job statistics");
                throw;
            }
        }

        public async Task<bool> ApplyToJobAsync(string jobId, ApplyToJobDto request)
        {
            try
            {
                _logger.LogInformation("Candidate {CandidateId} applying to job {JobId}", request.CandidateId, jobId);

                if (!Guid.TryParse(jobId, out var jobGuid))
                {
                    _logger.LogWarning("Invalid job ID format: {JobId}", jobId);
                    throw new JobApplicationException(
                        JobApplicationException.ErrorCodes.InvalidJobId,
                        "ID da vaga inválido",
                        $"Invalid job ID format: {jobId}");
                }

                // Check if job exists and is open for applications
                var job = await _unitOfWork.Jobs.GetByIdAsync(jobGuid);
                if (job == null)
                {
                    _logger.LogWarning("Job not found: {JobId}", jobId);
                    throw new JobApplicationException(
                        JobApplicationException.ErrorCodes.JobNotFound,
                        "Vaga não encontrada",
                        $"Job not found: {jobId}");
                }

                if (job.Status != "Publicado")
                {
                    _logger.LogWarning("Job {JobId} is not open for applications. Status: {Status}", jobId, job.Status);
                    throw new JobApplicationException(
                        JobApplicationException.ErrorCodes.JobNotOpen,
                        "Esta vaga não está mais aceitando candidaturas",
                        $"Job {jobId} is not open for applications. Status: {job.Status}");
                }

                if (job.ExpiresAt.HasValue && job.ExpiresAt.Value < DateTime.UtcNow)
                {
                    _logger.LogWarning("Job {JobId} has expired", jobId);
                    throw new JobApplicationException(
                        JobApplicationException.ErrorCodes.JobExpired,
                        "O prazo para candidatura a esta vaga já expirou",
                        $"Job {jobId} has expired on {job.ExpiresAt.Value}");
                }

                // TODO: Implement JobApplication repository to track applications
                // For now, just log the application attempt and return success
                _logger.LogInformation("Job application would be created for candidate {CandidateId} to job {JobId}",
                    request.CandidateId, jobId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying to job {JobId} for candidate {CandidateId}", jobId, request.CandidateId);
                throw;
            }
        }

        public async Task<PagedResult<JobListingDto>> GetJobCandidatesAsync(string jobId, int page = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Getting candidates for job {JobId}", jobId);
                
                // TODO: Implement actual candidate retrieval
                return new PagedResult<JobListingDto>
                {
                    Data = new List<JobListingDto>(),
                    Pagination = new PaginationInfo
                    {
                        Page = page,
                        PageSize = pageSize,
                        TotalPages = 0,
                        TotalCount = 0,
                        HasNextPage = false,
                        HasPreviousPage = false
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting candidates for job {JobId}", jobId);
                throw;
            }
        }


    }
}