using System.ComponentModel.DataAnnotations;

namespace Dignus.Candidate.Back.DTOs
{
    /// <summary>
    /// Psychology test submission with fixed question responses
    /// </summary>
    public class PsychologyTestSubmissionDto
    {
        /// <summary>
        /// Candidate ID
        /// </summary>
        [Required]
        public Guid CandidateId { get; set; }

        /// <summary>
        /// Psychology test ID
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
        /// Additional notes or comments (optional)
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Duration taken to complete the test in minutes
        /// </summary>
        public int? DurationMinutes { get; set; }
    }
}