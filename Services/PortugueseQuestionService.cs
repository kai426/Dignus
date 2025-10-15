using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;
using Dignus.Data.Models;
using Dignus.Data.Repositories.Interfaces;
using System.Text.Json;

namespace Dignus.Candidate.Back.Services
{
    /// <summary>
    /// Service for Portuguese question management operations
    /// </summary>
    public class PortugueseQuestionService : IPortugueseQuestionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PortugueseQuestionService> _logger;

        public PortugueseQuestionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<PortugueseQuestionService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get Portuguese questions with pagination and optional search
        /// </summary>
        public async Task<(IEnumerable<QuestionDto> questions, int totalCount)> GetQuestionsAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                _logger.LogInformation("Getting Portuguese questions - Page: {PageNumber}, Size: {PageSize}, Search: {SearchTerm}",
                    pageNumber, pageSize, searchTerm);

                var allQuestions = await _unitOfWork.Questions.GetAllAsync();
                var portugueseQuestions = allQuestions.Where(q => q.Type == "portuguese").AsEnumerable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    portugueseQuestions = portugueseQuestions.Where(q => q.Text.ToLower().Contains(searchLower));
                }

                var totalCount = portugueseQuestions.Count();

                var questions = portugueseQuestions
                    .OrderBy(q => q.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var questionDtos = _mapper.Map<IEnumerable<QuestionDto>>(questions);
                return (questionDtos, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Portuguese questions");
                throw;
            }
        }

        /// <summary>
        /// Get Portuguese question by ID
        /// </summary>
        public async Task<QuestionDto?> GetQuestionByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("Getting Portuguese question by ID: {QuestionId}", id);

                var question = await _unitOfWork.Questions.GetByIdAsync(id);
                if (question == null || question.Type != "portuguese")
                {
                    _logger.LogWarning("Portuguese question not found: {QuestionId}", id);
                    return null;
                }

                return _mapper.Map<QuestionDto>(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Portuguese question {QuestionId}", id);
                throw;
            }
        }

        /// <summary>
        /// Create new Portuguese question
        /// </summary>
        public async Task<QuestionDto> CreateQuestionAsync(CreatePortugueseQuestionDto createQuestionDto)
        {
            try
            {
                _logger.LogInformation("Creating Portuguese question");

                var options = new
                {
                    A = createQuestionDto.OptionA,
                    B = createQuestionDto.OptionB,
                    C = createQuestionDto.OptionC,
                    D = createQuestionDto.OptionD
                };

                var question = new Question
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = createQuestionDto.QuestionText,
                    Type = "portuguese",
                    OptionsJson = JsonSerializer.Serialize(options),
                    CorrectAnswer = createQuestionDto.CorrectAnswer,
                    Order = 0, // Will be set appropriately during test generation
                    AllowMultipleAnswers = false,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await _unitOfWork.Questions.AddAsync(question);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Portuguese question created with ID: {QuestionId}", question.Id);
                return _mapper.Map<QuestionDto>(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Portuguese question");
                throw;
            }
        }

        /// <summary>
        /// Update Portuguese question
        /// </summary>
        public async Task<QuestionDto?> UpdateQuestionAsync(string id, UpdatePortugueseQuestionDto updateQuestionDto)
        {
            try
            {
                _logger.LogInformation("Updating Portuguese question: {QuestionId}", id);

                var question = await _unitOfWork.Questions.GetByIdAsync(id);
                if (question == null || question.Type != "portuguese")
                {
                    _logger.LogWarning("Portuguese question not found for update: {QuestionId}", id);
                    return null;
                }

                // Update question text if provided
                if (!string.IsNullOrWhiteSpace(updateQuestionDto.QuestionText))
                    question.Text = updateQuestionDto.QuestionText;

                // Update options if provided
                if (updateQuestionDto.OptionA != null || updateQuestionDto.OptionB != null ||
                    updateQuestionDto.OptionC != null || updateQuestionDto.OptionD != null)
                {
                    // Parse existing options first
                    var existingOptions = new { A = "", B = "", C = "", D = "" };
                    if (!string.IsNullOrWhiteSpace(question.OptionsJson))
                    {
                        try
                        {
                            existingOptions = JsonSerializer.Deserialize<dynamic>(question.OptionsJson);
                        }
                        catch
                        {
                            // Keep default empty options if deserialization fails
                        }
                    }

                    var updatedOptions = new
                    {
                        A = updateQuestionDto.OptionA ?? existingOptions.A,
                        B = updateQuestionDto.OptionB ?? existingOptions.B,
                        C = updateQuestionDto.OptionC ?? existingOptions.C,
                        D = updateQuestionDto.OptionD ?? existingOptions.D
                    };

                    question.OptionsJson = JsonSerializer.Serialize(updatedOptions);
                }

                // Update correct answer if provided
                if (!string.IsNullOrWhiteSpace(updateQuestionDto.CorrectAnswer))
                    question.CorrectAnswer = updateQuestionDto.CorrectAnswer;

                _unitOfWork.Questions.Update(question);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Portuguese question updated: {QuestionId}", id);
                return _mapper.Map<QuestionDto>(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Portuguese question {QuestionId}", id);
                throw;
            }
        }

        /// <summary>
        /// Delete Portuguese question
        /// </summary>
        public async Task<bool> DeleteQuestionAsync(string id)
        {
            try
            {
                _logger.LogInformation("Deleting Portuguese question: {QuestionId}", id);

                var question = await _unitOfWork.Questions.GetByIdAsync(id);
                if (question == null || question.Type != "portuguese")
                {
                    _logger.LogWarning("Portuguese question not found for deletion: {QuestionId}", id);
                    return false;
                }

                _unitOfWork.Questions.Remove(question);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Portuguese question deleted: {QuestionId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Portuguese question {QuestionId}", id);
                throw;
            }
        }

        /// <summary>
        /// Bulk import Portuguese questions
        /// </summary>
        public async Task<object> BulkImportQuestionsAsync(List<CreatePortugueseQuestionDto> questions)
        {
            try
            {
                _logger.LogInformation("Starting bulk import of {Count} Portuguese questions", questions.Count);

                var importedQuestions = new List<Question>();
                var errors = new List<string>();

                foreach (var (questionDto, index) in questions.Select((q, i) => (q, i)))
                {
                    try
                    {
                        var options = new
                        {
                            A = questionDto.OptionA,
                            B = questionDto.OptionB,
                            C = questionDto.OptionC,
                            D = questionDto.OptionD
                        };

                        var question = new Question
                        {
                            Id = Guid.NewGuid().ToString(),
                            Text = questionDto.QuestionText,
                            Type = "portuguese",
                            OptionsJson = JsonSerializer.Serialize(options),
                            CorrectAnswer = questionDto.CorrectAnswer,
                            Order = 0, // Will be set appropriately during test generation
                            AllowMultipleAnswers = false,
                            CreatedAt = DateTimeOffset.UtcNow
                        };

                        await _unitOfWork.Questions.AddAsync(question);
                        importedQuestions.Add(question);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Question {index + 1}: {ex.Message}");
                        _logger.LogWarning(ex, "Error importing question at index {Index}", index);
                    }
                }

                if (importedQuestions.Any())
                {
                    await _unitOfWork.SaveChangesAsync();
                }

                var result = new
                {
                    TotalProvided = questions.Count,
                    ImportedCount = importedQuestions.Count,
                    ErrorCount = errors.Count,
                    Errors = errors,
                    ImportedIds = importedQuestions.Select(q => q.Id).ToList()
                };

                _logger.LogInformation("Bulk import completed - Imported: {ImportedCount}, Errors: {ErrorCount}",
                    importedQuestions.Count, errors.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk import of Portuguese questions");
                throw;
            }
        }
    }
}