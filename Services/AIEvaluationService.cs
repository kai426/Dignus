using Dignus.Candidate.Back.Configuration;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;
using Dignus.Data.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using AutoMapper;

namespace Dignus.Candidate.Back.Services
{
    /// <summary>
    /// Service implementation for comprehensive AI evaluation and scoring
    /// </summary>
    public class AIEvaluationService : IAIEvaluationService
    {
        private readonly AISettings _aiSettings;
        private readonly ILogger<AIEvaluationService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAITranscriptionService _transcriptionService;
        private readonly IAIVideoAnalysisService _videoAnalysisService;

        public AIEvaluationService(
            IOptions<AISettings> aiSettings,
            ILogger<AIEvaluationService> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IAITranscriptionService transcriptionService,
            IAIVideoAnalysisService videoAnalysisService)
        {
            _aiSettings = aiSettings.Value;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _transcriptionService = transcriptionService;
            _videoAnalysisService = videoAnalysisService;
        }

        public async Task<CandidateEvaluationDto> GenerateComprehensiveEvaluationAsync(Guid candidateId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting comprehensive evaluation for candidate {CandidateId}", candidateId);

                // Get candidate information
                var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
                if (candidate == null)
                {
                    throw new ArgumentException($"Candidate with ID {candidateId} not found");
                }

                // Get all test evaluations
                var testEvaluations = await GetTestEvaluationsForCandidateAsync(candidateId, cancellationToken);
                
                // Get audio evaluations
                var audioEvaluations = await GetAudioEvaluationsForCandidateAsync(candidateId, cancellationToken);
                
                // Get behavioral evaluations
                var behavioralEvaluations = await GetBehavioralEvaluationsForCandidateAsync(candidateId, cancellationToken);

                // Calculate overall score
                var overallScore = await CalculateOverallScoreAsync(candidateId, null, cancellationToken);

                // Generate executive summary and recommendations
                var evaluationSummary = await GenerateEvaluationSummaryAsync(
                    candidateId, testEvaluations, audioEvaluations, behavioralEvaluations, cancellationToken);

                var result = new CandidateEvaluationDto
                {
                    CandidateId = candidateId,
                    Candidate = _mapper.Map<CandidateDto>(candidate),
                    TestEvaluations = testEvaluations,
                    AudioEvaluations = audioEvaluations,
                    BehavioralEvaluations = behavioralEvaluations,
                    OverallScore = overallScore.OverallScore,
                    Recommendation = DetermineRecommendationLevel(overallScore.OverallScore),
                    KeyStrengths = evaluationSummary.KeyStrengths,
                    DevelopmentAreas = evaluationSummary.DevelopmentAreas,
                    ExecutiveSummary = evaluationSummary.ExecutiveSummary,
                    RiskFactors = evaluationSummary.RiskFactors,
                    CulturalFit = evaluationSummary.CulturalFit,
                    EvaluatedAt = DateTimeOffset.UtcNow,
                    EvaluationConfidence = evaluationSummary.EvaluationConfidence
                };

                _logger.LogInformation("Comprehensive evaluation completed for candidate {CandidateId} with overall score {OverallScore}", 
                    candidateId, result.OverallScore);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating comprehensive evaluation for candidate {CandidateId}", candidateId);
                throw;
            }
        }

