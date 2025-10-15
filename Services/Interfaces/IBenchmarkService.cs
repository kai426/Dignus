using Dignus.Candidate.Back.DTOs;

namespace Dignus.Candidate.Back.Services.Interfaces
{
    /// <summary>
    /// Service interface for benchmark comparison operations
    /// </summary>
    public interface IBenchmarkService
    {
        /// <summary>
        /// Compares candidate performance against market benchmarks
        /// </summary>
        /// <param name="candidateId">Candidate identifier</param>
        /// <param name="position">Position/role to benchmark against</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Benchmark comparison results</returns>
        Task<BenchmarkComparisonDto> CompareToBenchmarkAsync(Guid candidateId, string position, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets available benchmark positions
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of available positions for benchmarking</returns>
        Task<List<BenchmarkPositionDto>> GetAvailablePositionsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Calculates percentile ranking for a candidate
        /// </summary>
        /// <param name="candidateId">Candidate identifier</param>
        /// <param name="position">Position to compare against</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Percentile ranking (0-100)</returns>
        Task<int> CalculatePercentileRankAsync(Guid candidateId, string position, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets market insights for a specific competency
        /// </summary>
        /// <param name="competencyName">Name of the competency</param>
        /// <param name="position">Position context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Market insights and statistics</returns>
        Task<MarketInsightDto> GetCompetencyMarketInsightAsync(string competencyName, string position, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates benchmark data with new candidate evaluations
        /// </summary>
        /// <param name="candidateEvaluation">Candidate evaluation to include in benchmarks</param>
        /// <param name="position">Position this candidate evaluated for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success status</returns>
        Task<bool> UpdateBenchmarkDataAsync(CandidateEvaluationDto candidateEvaluation, string position, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets trending competencies and market demands
        /// </summary>
        /// <param name="position">Position to analyze</param>
        /// <param name="timeRange">Time range for trend analysis</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Trending competencies data</returns>
        Task<CompetencyTrendDto> GetCompetencyTrendsAsync(string position, TimeRange timeRange, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates benchmark statistics for administrative purposes
        /// </summary>
        /// <param name="position">Position to generate stats for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Comprehensive benchmark statistics</returns>
        Task<BenchmarkStatisticsDto> GenerateBenchmarkStatisticsAsync(string position, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// DTO for benchmark position information
    /// </summary>
    public class BenchmarkPositionDto
    {
        /// <summary>
        /// Position identifier
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Position name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Position category
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Number of candidates benchmarked for this position
        /// </summary>
        public int CandidateCount { get; set; }

        /// <summary>
        /// Average score for this position
        /// </summary>
        public decimal AverageScore { get; set; }

        /// <summary>
        /// Key competencies for this position
        /// </summary>
        public List<string> KeyCompetencies { get; set; } = new();

        /// <summary>
        /// Whether this position has sufficient data for reliable benchmarking
        /// </summary>
        public bool HasSufficientData { get; set; }

        /// <summary>
        /// Last updated timestamp
        /// </summary>
        public DateTimeOffset LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO for market insight information
    /// </summary>
    public class MarketInsightDto
    {
        /// <summary>
        /// Competency name
        /// </summary>
        public string CompetencyName { get; set; } = string.Empty;

        /// <summary>
        /// Position context
        /// </summary>
        public string Position { get; set; } = string.Empty;

        /// <summary>
        /// Market average score
        /// </summary>
        public decimal MarketAverage { get; set; }

        /// <summary>
        /// Standard deviation
        /// </summary>
        public decimal StandardDeviation { get; set; }

        /// <summary>
        /// Top 10% threshold score
        /// </summary>
        public decimal Top10PercentThreshold { get; set; }

        /// <summary>
        /// Bottom 10% threshold score
        /// </summary>
        public decimal Bottom10PercentThreshold { get; set; }

        /// <summary>
        /// Market demand level for this competency
        /// </summary>
        public DemandLevel DemandLevel { get; set; }

        /// <summary>
        /// Trend direction for this competency
        /// </summary>
        public TrendDirection TrendDirection { get; set; }

        /// <summary>
        /// Sample size used for calculations
        /// </summary>
        public int SampleSize { get; set; }

        /// <summary>
        /// Insights and recommendations
        /// </summary>
        public List<string> Insights { get; set; } = new();
    }

    /// <summary>
    /// DTO for competency trend information
    /// </summary>
    public class CompetencyTrendDto
    {
        /// <summary>
        /// Position analyzed
        /// </summary>
        public string Position { get; set; } = string.Empty;

        /// <summary>
        /// Time range analyzed
        /// </summary>
        public TimeRange TimeRange { get; set; }

        /// <summary>
        /// Trending competencies (increasing importance)
        /// </summary>
        public List<TrendingCompetencyDto> TrendingUp { get; set; } = new();

        /// <summary>
        /// Declining competencies (decreasing importance)
        /// </summary>
        public List<TrendingCompetencyDto> TrendingDown { get; set; } = new();

        /// <summary>
        /// Stable competencies (consistent importance)
        /// </summary>
        public List<TrendingCompetencyDto> Stable { get; set; } = new();

        /// <summary>
        /// Emerging competencies (new to the market)
        /// </summary>
        public List<TrendingCompetencyDto> Emerging { get; set; } = new();

        /// <summary>
        /// Analysis timestamp
        /// </summary>
        public DateTimeOffset AnalysisDate { get; set; }
    }

    /// <summary>
    /// DTO for trending competency information
    /// </summary>
    public class TrendingCompetencyDto
    {
        /// <summary>
        /// Competency name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Current market average score
        /// </summary>
        public decimal CurrentAverage { get; set; }

        /// <summary>
        /// Previous period average score
        /// </summary>
        public decimal PreviousAverage { get; set; }

        /// <summary>
        /// Percentage change
        /// </summary>
        public decimal PercentageChange { get; set; }

        /// <summary>
        /// Trend strength (how significant the trend is)
        /// </summary>
        public TrendStrength Strength { get; set; }

        /// <summary>
        /// Market demand level
        /// </summary>
        public DemandLevel DemandLevel { get; set; }
    }

    /// <summary>
    /// DTO for benchmark statistics
    /// </summary>
    public class BenchmarkStatisticsDto
    {
        /// <summary>
        /// Position analyzed
        /// </summary>
        public string Position { get; set; } = string.Empty;

        /// <summary>
        /// Total number of candidates in benchmark
        /// </summary>
        public int TotalCandidates { get; set; }

        /// <summary>
        /// Overall statistics
        /// </summary>
        public BenchmarkStatsDto OverallStats { get; set; } = new();

        /// <summary>
        /// Statistics per competency
        /// </summary>
        public List<CompetencyStatsDto> CompetencyStats { get; set; } = new();

        /// <summary>
        /// Score distribution percentiles
        /// </summary>
        public Dictionary<int, decimal> ScorePercentiles { get; set; } = new();

        /// <summary>
        /// Data quality indicators
        /// </summary>
        public DataQualityDto DataQuality { get; set; } = new();

        /// <summary>
        /// Generation timestamp
        /// </summary>
        public DateTimeOffset GeneratedAt { get; set; }
    }

    /// <summary>
    /// DTO for overall benchmark statistics
    /// </summary>
    public class BenchmarkStatsDto
    {
        /// <summary>
        /// Mean score
        /// </summary>
        public decimal Mean { get; set; }

        /// <summary>
        /// Median score
        /// </summary>
        public decimal Median { get; set; }

        /// <summary>
        /// Standard deviation
        /// </summary>
        public decimal StandardDeviation { get; set; }

        /// <summary>
        /// Minimum score
        /// </summary>
        public decimal Minimum { get; set; }

        /// <summary>
        /// Maximum score
        /// </summary>
        public decimal Maximum { get; set; }

        /// <summary>
        /// Skewness of distribution
        /// </summary>
        public decimal Skewness { get; set; }

        /// <summary>
        /// Kurtosis of distribution
        /// </summary>
        public decimal Kurtosis { get; set; }
    }

    /// <summary>
    /// DTO for competency-specific statistics
    /// </summary>
    public class CompetencyStatsDto
    {
        /// <summary>
        /// Competency name
        /// </summary>
        public string CompetencyName { get; set; } = string.Empty;

        /// <summary>
        /// Overall statistics for this competency
        /// </summary>
        public BenchmarkStatsDto Stats { get; set; } = new();

        /// <summary>
        /// Correlation with overall performance
        /// </summary>
        public decimal CorrelationWithOverall { get; set; }

        /// <summary>
        /// Importance weight in overall evaluation
        /// </summary>
        public decimal ImportanceWeight { get; set; }
    }

    /// <summary>
    /// DTO for data quality information
    /// </summary>
    public class DataQualityDto
    {
        /// <summary>
        /// Completeness score (0.0-1.0)
        /// </summary>
        public decimal Completeness { get; set; }

        /// <summary>
        /// Data freshness (days since last update)
        /// </summary>
        public int FreshnessDays { get; set; }

        /// <summary>
        /// Consistency score (0.0-1.0)
        /// </summary>
        public decimal Consistency { get; set; }

        /// <summary>
        /// Sample size adequacy indicator
        /// </summary>
        public SampleAdequacy SampleAdequacy { get; set; }

        /// <summary>
        /// Data quality issues identified
        /// </summary>
        public List<string> QualityIssues { get; set; } = new();
    }

    #region Enums

    /// <summary>
    /// Market demand level enumeration
    /// </summary>
    public enum DemandLevel
    {
        VeryLow,
        Low,
        Moderate,
        High,
        VeryHigh
    }

    /// <summary>
    /// Trend direction enumeration
    /// </summary>
    public enum TrendDirection
    {
        StronglyDecreasing,
        Decreasing,
        Stable,
        Increasing,
        StronglyIncreasing
    }

    /// <summary>
    /// Time range enumeration
    /// </summary>
    public enum TimeRange
    {
        Last3Months,
        Last6Months,
        LastYear,
        Last2Years,
        AllTime
    }

    /// <summary>
    /// Trend strength enumeration
    /// </summary>
    public enum TrendStrength
    {
        Weak,
        Moderate,
        Strong,
        VeryStrong
    }

    /// <summary>
    /// Sample adequacy enumeration
    /// </summary>
    public enum SampleAdequacy
    {
        Inadequate,
        Marginal,
        Adequate,
        Good,
        Excellent
    }

    #endregion
}