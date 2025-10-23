using AutoMapper;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;
using Dignus.Data.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dignus.Candidate.Back.Services
{
    /// <summary>
    /// Service for questionnaire operations - matches frontend 9-section structure
    /// </summary>
    public class QuestionnaireService : IQuestionnaireService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<QuestionnaireService> _logger;

        // Store progress in memory for now (in production, this would be in database)
        private readonly Dictionary<Guid, QuestionnaireProgressDto> _candidateProgress = new();

        public QuestionnaireService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<QuestionnaireService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<QuestionnaireDto> GetQuestionnaireAsync()
        {
            try
            {
                _logger.LogInformation("Getting questionnaire structure");

                // Return the questionnaire structure matching the frontend's 9 sections
                return new QuestionnaireDto
                {
                    Sections = GetQuestionnaireSections(),
                    CurrentSection = 0,
                    Responses = new Dictionary<string, object>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting questionnaire structure");
                throw;
            }
        }

        public async Task<QuestionnaireProgressDto> GetProgressAsync(Guid candidateId)
        {
            try
            {
                _logger.LogInformation("Getting questionnaire progress for candidate {CandidateId}", candidateId);

                if (_candidateProgress.TryGetValue(candidateId, out var progress))
                {
                    return progress;
                }

                // Return empty progress if not found
                return new QuestionnaireProgressDto
                {
                    CandidateId = candidateId,
                    TotalSections = 9,
                    CompletedSections = 0,
                    CurrentSection = 0,
                    ProgressPercentage = 0,
                    AllResponses = new Dictionary<string, object>(),
                    IsCompleted = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting questionnaire progress for candidate {CandidateId}", candidateId);
                throw;
            }
        }

        public async Task<bool> SaveSectionResponseAsync(Guid candidateId, int sectionId, Dictionary<string, object> responses)
        {
            try
            {
                _logger.LogInformation("Saving section {SectionId} responses for candidate {CandidateId}", sectionId, candidateId);

                // Get or create progress
                if (!_candidateProgress.TryGetValue(candidateId, out var progress))
                {
                    progress = await InitializeQuestionnaireAsync(candidateId);
                }

                // Update responses for this section
                foreach (var response in responses)
                {
                    progress.AllResponses[$"section_{sectionId}_{response.Key}"] = response.Value;
                }

                // Update progress
                var completedSections = Enumerable.Range(0, 9).Count(i => 
                    progress.AllResponses.Any(r => r.Key.StartsWith($"section_{i}_")));

                progress.CompletedSections = completedSections;
                progress.CurrentSection = Math.Min(sectionId + 1, 8); // Next section, max 8 (0-indexed)
                progress.ProgressPercentage = (int)((completedSections / 9.0) * 100);
                progress.LastSavedAt = DateTimeOffset.UtcNow;
                progress.IsCompleted = completedSections == 9;

                _candidateProgress[candidateId] = progress;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving section responses for candidate {CandidateId}", candidateId);
                throw;
            }
        }

        public async Task<bool> SubmitQuestionnaireAsync(SubmitQuestionnaireDto submission)
        {
            try
            {
                _logger.LogInformation("Submitting completed questionnaire for candidate {CandidateId}", submission.CandidateId);

                if (_candidateProgress.TryGetValue(submission.CandidateId, out var progress))
                {
                    progress.IsCompleted = true;
                    progress.CompletedSections = 9;
                    progress.ProgressPercentage = 100;
                    progress.AllResponses = submission.Responses;
                    _candidateProgress[submission.CandidateId] = progress;
                }

                // TODO: Save to database
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting questionnaire for candidate {CandidateId}", submission.CandidateId);
                throw;
            }
        }

        public async Task<QuestionnaireSectionDto?> GetSectionAsync(int sectionId)
        {
            try
            {
                _logger.LogInformation("Getting section {SectionId}", sectionId);

                var sections = GetQuestionnaireSections();
                return sections.FirstOrDefault(s => s.Order == sectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting section {SectionId}", sectionId);
                throw;
            }
        }

        public async Task<bool> CanStartQuestionnaireAsync(Guid candidateId)
        {
            try
            {
                _logger.LogInformation("Checking if candidate {CandidateId} can start questionnaire", candidateId);
                
                // TODO: Add business logic checks (e.g., other tests completed)
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if candidate {CandidateId} can start questionnaire", candidateId);
                throw;
            }
        }

        public async Task<QuestionnaireProgressDto> InitializeQuestionnaireAsync(Guid candidateId)
        {
            try
            {
                _logger.LogInformation("Initializing questionnaire for candidate {CandidateId}", candidateId);

                var progress = new QuestionnaireProgressDto
                {
                    CandidateId = candidateId,
                    TotalSections = 9,
                    CompletedSections = 0,
                    CurrentSection = 0,
                    ProgressPercentage = 0,
                    AllResponses = new Dictionary<string, object>(),
                    IsCompleted = false,
                    StartedAt = DateTimeOffset.UtcNow
                };

                _candidateProgress[candidateId] = progress;
                return progress;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing questionnaire for candidate {CandidateId}", candidateId);
                throw;
            }
        }

        private List<QuestionnaireSectionDto> GetQuestionnaireSections()
        {
            // Return the 9 sections matching the frontend structure
            return new List<QuestionnaireSectionDto>
            {
                new QuestionnaireSectionDto
                {
                    Id = "sec1",
                    Title = "Personalidade",
                    Order = 0,
                    Questions = new List<QuestionnaireQuestionDto>
                    {
                        new QuestionnaireQuestionDto
                        {
                            Id = "q1",
                            Prompt = "Como você se descreveria em termos de personalidade?",
                            Type = "multi",
                            MaxSelections = 3,
                            Options = new List<QuestionnaireOptionDto>
                            {
                                new QuestionnaireOptionDto { Id = "opt1", Label = "Extrovertido(a)" },
                                new QuestionnaireOptionDto { Id = "opt2", Label = "Introvertido(a)" },
                                new QuestionnaireOptionDto { Id = "opt3", Label = "Analítico(a)" },
                                new QuestionnaireOptionDto { Id = "opt4", Label = "Criativo(a)" },
                                new QuestionnaireOptionDto { Id = "opt5", Label = "Organizado(a)" },
                                new QuestionnaireOptionDto { Id = "opt6", Label = "Flexível" }
                            }
                        }
                    }
                },
                new QuestionnaireSectionDto
                {
                    Id = "sec2", 
                    Title = "Hábitos",
                    Order = 1,
                    Questions = new List<QuestionnaireQuestionDto>
                    {
                        new QuestionnaireQuestionDto
                        {
                            Id = "q2",
                            Prompt = "Quais são seus hábitos de trabalho?",
                            Type = "multi",
                            MaxSelections = 2,
                            Options = new List<QuestionnaireOptionDto>
                            {
                                new QuestionnaireOptionDto { Id = "opt1", Label = "Matutino" },
                                new QuestionnaireOptionDto { Id = "opt2", Label = "Vespertino" },
                                new QuestionnaireOptionDto { Id = "opt3", Label = "Noturno" },
                                new QuestionnaireOptionDto { Id = "opt4", Label = "Home office" },
                                new QuestionnaireOptionDto { Id = "opt5", Label = "Presencial" }
                            }
                        }
                    }
                },
                new QuestionnaireSectionDto
                {
                    Id = "sec3",
                    Title = "Experiência e Formação",
                    Order = 2,
                    Questions = new List<QuestionnaireQuestionDto>
                    {
                        new QuestionnaireQuestionDto
                        {
                            Id = "q3",
                            Prompt = "Qual sua área de formação principal?",
                            Type = "single",
                            Options = new List<QuestionnaireOptionDto>
                            {
                                new QuestionnaireOptionDto { Id = "opt1", Label = "Tecnologia" },
                                new QuestionnaireOptionDto { Id = "opt2", Label = "Administração" },
                                new QuestionnaireOptionDto { Id = "opt3", Label = "Engenharia" },
                                new QuestionnaireOptionDto { Id = "opt4", Label = "Humanas" },
                                new QuestionnaireOptionDto { Id = "opt5", Label = "Saúde" }
                            }
                        }
                    }
                },
                new QuestionnaireSectionDto
                {
                    Id = "sec4",
                    Title = "Motivação e Direcionamento",
                    Order = 3,
                    Questions = new List<QuestionnaireQuestionDto>
                    {
                        new QuestionnaireQuestionDto
                        {
                            Id = "q4",
                            Prompt = "O que mais te motiva no trabalho?",
                            Type = "multi",
                            MaxSelections = 3,
                            Options = new List<QuestionnaireOptionDto>
                            {
                                new QuestionnaireOptionDto { Id = "opt1", Label = "Desafios técnicos" },
                                new QuestionnaireOptionDto { Id = "opt2", Label = "Crescimento profissional" },
                                new QuestionnaireOptionDto { Id = "opt3", Label = "Trabalho em equipe" },
                                new QuestionnaireOptionDto { Id = "opt4", Label = "Recompensas financeiras" },
                                new QuestionnaireOptionDto { Id = "opt5", Label = "Impacto social" }
                            }
                        }
                    }
                },
                new QuestionnaireSectionDto
                {
                    Id = "sec5",
                    Title = "Autopercepção e Imagem Social",
                    Order = 4,
                    Questions = new List<QuestionnaireQuestionDto>
                    {
                        new QuestionnaireQuestionDto
                        {
                            Id = "q5",
                            Prompt = "Como você acredita que os colegas te veem?",
                            Type = "single",
                            Options = new List<QuestionnaireOptionDto>
                            {
                                new QuestionnaireOptionDto { Id = "opt1", Label = "Confiável" },
                                new QuestionnaireOptionDto { Id = "opt2", Label = "Inovador(a)" },
                                new QuestionnaireOptionDto { Id = "opt3", Label = "Colaborativo(a)" },
                                new QuestionnaireOptionDto { Id = "opt4", Label = "Líder natural" },
                                new QuestionnaireOptionDto { Id = "opt5", Label = "Especialista técnico" }
                            }
                        }
                    }
                },
                new QuestionnaireSectionDto
                {
                    Id = "sec6",
                    Title = "Emoções e Relações",
                    Order = 5,
                    Questions = new List<QuestionnaireQuestionDto>
                    {
                        new QuestionnaireQuestionDto
                        {
                            Id = "q6",
                            Prompt = "Como você lida com conflitos no trabalho?",
                            Type = "single",
                            Options = new List<QuestionnaireOptionDto>
                            {
                                new QuestionnaireOptionDto { Id = "opt1", Label = "Busco diálogo direto" },
                                new QuestionnaireOptionDto { Id = "opt2", Label = "Prefiro mediação" },
                                new QuestionnaireOptionDto { Id = "opt3", Label = "Evito confrontos" },
                                new QuestionnaireOptionDto { Id = "opt4", Label = "Busco soluções criativas" },
                                new QuestionnaireOptionDto { Id = "opt5", Label = "Escalo para gestão" }
                            }
                        }
                    }
                },
                new QuestionnaireSectionDto
                {
                    Id = "sec7",
                    Title = "Tomada de Decisão e Desempenho",
                    Order = 6,
                    Questions = new List<QuestionnaireQuestionDto>
                    {
                        new QuestionnaireQuestionDto
                        {
                            Id = "q7",
                            Prompt = "Como você toma decisões importantes?",
                            Type = "single",
                            Options = new List<QuestionnaireOptionDto>
                            {
                                new QuestionnaireOptionDto { Id = "opt1", Label = "Baseado em dados" },
                                new QuestionnaireOptionDto { Id = "opt2", Label = "Intuição" },
                                new QuestionnaireOptionDto { Id = "opt3", Label = "Consulto a equipe" },
                                new QuestionnaireOptionDto { Id = "opt4", Label = "Análise de riscos" },
                                new QuestionnaireOptionDto { Id = "opt5", Label = "Experiência passada" }
                            }
                        }
                    }
                },
                new QuestionnaireSectionDto
                {
                    Id = "sec8",
                    Title = "Estilo Pessoal e Preferências",
                    Order = 7,
                    Questions = new List<QuestionnaireQuestionDto>
                    {
                        new QuestionnaireQuestionDto
                        {
                            Id = "q8",
                            Prompt = "Qual ambiente de trabalho você prefere?",
                            Type = "single",
                            Options = new List<QuestionnaireOptionDto>
                            {
                                new QuestionnaireOptionDto { Id = "opt1", Label = "Dinâmico e agitado" },
                                new QuestionnaireOptionDto { Id = "opt2", Label = "Calmo e organizado" },
                                new QuestionnaireOptionDto { Id = "opt3", Label = "Criativo e flexível" },
                                new QuestionnaireOptionDto { Id = "opt4", Label = "Estruturado e formal" },
                                new QuestionnaireOptionDto { Id = "opt5", Label = "Colaborativo e social" }
                            }
                        }
                    }
                },
                new QuestionnaireSectionDto
                {
                    Id = "sec9",
                    Title = "Diversidade e Inclusão",
                    Order = 8,
                    Questions = new List<QuestionnaireQuestionDto>
                    {
                        new QuestionnaireQuestionDto
                        {
                            Id = "q9",
                            Prompt = "Como você contribui para um ambiente inclusivo?",
                            Type = "multi",
                            MaxSelections = 2,
                            Options = new List<QuestionnaireOptionDto>
                            {
                                new QuestionnaireOptionDto { Id = "opt1", Label = "Escuto diferentes perspectivas" },
                                new QuestionnaireOptionDto { Id = "opt2", Label = "Promovo diversidade na equipe" },
                                new QuestionnaireOptionDto { Id = "opt3", Label = "Questiono preconceitos" },
                                new QuestionnaireOptionDto { Id = "opt4", Label = "Apoio colegas minoritários" },
                                new QuestionnaireOptionDto { Id = "opt5", Label = "Busco educação contínua" }
                            }
                        }
                    }
                }
            };
        }

        public async Task<QuestionnaireDto> GetCompleteQuestionnaireAsync()
        {
            try
            {
                _logger.LogInformation("Getting complete questionnaire with all fixed psychology questions");

                // For psychology tests, return ALL fixed questions - no randomization
                // This matches the implementation requirement for fixed questions
                return await GetQuestionnaireAsync(); // Returns the complete questionnaire structure
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting complete questionnaire structure");
                throw;
            }
        }

        public async Task<bool> ProcessFixedResponsesAsync(PsychologyTestSubmissionDto submission)
        {
            try
            {
                _logger.LogInformation("Processing fixed psychology test responses for candidate {CandidateId}, test {TestId}",
                    submission.CandidateId, submission.TestId);

                // Validate the submission
                if (submission.FixedResponses == null || !submission.FixedResponses.Any())
                {
                    _logger.LogWarning("No responses provided in psychology test submission");
                    return false;
                }

                // Get the psychology test
                var psychologyTest = await _unitOfWork.PsychologyTests.GetByIdAsync(submission.TestId);
                if (psychologyTest == null)
                {
                    _logger.LogWarning("Psychology test not found: {TestId}", submission.TestId);
                    return false;
                }

                // Verify the test belongs to the candidate
                if (psychologyTest.Candidate?.Id != submission.CandidateId)
                {
                    _logger.LogWarning("Psychology test {TestId} does not belong to candidate {CandidateId}",
                        submission.TestId, submission.CandidateId);
                    return false;
                }

                // Process each fixed response (A, B, C, D, E format)
                var processedResponses = new Dictionary<string, string>();
                foreach (var response in submission.FixedResponses)
                {
                    var questionNumber = response.Key;
                    var selectedOption = response.Value.ToUpper();

                    // Validate option is A, B, C, D, or E
                    if (!new[] { "A", "B", "C", "D", "E" }.Contains(selectedOption))
                    {
                        _logger.LogWarning("Invalid response option '{Option}' for question {QuestionNumber}",
                            selectedOption, questionNumber);
                        continue;
                    }

                    processedResponses[questionNumber] = selectedOption;
                }

                // Update psychology test with responses and completion status
                psychologyTest.Status = Data.Models.TestStatus.Submitted;
                psychologyTest.CompletedAt = submission.SubmittedAt;

                // In a real implementation, you would store the responses in a related table
                // For now, we'll just mark the test as submitted
                _unitOfWork.PsychologyTests.Update(psychologyTest);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully processed {ResponseCount} psychology test responses for candidate {CandidateId}",
                    processedResponses.Count, submission.CandidateId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing fixed psychology test responses for candidate {CandidateId}",
                    submission.CandidateId);
                return false;
            }
        }
    }
}