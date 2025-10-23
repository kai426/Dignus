using System.ComponentModel.DataAnnotations;

namespace Dignus.Candidate.Back.DTOs
{
    /// <summary>
    /// Visual Retention test submission with fixed question responses
    /// </summary>
    public class VisualRetentionSubmissionDto
    {
        /// <summary>
        /// Candidate ID
        /// </summary>
        [Required]
        public Guid CandidateId { get; set; }

        /// <summary>
        /// Visual Retention test ID
        /// </summary>
        [Required]
        public Guid TestId { get; set; }

        /// <summary>
        /// Fixed responses in format: { "1": "A", "2": "C", "3": "B", ... }
        /// Each question number maps to selected answer option (A, B, C, D, E)
        /// </summary>
        [Required]
        public Dictionary<string, string> FixedResponses { get; set; } = new();

        /// <summary>
        /// When the test was submitted
        /// </summary>
        public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Duration taken to complete the test in minutes
        /// </summary>
        public int? DurationMinutes { get; set; }

        /// <summary>
        /// Total time allowed for the test (configured limit)
        /// </summary>
        public int? TimeAllowedMinutes { get; set; } = 20;

        /// <summary>
        /// Accuracy score (optional - can be calculated later)
        /// </summary>
        public decimal? AccuracyScore { get; set; }

        /// <summary>
        /// Speed score based on response times (optional)
        /// </summary>
        public decimal? SpeedScore { get; set; }

        /// <summary>
        /// Individual question response times in milliseconds
        /// Format: { "1": 5000, "2": 3200, "3": 4500, ... }
        /// </summary>
        public Dictionary<string, int>? ResponseTimes { get; set; }
    }
}