        public async Task<TestEvaluationDto> EvaluateTestAnswersAsync(Guid testId, IEnumerable<TestAnswerDto> answers, string testType, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting AI-enhanced test evaluation for test {TestId} of type {TestType}", testId, testType);

                // Get the test and questions
                var test = await GetTestByIdAsync(testId, testType);
                if (test == null)
                {
                    throw new ArgumentException($"Test with ID {testId} not found");
                }

                // Calculate raw score
                var rawScore = await CalculateRawScoreAsync(testId, answers, testType);
                
                // Get AI-enhanced evaluation
                var enhancedEvaluation = await GetAIEnhancedEvaluationAsync(answers, testType, cancellationToken);

                var result = new TestEvaluationDto
                {
                    TestId = testId,
                    TestType = testType,
                    RawScore = rawScore,
                    EnhancedScore = enhancedEvaluation.EnhancedScore,
                    CompletionTimeMinutes = CalculateCompletionTime(answers),
                    EfficiencyScore = CalculateEfficiencyScore(rawScore, CalculateCompletionTime(answers)),
                    AnswerPatternAnalysis = enhancedEvaluation.AnswerPatternAnalysis,
                    Strengths = enhancedEvaluation.Strengths,
                    Weaknesses = enhancedEvaluation.Weaknesses,
                    DetailedFeedback = enhancedEvaluation.DetailedFeedback,
                    PercentileRanking = await CalculatePercentileRankingAsync(rawScore, testType),
                    EvaluatedAt = DateTimeOffset.UtcNow
                };

                _logger.LogInformation("Test evaluation completed for test {TestId} with enhanced score {EnhancedScore}", 
                    testId, result.EnhancedScore);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating test answers for test {TestId}", testId);
                throw;
            }
        }

        public async Task<PersonalizedFeedbackDto> GeneratePersonalizedFeedbackAsync(Guid candidateId, CandidateEvaluationDto evaluationResults, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Generating personalized feedback for candidate {CandidateId}", candidateId);

                var feedbackPrompt = CreatePersonalizedFeedbackPrompt(evaluationResults);
                var feedbackResult = await GenerateFeedbackWithGeminiAsync(feedbackPrompt, cancellationToken);

                var result = new PersonalizedFeedbackDto
                {
                    CandidateId = candidateId,
                    CongratulationsMessage = feedbackResult.CongratulationsMessage,
                    KeyAchievements = feedbackResult.KeyAchievements,
                    DevelopmentRecommendations = feedbackResult.DevelopmentRecommendations,
                    CareerPathSuggestions = feedbackResult.CareerPathSuggestions,
                    LearningResources = feedbackResult.LearningResources,
                    NextSteps = feedbackResult.NextSteps,
                    GeneratedAt = DateTimeOffset.UtcNow
                };

                _logger.LogInformation("Personalized feedback generated for candidate {CandidateId}", candidateId);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating personalized feedback for candidate {CandidateId}", candidateId);
                throw;
            }
        }

        public async Task<OverallCandidateScoreDto> CalculateOverallScoreAsync(Guid candidateId, ScoringWeightsDto? weights = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Calculating overall score for candidate {CandidateId}", candidateId);

                // Use default weights if none provided
                weights ??= new ScoringWeightsDto();

                // Get component scores
                var componentScores = await GetComponentScoresAsync(candidateId, cancellationToken);

                // Calculate weighted score
                var overallScore = (int)Math.Round(
                    componentScores.TechnicalSkills * weights.TechnicalSkillsWeight +
                    componentScores.CommunicationSkills * weights.CommunicationSkillsWeight +
                    componentScores.BehavioralAssessment * weights.BehavioralAssessmentWeight +
                    componentScores.CognitiveAbilities * weights.CognitiveAbilitiesWeight +
                    componentScores.CulturalFit * weights.CulturalFitWeight);

                var result = new OverallCandidateScoreDto
                {
                    CandidateId = candidateId,
                    OverallScore = Math.Max(0, Math.Min(100, overallScore)), // Clamp to 0-100
                    ScoreBreakdown = componentScores,
                    ConfidenceInterval = CalculateConfidenceInterval(overallScore, componentScores),
                    PercentileRanking = await CalculateOverallPercentileAsync(overallScore),
                    ScoreTrend = await GetScoreTrendAsync(candidateId),
                    CalculatedAt = DateTimeOffset.UtcNow
                };

                _logger.LogInformation("Overall score calculated for candidate {CandidateId}: {OverallScore}", 
                    candidateId, result.OverallScore);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating overall score for candidate {CandidateId}", candidateId);
                throw;
            }
        }

        public async Task<BenchmarkComparisonDto> CompareAgainstBenchmarksAsync(Guid candidateId, string? jobRole = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Comparing candidate {CandidateId} against benchmarks for role {JobRole}", candidateId, jobRole ?? "general");

                // Get candidate's overall evaluation
                var candidateEvaluation = await GenerateComprehensiveEvaluationAsync(candidateId, cancellationToken);
                
                // Get benchmark data (in production, this would come from a database)
                var benchmarks = await GetBenchmarkDataAsync(jobRole, cancellationToken);

                var categoryComparisons = new List<BenchmarkCategoryDto>
                {
                    CreateCategoryComparison("Technical Skills", candidateEvaluation.TestEvaluations.Average(t => t.EnhancedScore), benchmarks.TechnicalSkills),
                    CreateCategoryComparison("Communication", candidateEvaluation.AudioEvaluations.Average(a => a.OverallScore), benchmarks.Communication),
                    CreateCategoryComparison("Behavioral", candidateEvaluation.BehavioralEvaluations.Average(b => b.OverallBehavioralScore), benchmarks.Behavioral),
                    CreateCategoryComparison("Overall Performance", candidateEvaluation.OverallScore, benchmarks.Overall)
                };

                var result = new BenchmarkComparisonDto
                {
                    CandidateId = candidateId,
                    JobRole = jobRole,
                    CategoryComparisons = categoryComparisons,
                    OverallFitScore = CalculateOverallFitScore(categoryComparisons),
                    Ranking = DetermineRanking(categoryComparisons.Average(c => c.Percentile)),
                    ComparisonSummary = GenerateComparisonSummary(categoryComparisons, jobRole),
                    ComparedAt = DateTimeOffset.UtcNow
                };

                _logger.LogInformation("Benchmark comparison completed for candidate {CandidateId} with fit score {FitScore}", 
                    candidateId, result.OverallFitScore);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing candidate {CandidateId} against benchmarks", candidateId);
                throw;
            }
        }

        // Helper methods implementation would continue here...
        // For brevity, I'll include just the key private methods

        private async Task<List<TestEvaluationDto>> GetTestEvaluationsForCandidateAsync(Guid candidateId, CancellationToken cancellationToken)
        {
            // Mock implementation - in production, get actual test data and evaluate
            await Task.Delay(500, cancellationToken);
            
            return new List<TestEvaluationDto>
            {
                new()
                {
                    TestId = Guid.NewGuid(),
                    TestType = "Math",
                    RawScore = 85,
                    EnhancedScore = 88,
                    CompletionTimeMinutes = 42,
                    EfficiencyScore = 82,
                    AnswerPatternAnalysis = "Strong performance in algebra and geometry, weaker in statistics",
                    Strengths = new List<string> { "Problem-solving skills", "Mathematical reasoning" },
                    Weaknesses = new List<string> { "Speed in complex calculations" },
                    DetailedFeedback = "Demonstrou sólido conhecimento matemático com raciocínio lógico bem desenvolvido.",
                    PercentileRanking = 78,
                    EvaluatedAt = DateTimeOffset.UtcNow
                }
            };
        }

        private async Task<List<AudioEvaluationDto>> GetAudioEvaluationsForCandidateAsync(Guid candidateId, CancellationToken cancellationToken)
        {
            // Mock implementation
            await Task.Delay(300, cancellationToken);
            return new List<AudioEvaluationDto>();
        }

        private async Task<List<BehavioralEvaluationDto>> GetBehavioralEvaluationsForCandidateAsync(Guid candidateId, CancellationToken cancellationToken)
        {
            // Mock implementation
            await Task.Delay(300, cancellationToken);
            return new List<BehavioralEvaluationDto>();
        }

        private async Task<object?> GetTestByIdAsync(Guid testId, string testType)
        {
            return testType.ToLowerInvariant() switch
            {
                "math" => await _unitOfWork.MathTests.GetByIdAsync(testId),
                "portuguese" => await _unitOfWork.PortugueseTests.GetByIdAsync(testId),
                "psychology" => await _unitOfWork.PsychologyTests.GetByIdAsync(testId),
                _ => null
            };
        }

        private static RecommendationLevel DetermineRecommendationLevel(int overallScore)
        {
            return overallScore switch
            {
                >= 90 => RecommendationLevel.StronglyRecommended,
                >= 75 => RecommendationLevel.Recommended,
                >= 60 => RecommendationLevel.Conditional,
                >= 40 => RecommendationLevel.NotRecommended,
                _ => RecommendationLevel.StronglyNotRecommended
            };
        }

        private async Task<EvaluationSummary> GenerateEvaluationSummaryAsync(Guid candidateId, List<TestEvaluationDto> testEvaluations, List<AudioEvaluationDto> audioEvaluations, List<BehavioralEvaluationDto> behavioralEvaluations, CancellationToken cancellationToken)
        {
            // Mock implementation - in production, use AI to generate comprehensive summary
            await Task.Delay(1000, cancellationToken);
            
            return new EvaluationSummary
            {
                KeyStrengths = new List<string> { "Strong analytical skills", "Good communication abilities" },
                DevelopmentAreas = new List<string> { "Time management", "Technical depth" },
                ExecutiveSummary = "Candidato com potencial sólido, demonstrando competências técnicas adequadas e boa capacidade de comunicação.",
                RiskFactors = new List<string>(),
                CulturalFit = new CulturalFitDto
                {
                    ValuesAlignmentScore = 85,
                    WorkStyleCompatibility = 78,
                    CommunicationStyleFit = 82,
                    TeamCollaborationScore = 80,
                    FitAssessmentDetails = "Boa adaptação esperada ao ambiente organizacional",
                    OverallCulturalFitScore = 81
                },
                EvaluationConfidence = 0.85
            };
        }

        private async Task<ScoreBreakdownDto> GetComponentScoresAsync(Guid candidateId, CancellationToken cancellationToken)
        {
            // Mock implementation
            await Task.Delay(200, cancellationToken);
            
            return new ScoreBreakdownDto
            {
                TechnicalSkills = 85,
                CommunicationSkills = 78,
                BehavioralAssessment = 82,
                CognitiveAbilities = 88,
                CulturalFit = 81
            };
        }

        private static int CalculateRawScore(IEnumerable<TestAnswerDto> answers)
        {
            // Mock calculation - in production, compare with correct answers
            return 85;
        }

        private static int CalculateCompletionTime(IEnumerable<TestAnswerDto> answers)
        {
            return answers.Sum(a => a.TimeSpentSeconds) / 60; // Convert to minutes
        }

        private static int CalculateEfficiencyScore(int rawScore, int completionTimeMinutes)
        {
            // Simple efficiency calculation - in production, use more sophisticated algorithm
            var timeEfficiency = Math.Max(0, 100 - Math.Max(0, completionTimeMinutes - 45)); // 45 min baseline
            return (int)Math.Round((rawScore * 0.7) + (timeEfficiency * 0.3));
        }

        private BenchmarkCategoryDto CreateCategoryComparison(string categoryName, double candidateScore, int benchmarkAverage)
        {
            var percentile = (int)Math.Round((candidateScore / benchmarkAverage) * 50 + 25); // Simplified percentile calculation
            
            return new BenchmarkCategoryDto
            {
                CategoryName = categoryName,
                CandidateScore = (int)candidateScore,
                BenchmarkAverage = benchmarkAverage,
                PerformanceLevel = candidateScore >= benchmarkAverage ? "Above" : candidateScore >= benchmarkAverage * 0.9 ? "At" : "Below",
                Percentile = Math.Max(1, Math.Min(99, percentile))
            };
        }

        private class EvaluationSummary
        {
            public List<string> KeyStrengths { get; set; } = new();
            public List<string> DevelopmentAreas { get; set; } = new();
            public string ExecutiveSummary { get; set; } = null!;
            public List<string> RiskFactors { get; set; } = new();
            public CulturalFitDto CulturalFit { get; set; } = null!;
            public double EvaluationConfidence { get; set; }
        }

        private class BenchmarkData
        {
            public int TechnicalSkills { get; set; } = 75;
            public int Communication { get; set; } = 72;
            public int Behavioral { get; set; } = 78;
            public int Overall { get; set; } = 75;
        }

        // Additional helper method stubs...
        private Task<int> CalculateRawScoreAsync(Guid testId, IEnumerable<TestAnswerDto> answers, string testType) => Task.FromResult(85);
        private Task<TestEnhancedEvaluation> GetAIEnhancedEvaluationAsync(IEnumerable<TestAnswerDto> answers, string testType, CancellationToken cancellationToken) => Task.FromResult(new TestEnhancedEvaluation());
        private Task<int> CalculatePercentileRankingAsync(int rawScore, string testType) => Task.FromResult(75);
        private string CreatePersonalizedFeedbackPrompt(CandidateEvaluationDto evaluationResults) => "Mock prompt";
        private Task<PersonalizedFeedbackResult> GenerateFeedbackWithGeminiAsync(string prompt, CancellationToken cancellationToken) => Task.FromResult(new PersonalizedFeedbackResult());
        private static ConfidenceIntervalDto CalculateConfidenceInterval(int overallScore, ScoreBreakdownDto componentScores) => new() { LowerBound = overallScore - 5, UpperBound = overallScore + 5, ConfidenceLevel = 95 };
        private Task<int> CalculateOverallPercentileAsync(int overallScore) => Task.FromResult(78);
        private Task<string?> GetScoreTrendAsync(Guid candidateId) => Task.FromResult<string?>("Stable");
        private Task<BenchmarkData> GetBenchmarkDataAsync(string? jobRole, CancellationToken cancellationToken) => Task.FromResult(new BenchmarkData());
        private static int CalculateOverallFitScore(List<BenchmarkCategoryDto> categoryComparisons) => (int)categoryComparisons.Average(c => c.Percentile);
        private static string DetermineRanking(double averagePercentile) => averagePercentile >= 75 ? "Top Quartile" : averagePercentile >= 50 ? "Above Average" : "Below Average";
        private static string GenerateComparisonSummary(List<BenchmarkCategoryDto> comparisons, string? jobRole) => $"Performance summary for {jobRole ?? "general"} position shows mixed results across categories.";

        private class TestEnhancedEvaluation
        {
            public int EnhancedScore { get; set; } = 88;
            public string AnswerPatternAnalysis { get; set; } = "Mock analysis";
            public List<string> Strengths { get; set; } = new();
            public List<string> Weaknesses { get; set; } = new();
            public string DetailedFeedback { get; set; } = "Mock feedback";
        }

        private class PersonalizedFeedbackResult
        {
            public string CongratulationsMessage { get; set; } = "Parabéns pelo seu desempenho!";
            public List<string> KeyAchievements { get; set; } = new();
            public List<DevelopmentRecommendationDto> DevelopmentRecommendations { get; set; } = new();
            public List<string> CareerPathSuggestions { get; set; } = new();
            public List<LearningResourceDto> LearningResources { get; set; } = new();
            public string NextSteps { get; set; } = "Aguarde contato da equipe de RH";
        }
    }
}