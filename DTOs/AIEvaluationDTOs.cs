using System.ComponentModel.DataAnnotations;

namespace Dignus.Candidate.Back.DTOs
{
    /// <summary>
    /// DTO for comprehensive candidate evaluation
    /// </summary>
    public class CandidateEvaluationDto
    {
        /// <summary>
        /// The ID of the candidate
        /// </summary>
        public Guid CandidateId { get; set; }
        
        /// <summary>
        /// Candidate basic information
        /// </summary>
        public CandidateDto Candidate { get; set; } = null!;

        /// <summary>
        /// Full name of the candidate
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Current status in the recruitment process
        /// </summary>
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// Test evaluations (Math, Portuguese, Psychology)
        /// </summary>
        public List<TestEvaluationDto> TestEvaluations { get; set; } = new();
        
        /// <summary>
        /// Audio evaluation results
        /// </summary>
        public List<AudioEvaluationDto> AudioEvaluations { get; set; } = new();
        
        /// <summary>
        /// Video/behavioral analysis results
        /// </summary>
        public List<BehavioralEvaluationDto> BehavioralEvaluations { get; set; } = new();
        
        /// <summary>
        /// Overall candidate score (0-100)
        /// </summary>
        [Range(0, 100)]
        public int OverallScore { get; set; }
        
        /// <summary>
        /// Recommendation level for hiring
        /// </summary>
        public RecommendationLevel Recommendation { get; set; }
        
        /// <summary>
        /// Key strengths across all evaluations
        /// </summary>
        public List<string> KeyStrengths { get; set; } = new();
        
        /// <summary>
        /// Primary areas for development
        /// </summary>
        public List<string> DevelopmentAreas { get; set; } = new();
        
        /// <summary>
        /// Executive summary of the candidate
        /// </summary>
        public string ExecutiveSummary { get; set; } = null!;
        
        /// <summary>
        /// Risk factors identified
        /// </summary>
        public List<string> RiskFactors { get; set; } = new();
        
        /// <summary>
        /// Cultural fit assessment
        /// </summary>
        public CulturalFitDto CulturalFit { get; set; } = null!;
        
        /// <summary>
        /// Evaluation completion timestamp
        /// </summary>
        public DateTimeOffset EvaluatedAt { get; set; }
        
        /// <summary>
        /// AI confidence in the evaluation (0.0-1.0)
        /// </summary>
        [Range(0.0, 1.0)]
        public double EvaluationConfidence { get; set; }
    }

    /// <summary>
    /// DTO for individual test evaluation
    /// </summary>
    public class TestEvaluationDto
    {
        /// <summary>
        /// The ID of the test
        /// </summary>
        public Guid TestId { get; set; }
        
        /// <summary>
        /// Type of test (Math, Portuguese, Psychology)
        /// </summary>
        public string TestType { get; set; } = null!;
        
        /// <summary>
        /// Raw score based on correct answers
        /// </summary>
        [Range(0, 100)]
        public int RawScore { get; set; }
        
        /// <summary>
        /// AI-enhanced score considering partial credit and reasoning
        /// </summary>
        [Range(0, 100)]
        public int EnhancedScore { get; set; }
        
        /// <summary>
        /// Time taken to complete the test in minutes
        /// </summary>
        public int CompletionTimeMinutes { get; set; }
        
        /// <summary>
        /// Efficiency score based on time vs accuracy
        /// </summary>
        [Range(0, 100)]
        public int EfficiencyScore { get; set; }
        
        /// <summary>
        /// Analysis of answer patterns
        /// </summary>
        public string AnswerPatternAnalysis { get; set; } = null!;
        
        /// <summary>
        /// Strengths demonstrated in this test
        /// </summary>
        public List<string> Strengths { get; set; } = new();
        
        /// <summary>
        /// Weaknesses identified in this test
        /// </summary>
        public List<string> Weaknesses { get; set; } = new();
        
        /// <summary>
        /// Detailed feedback for the test performance
        /// </summary>
        public string DetailedFeedback { get; set; } = null!;
        
        /// <summary>
        /// Percentile ranking compared to other candidates
        /// </summary>
        [Range(0, 100)]
        public int PercentileRanking { get; set; }
        
