using AutoMapper;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;
using Dignus.Data.Models;
using Dignus.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dignus.Candidate.Back.Services
{
    /// <summary>
    /// Service implementation for candidate management operations
    /// </summary>
    public class CandidateService : ICandidateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CandidateService> _logger;

        public CandidateService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CandidateService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CandidateDto?> GetCandidateByIdAsync(Guid id)
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByIdAsync(id);
                return candidate != null ? _mapper.Map<CandidateDto>(candidate) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving candidate with ID {CandidateId}", id);
                throw;
            }
        }

        public async Task<CandidateDto?> GetCandidateByCpfAsync(string cpf)
        {
            try
            {
                var candidates = await _unitOfWork.Candidates.GetAllAsync();
                var candidate = candidates.FirstOrDefault(c => c.Cpf == cpf);
                return candidate != null ? _mapper.Map<CandidateDto>(candidate) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving candidate with CPF {Cpf}", cpf);
                throw;
            }
        }

        public async Task<CandidateDto?> GetCandidateByEmailAsync(string email)
        {
            try
            {
                var candidates = await _unitOfWork.Candidates.GetAllAsync();
                var candidate = candidates.FirstOrDefault(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
                return candidate != null ? _mapper.Map<CandidateDto>(candidate) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving candidate with email {Email}", email);
                throw;
            }
        }

        public async Task<(IEnumerable<CandidateDto> Candidates, int TotalCount)> GetCandidatesAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var allCandidates = await _unitOfWork.Candidates.GetAllAsync();
                var totalCount = allCandidates.Count();
                
                var candidates = allCandidates
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var candidateDtos = _mapper.Map<List<CandidateDto>>(candidates);
                
                return (candidateDtos, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving candidates page {PageNumber} with size {PageSize}", pageNumber, pageSize);
                throw;
            }
        }

        public async Task<CandidateDto> CreateCandidateAsync(CreateCandidateDto createCandidateDto)
        {
            try
            {
                // Check if candidate already exists by CPF
                if (await CandidateExistsByCpfAsync(createCandidateDto.Cpf))
                {
                    throw new InvalidOperationException($"A candidate with CPF {createCandidateDto.Cpf} already exists.");
                }

                // Check if candidate already exists by email
                if (await CandidateExistsByEmailAsync(createCandidateDto.Email))
                {
                    throw new InvalidOperationException($"A candidate with email {createCandidateDto.Email} already exists.");
                }

                var candidate = _mapper.Map<Data.Models.Candidate>(createCandidateDto);
                candidate.Id = Guid.NewGuid();
                candidate.CreatedAt = DateTimeOffset.UtcNow;
                candidate.Status = CandidateStatus.InProcess;

                await _unitOfWork.Candidates.AddAsync(candidate);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created new candidate with ID {CandidateId}", candidate.Id);

                return _mapper.Map<CandidateDto>(candidate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating candidate with CPF {Cpf}", createCandidateDto.Cpf);
                throw;
            }
        }

        public async Task<CandidateDto?> UpdateCandidateAsync(Guid id, UpdateCandidateDto updateCandidateDto)
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByIdAsync(id);
                if (candidate == null)
                {
                    return null;
                }

                // Update only provided fields
                if (!string.IsNullOrEmpty(updateCandidateDto.Name))
                {
                    candidate.Name = updateCandidateDto.Name;
                }

                if (!string.IsNullOrEmpty(updateCandidateDto.Phone))
                {
                    candidate.Phone = updateCandidateDto.Phone;
                }

                if (updateCandidateDto.Status.HasValue)
                {
                    candidate.Status = updateCandidateDto.Status.Value;
                }

                if (updateCandidateDto.IsPCD.HasValue)
                {
                    candidate.IsPCD = updateCandidateDto.IsPCD.Value;
                }

                if (updateCandidateDto.IsForeigner.HasValue)
                {
                    candidate.IsForeigner = updateCandidateDto.IsForeigner.Value;
                }

                if (!string.IsNullOrEmpty(updateCandidateDto.CountryOfOrigin))
                {
                    candidate.CountryOfOrigin = updateCandidateDto.CountryOfOrigin;
                }
                else if (updateCandidateDto.IsForeigner == false)
                {
                    // Clear country of origin when marking as not foreigner
                    candidate.CountryOfOrigin = null;
                }

                _unitOfWork.Candidates.Update(candidate);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated candidate with ID {CandidateId}", id);

                return _mapper.Map<CandidateDto>(candidate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating candidate with ID {CandidateId}", id);
                throw;
            }
        }

        public async Task<bool> UpdateCandidateStatusAsync(Guid id, CandidateStatus status)
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByIdAsync(id);
                if (candidate == null)
                {
                    return false;
                }

                candidate.Status = status;
                _unitOfWork.Candidates.Update(candidate);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated candidate status to {Status} for candidate {CandidateId}", status, id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating candidate status for ID {CandidateId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCandidateAsync(Guid id)
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByIdAsync(id);
                if (candidate == null)
                {
                    return false;
                }

                _unitOfWork.Candidates.Remove(candidate);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted candidate with ID {CandidateId}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting candidate with ID {CandidateId}", id);
                throw;
            }
        }

        public async Task<bool> CandidateExistsByCpfAsync(string cpf)
        {
            try
            {
                var candidates = await _unitOfWork.Candidates.GetAllAsync();
                return candidates.Any(c => c.Cpf == cpf);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if candidate exists with CPF {Cpf}", cpf);
                throw;
            }
        }

        public async Task<bool> CandidateExistsByEmailAsync(string email)
        {
            try
            {
                var candidates = await _unitOfWork.Candidates.GetAllAsync();
                return candidates.Any(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if candidate exists with email {Email}", email);
                throw;
            }
        }

        public async Task<ProgressDto?> GetCandidateProgressAsync(Guid candidateId)
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
                if (candidate == null)
                {
                    return null;
                }

                // TODO: Update to use TestInstance from unified schema
                // Old test-specific repositories removed - returning mock progress for now
                var testProgress = new Dictionary<string, TestProgressDto>
                {
                    ["Portuguese"] = new TestProgressDto { TestType = "Portuguese", Status = "NotStarted", IsCompleted = false },
                    ["Math"] = new TestProgressDto { TestType = "Math", Status = "NotStarted", IsCompleted = false },
                    ["Psychology"] = new TestProgressDto { TestType = "Psychology", Status = "NotStarted", IsCompleted = false },
                    ["VisualRetention"] = new TestProgressDto { TestType = "VisualRetention", Status = "NotStarted", IsCompleted = false },
                    ["VideoInterview"] = new TestProgressDto { TestType = "VideoInterview", Status = "NotStarted", IsCompleted = false }
                };

                var completedTests = testProgress.Values.Count(tp => tp.IsCompleted);
                var totalTests = testProgress.Count;
                var completionPercentage = totalTests > 0 ? (decimal)completedTests / totalTests * 100 : 0;

                var progress = new ProgressDto
                {
                    CandidateId = candidateId,
                    CompletionPercentage = completionPercentage,
                    CompletedTests = completedTests,
                    TotalTests = totalTests,
                    TestProgress = testProgress
                };

                _logger.LogInformation("Retrieved progress for candidate {CandidateId}: {CompletionPercentage}% complete",
                    candidateId, completionPercentage);

                return progress;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving progress for candidate {CandidateId}", candidateId);
                throw;
            }
        }

        public async Task<CandidateJobDto?> GetCandidateJobAsync(Guid candidateId)
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
                if (candidate?.Job == null)
                {
                    return null;
                }

                var candidateJob = new CandidateJobDto
                {
                    CandidateId = candidateId,
                    AppliedAt = candidate.CreatedAt.DateTime,
                    Job = _mapper.Map<JobDto>(candidate.Job)
                };

                _logger.LogInformation("Retrieved job information for candidate {CandidateId}", candidateId);

                return candidateJob;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving job for candidate {CandidateId}", candidateId);
                throw;
            }
        }

        public async Task<CandidateDto?> UpdateCandidateDocumentAsync(Guid candidateId, string documentUrl, string fileName)
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
                if (candidate == null)
                {
                    return null;
                }

                candidate.PCDDocumentUrl = documentUrl;
                candidate.PCDDocumentFileName = fileName;
                candidate.PCDDocumentUploadedAt = DateTimeOffset.UtcNow;

                _unitOfWork.Candidates.Update(candidate);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated PCD document for candidate with ID {CandidateId}", candidateId);

                return _mapper.Map<CandidateDto>(candidate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating PCD document for candidate with ID {CandidateId}", candidateId);
                throw;
            }
        }
    }
}