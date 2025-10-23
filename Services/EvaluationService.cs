using AutoMapper;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;
using Dignus.Data.Models;
using Dignus.Data.Repositories.Interfaces;

namespace Dignus.Candidate.Back.Services
{
    /// <summary>
    /// Service implementation for evaluation management operations
    /// </summary>
    public class EvaluationService : IEvaluationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<EvaluationService> _logger;
        private readonly ITestService _testService;

        public EvaluationService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<EvaluationService> logger,
            ITestService testService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _testService = testService;
        }

        public async Task<EvaluationDto?> GetEvaluationByCandidateIdAsync(Guid candidateId)
        {
            try
            {
                var evaluations = await _unitOfWork.Evaluations.GetAllAsync();
                var evaluation = evaluations.FirstOrDefault(e => e.Candidate.Id == candidateId);
                
                return evaluation != null ? _mapper.Map<EvaluationDto>(evaluation) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving evaluation for candidate {CandidateId}", candidateId);
                throw;
            }
        }

        public async Task<EvaluationDto?> GetEvaluationByIdAsync(Guid evaluationId)
        {
            try
            {
                var evaluation = await _unitOfWork.Evaluations.GetByIdAsync(evaluationId);
                return evaluation != null ? _mapper.Map<EvaluationDto>(evaluation) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving evaluation {EvaluationId}", evaluationId);
                throw;
            }
        }

        public async Task<EvaluationDto> CreateEvaluationAsync(CreateEvaluationDto createEvaluationDto)
        {
            try
            {
                // Validate candidate exists
                var candidate = await _unitOfWork.Candidates.GetByIdAsync(createEvaluationDto.CandidateId);
                if (candidate == null)
                {
                    throw new InvalidOperationException($"Candidate with ID {createEvaluationDto.CandidateId} not found.");
                }

                // Check if evaluation already exists for this candidate
                var existingEvaluation = await GetEvaluationByCandidateIdAsync(createEvaluationDto.CandidateId);
                if (existingEvaluation != null)
                {
                    throw new InvalidOperationException($"Evaluation already exists for candidate {createEvaluationDto.CandidateId}.");
                }

                var evaluation = _mapper.Map<Evaluation>(createEvaluationDto);
                evaluation.Id = Guid.NewGuid();
                evaluation.Candidate = candidate;
                evaluation.EvaluatedAt = DateTimeOffset.UtcNow;

                // Note: Recruiter relationship not available in current model

                await _unitOfWork.Evaluations.AddAsync(evaluation);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created evaluation {EvaluationId} for candidate {CandidateId}", 
                    evaluation.Id, createEvaluationDto.CandidateId);

                return _mapper.Map<EvaluationDto>(evaluation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating evaluation for candidate {CandidateId}", createEvaluationDto.CandidateId);
                throw;
            }
        }

        public async Task<EvaluationDto?> UpdateEvaluationAsync(Guid evaluationId, UpdateEvaluationDto updateEvaluationDto)
        {
            try
            {
                var evaluation = await _unitOfWork.Evaluations.GetByIdAsync(evaluationId);
                if (evaluation == null)
                {
                    return null;
                }

                // Update only provided fields
                if (updateEvaluationDto.PortugueseTestFeedback.HasValue)
                    evaluation.PortugueseTestFeedback = updateEvaluationDto.PortugueseTestFeedback.Value;

                if (!string.IsNullOrEmpty(updateEvaluationDto.MathTestFeedback))
                    evaluation.MathTestFeedback = updateEvaluationDto.MathTestFeedback;

                if (!string.IsNullOrEmpty(updateEvaluationDto.VideoInterviewFeedback))
                    evaluation.VideoInterviewFeedback = updateEvaluationDto.VideoInterviewFeedback;

                if (!string.IsNullOrEmpty(updateEvaluationDto.PsychologyTestFeedback))
                    evaluation.PsychologyTestFeedback = updateEvaluationDto.PsychologyTestFeedback;

                _unitOfWork.Evaluations.Update(evaluation);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated evaluation {EvaluationId}", evaluationId);

                return _mapper.Map<EvaluationDto>(evaluation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating evaluation {EvaluationId}", evaluationId);
                throw;
            }
        }

        public async Task<EvaluationDto> GenerateAutomaticEvaluationAsync(Guid candidateId)
        {
            try
            {
                // Check if candidate is ready for evaluation
                if (!await IsReadyForEvaluationAsync(candidateId))
                {
                    throw new InvalidOperationException($"Candidate {candidateId} is not ready for evaluation. Complete all tests first.");
                }

                // Create basic evaluation placeholder
                var createEvaluationDto = new CreateEvaluationDto
                {
                    CandidateId = candidateId,
                    MathTestFeedback = "Generated by AI analysis",
                    PsychologyTestFeedback = "Generated by AI analysis",
                    VideoInterviewFeedback = "Generated by AI analysis"
                };

                var evaluation = await CreateEvaluationAsync(createEvaluationDto);

                _logger.LogInformation("Generated automatic evaluation for candidate {CandidateId}", candidateId);

                return evaluation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating automatic evaluation for candidate {CandidateId}", candidateId);
                throw;
            }
        }

        public async Task<bool> UpdateAiAnalysisAsync(Guid evaluationId, string aiAnalysis)
        {
            try
            {
                var evaluation = await _unitOfWork.Evaluations.GetByIdAsync(evaluationId);
                if (evaluation == null)
                {
                    return false;
                }

                // Update feedback fields with AI analysis
                evaluation.MathTestFeedback = aiAnalysis;
                _unitOfWork.Evaluations.Update(evaluation);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated AI analysis for evaluation {EvaluationId}", evaluationId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating AI analysis for evaluation {EvaluationId}", evaluationId);
                throw;
            }
        }

        public async Task<bool> AddRecruiterNotesAsync(Guid evaluationId, Guid recruiterId, string notes)
        {
            try
            {
                var evaluation = await _unitOfWork.Evaluations.GetByIdAsync(evaluationId);
                if (evaluation == null)
                {
                    return false;
                }

                // Store notes in feedback field
                evaluation.VideoInterviewFeedback = notes;
                _unitOfWork.Evaluations.Update(evaluation);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Added recruiter notes to evaluation {EvaluationId} by recruiter {RecruiterId}", 
                    evaluationId, recruiterId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding recruiter notes to evaluation {EvaluationId}", evaluationId);
                throw;
            }
        }

        public async Task<decimal> CalculateOverallScoreAsync(Guid candidateId)
        {
            try
            {
                var tests = await _testService.GetCandidateTestsAsync(candidateId);
                var completedTests = tests.Where(t => t.Status == Dignus.Data.Models.TestStatus.Submitted && t.Score.HasValue).ToList();

                if (!completedTests.Any())
                {
                    return 0;
                }

                // Simple average of all completed test scores
                var averageScore = completedTests.Average(t => t.Score!.Value);
                return Math.Round(averageScore, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating overall score for candidate {CandidateId}", candidateId);
                throw;
            }
        }

        public async Task<bool> IsReadyForEvaluationAsync(Guid candidateId)
        {
            try
            {
                // Get all tests for the candidate
                var tests = await _testService.GetCandidateTestsAsync(candidateId);

                // Check if all required test types are completed
                var hasMath = tests.Any(t => t.TestType == Dignus.Data.Models.Enums.TestType.Math &&
                                            t.Status == Dignus.Data.Models.TestStatus.Submitted);
                var hasPortuguese = tests.Any(t => t.TestType == Dignus.Data.Models.Enums.TestType.Portuguese &&
                                                  t.Status == Dignus.Data.Models.TestStatus.Submitted);
                var hasPsychology = tests.Any(t => t.TestType == Dignus.Data.Models.Enums.TestType.Psychology &&
                                                  t.Status == Dignus.Data.Models.TestStatus.Submitted);

                return hasMath && hasPortuguese && hasPsychology;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if candidate {CandidateId} is ready for evaluation", candidateId);
                throw;
            }
        }

        public async Task<(IEnumerable<EvaluationDto> Evaluations, int TotalCount)> GetEvaluationsAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            decimal? minScore = null, 
            decimal? maxScore = null)
        {
            try
            {
                var allEvaluations = await _unitOfWork.Evaluations.GetAllAsync();
                var totalCount = allEvaluations.Count();

                // Apply pagination
                var evaluations = allEvaluations
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var evaluationDtos = _mapper.Map<List<EvaluationDto>>(evaluations);

                return (evaluationDtos, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving evaluations page {PageNumber} with size {PageSize}", 
                    pageNumber, pageSize);
                throw;
            }
        }

        // Private helper methods - placeholder for future AI integration
    }
}