        /// <summary>
        /// Evaluation timestamp
        /// </summary>
        public DateTimeOffset EvaluatedAt { get; set; }
    }

    /// <summary>
    /// DTO for individual test answer
    /// </summary>
    public class TestAnswerDto
    {
        /// <summary>
        /// The ID of the question
        /// </summary>
        public Guid QuestionId { get; set; }
        
        /// <summary>
        /// The selected answer
        /// </summary>
        public string SelectedAnswer { get; set; } = null!;
        
        /// <summary>
        /// Time spent on this question in seconds
        /// </summary>
        public int TimeSpentSeconds { get; set; }
        
        /// <summary>
        /// Whether the answer was changed before submission
        /// </summary>
        public bool WasChanged { get; set; }
    }

    /// <summary>
    /// DTO for personalized feedback
    /// </summary>
    public class PersonalizedFeedbackDto
    {
        /// <summary>
        /// The ID of the candidate
        /// </summary>
        public Guid CandidateId { get; set; }
        
        /// <summary>
        /// Personalized congratulations message
        /// </summary>
        public string CongratulationsMessage { get; set; } = null!;
        
        /// <summary>
        /// Key achievements highlighted
        /// </summary>
        public List<string> KeyAchievements { get; set; } = new();
        
        /// <summary>
        /// Specific development recommendations
        /// </summary>
        public List<DevelopmentRecommendationDto> DevelopmentRecommendations { get; set; } = new();
        
        /// <summary>
        /// Career path suggestions based on strengths
        /// </summary>
        public List<string> CareerPathSuggestions { get; set; } = new();
        
        /// <summary>
        /// Learning resources recommendations
        /// </summary>
        public List<LearningResourceDto> LearningResources { get; set; } = new();
        
        /// <summary>
        /// Next steps in the process
        /// </summary>
        public string NextSteps { get; set; } = null!;
        
        /// <summary>
        /// Generated timestamp
        /// </summary>
        public DateTimeOffset GeneratedAt { get; set; }
    }

    /// <summary>
    /// DTO for development recommendations
    /// </summary>
    public class DevelopmentRecommendationDto
    {
        /// <summary>
        /// Area for development
        /// </summary>
        public string Area { get; set; } = null!;
        
        /// <summary>
        /// Priority level (High, Medium, Low)
        /// </summary>
        public string Priority { get; set; } = null!;
        
        /// <summary>
        /// Specific recommendation
        /// </summary>
        public string Recommendation { get; set; } = null!;
        
        /// <summary>
        /// Expected impact if addressed
        /// </summary>
        public string ExpectedImpact { get; set; } = null!;
    }

    /// <summary>
    /// DTO for learning resource recommendations
    /// </summary>
    public class LearningResourceDto
    {
        /// <summary>
        /// Title of the resource
        /// </summary>
        public string Title { get; set; } = null!;
        
        /// <summary>
        /// Type of resource (Course, Book, Article, etc.)
        /// </summary>
        public string Type { get; set; } = null!;
        
        /// <summary>
        /// URL to the resource (if applicable)
        /// </summary>
        public string? Url { get; set; }
        
        /// <summary>
        /// Brief description
        /// </summary>
        public string Description { get; set; } = null!;
        
        /// <summary>
        /// Relevance to the candidate's development needs
        /// </summary>
        public string Relevance { get; set; } = null!;
    }

    /// <summary>
    /// DTO for overall candidate scoring
    /// </summary>
    public class OverallCandidateScoreDto
    {
        /// <summary>
        /// The ID of the candidate
        /// </summary>
        public Guid CandidateId { get; set; }
        
        /// <summary>
        /// Overall composite score (0-100)
        /// </summary>
        [Range(0, 100)]
        public int OverallScore { get; set; }
        
        /// <summary>
        /// Breakdown of scores by category
        /// </summary>
        public ScoreBreakdownDto ScoreBreakdown { get; set; } = null!;
        
        /// <summary>
        /// Confidence interval for the score
        /// </summary>
        public ConfidenceIntervalDto ConfidenceInterval { get; set; } = null!;
        
        /// <summary>
        /// Percentile ranking among all candidates
        /// </summary>
        [Range(0, 100)]
        public int PercentileRanking { get; set; }
        
        /// <summary>
        /// Score trend over multiple assessments (if applicable)
        /// </summary>
        public string? ScoreTrend { get; set; }
        
        /// <summary>
        /// Calculation timestamp
        /// </summary>
        public DateTimeOffset CalculatedAt { get; set; }
    }

    /// <summary>
    /// DTO for score breakdown by categories
    /// </summary>
    public class ScoreBreakdownDto
    {
        /// <summary>
        /// Technical skills score (from tests)
        /// </summary>
        [Range(0, 100)]
        public int TechnicalSkills { get; set; }
        
        /// <summary>
        /// Communication skills score (from audio/video)
        /// </summary>
        [Range(0, 100)]
        public int CommunicationSkills { get; set; }
        
        /// <summary>
        /// Behavioral assessment score
        /// </summary>
        [Range(0, 100)]
        public int BehavioralAssessment { get; set; }
        
        /// <summary>
        /// Cognitive abilities score
        /// </summary>
        [Range(0, 100)]
        public int CognitiveAbilities { get; set; }
        
        /// <summary>
        /// Cultural fit score
        /// </summary>
        [Range(0, 100)]
        public int CulturalFit { get; set; }
    }

    /// <summary>
    /// DTO for confidence interval
    /// </summary>
    public class ConfidenceIntervalDto
    {
        /// <summary>
        /// Lower bound of confidence interval
        /// </summary>
        [Range(0, 100)]
        public int LowerBound { get; set; }
        
        /// <summary>
        /// Upper bound of confidence interval
        /// </summary>
        [Range(0, 100)]
        public int UpperBound { get; set; }
        
        /// <summary>
        /// Confidence level (e.g., 95%)
        /// </summary>
        [Range(0, 100)]
        public int ConfidenceLevel { get; set; }
    }

    /// <summary>
    /// DTO for scoring weights configuration
    /// </summary>
    public class ScoringWeightsDto
    {
        /// <summary>
        /// Weight for technical skills (0.0-1.0)
        /// </summary>
        [Range(0.0, 1.0)]
        public double TechnicalSkillsWeight { get; set; } = 0.3;
        
        /// <summary>
        /// Weight for communication skills (0.0-1.0)
        /// </summary>
        [Range(0.0, 1.0)]
        public double CommunicationSkillsWeight { get; set; } = 0.25;
        
        /// <summary>
        /// Weight for behavioral assessment (0.0-1.0)
        /// </summary>
        [Range(0.0, 1.0)]
        public double BehavioralAssessmentWeight { get; set; } = 0.25;
        
        /// <summary>
        /// Weight for cognitive abilities (0.0-1.0)
        /// </summary>
        [Range(0.0, 1.0)]
        public double CognitiveAbilitiesWeight { get; set; } = 0.15;
        
        /// <summary>
        /// Weight for cultural fit (0.0-1.0)
        /// </summary>
        [Range(0.0, 1.0)]
        public double CulturalFitWeight { get; set; } = 0.05;
    }

    /// <summary>
    /// DTO for benchmark comparison
    /// </summary>
    public class BenchmarkComparisonDto
    {
        /// <summary>
        /// The ID of the candidate
        /// </summary>
        public Guid CandidateId { get; set; }
        
        /// <summary>
        /// Target job role for comparison
        /// </summary>
        public string? JobRole { get; set; }

        /// <summary>
        /// Position in the ranking
        /// </summary>
        public string Position { get; set; } = string.Empty;

        /// <summary>
        /// Percentile rank compared to other candidates
        /// </summary>
        public decimal PercentileRank { get; set; }

        /// <summary>
        /// Gap from top performer
        /// </summary>
        public decimal TopPerformerGap { get; set; }

        /// <summary>
        /// Competency benchmark comparisons
        /// </summary>
        public List<CompetencyBenchmarkDto> CompetencyBenchmarks { get; set; } = new();
        
        /// <summary>
        /// Comparison results by category
        /// </summary>
        public List<BenchmarkCategoryDto> CategoryComparisons { get; set; } = new();
        
        /// <summary>
        /// Overall fit score compared to benchmarks (0-100)
        /// </summary>
        [Range(0, 100)]
        public int OverallFitScore { get; set; }
        
        /// <summary>
        /// Ranking among similar candidates
        /// </summary>
        public string Ranking { get; set; } = null!;
        
        /// <summary>
        /// Comparison summary
        /// </summary>
        public string ComparisonSummary { get; set; } = null!;
        
        /// <summary>
        /// Comparison timestamp
        /// </summary>
        public DateTimeOffset ComparedAt { get; set; }
    }

    /// <summary>
    /// DTO for benchmark category comparison
    /// </summary>
    public class BenchmarkCategoryDto
    {
        /// <summary>
        /// Category name
        /// </summary>
        public string CategoryName { get; set; } = null!;
        
        /// <summary>
        /// Candidate score in this category
        /// </summary>
        [Range(0, 100)]
        public int CandidateScore { get; set; }
        
        /// <summary>
        /// Benchmark average for this category
        /// </summary>
        [Range(0, 100)]
        public int BenchmarkAverage { get; set; }
        
        /// <summary>
        /// Performance vs benchmark (Above, At, Below)
        /// </summary>
        public string PerformanceLevel { get; set; } = null!;
        
        /// <summary>
        /// Percentile ranking in this category
        /// </summary>
        [Range(0, 100)]
        public int Percentile { get; set; }
    }

    /// <summary>
    /// DTO for competency benchmark comparison
    /// </summary>
    public class CompetencyBenchmarkDto
    {
        /// <summary>
        /// Competency name
        /// </summary>
        public string CompetencyName { get; set; } = string.Empty;

        /// <summary>
        /// Candidate's score
        /// </summary>
        public decimal CandidateScore { get; set; }

        /// <summary>
        /// Market average for this competency
        /// </summary>
        public decimal MarketAverage { get; set; }

        /// <summary>
        /// Variance from market average
        /// </summary>
        public decimal Variance { get; set; }

        /// <summary>
        /// Performance relative to market (Above/Below/Average)
        /// </summary>
        public string MarketPerformance { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for cultural fit assessment
    /// </summary>
    public class CulturalFitDto
    {
        /// <summary>
        /// Values alignment score (0-100)
        /// </summary>
        [Range(0, 100)]
        public int ValuesAlignmentScore { get; set; }
        
        /// <summary>
        /// Work style compatibility (0-100)
        /// </summary>
        [Range(0, 100)]
        public int WorkStyleCompatibility { get; set; }
        
        /// <summary>
        /// Communication style fit (0-100)
        /// </summary>
        [Range(0, 100)]
        public int CommunicationStyleFit { get; set; }
        
        /// <summary>
        /// Team collaboration indicators (0-100)
        /// </summary>
        [Range(0, 100)]
        public int TeamCollaborationScore { get; set; }
        
        /// <summary>
        /// Cultural fit assessment details
        /// </summary>
        public string FitAssessmentDetails { get; set; } = null!;
        
        /// <summary>
        /// Overall cultural fit score (0-100)
        /// </summary>
        [Range(0, 100)]
        public int OverallCulturalFitScore { get; set; }
    }

    /// <summary>
    /// Recommendation levels for hiring decisions
    /// </summary>
    public enum RecommendationLevel
    {
        /// <summary>
        /// Strong recommendation to hire
        /// </summary>
        StronglyRecommended,
        
        /// <summary>
        /// Recommended with some reservations
        /// </summary>
        Recommended,
        
        /// <summary>
        /// Conditional recommendation
        /// </summary>
        Conditional,
        
        /// <summary>
        /// Not recommended at this time
        /// </summary>
        NotRecommended,
        
        /// <summary>
        /// Strong recommendation not to hire
        /// </summary>
        StronglyNotRecommended,

        /// <summary>
        /// Approved for hire
        /// </summary>
        Approved,

        /// <summary>
        /// Requires additional review
        /// </summary>
        Review,

        /// <summary>
        /// Rejected
        /// </summary>
        Rejected
    }
}