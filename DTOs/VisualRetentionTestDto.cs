namespace Dignus.Candidate.Back.DTOs
{
    /// <summary>
    /// DTO for visual retention test
    /// </summary>
    public class VisualRetentionTestDto : BaseTestDto
    {
        /// <summary>
        /// Number of correct visual pattern matches
        /// </summary>
        public int? CorrectMatches { get; set; }
        
        /// <summary>
        /// Total number of patterns presented
        /// </summary>
        public int? TotalPatterns { get; set; }
        
        /// <summary>
        /// Average response time in milliseconds
        /// </summary>
        public double? AverageResponseTimeMs { get; set; }
        
        /// <summary>
        /// Difficulty level completed (1-5)
        /// </summary>
        public int? DifficultyLevel { get; set; }
        
        /// <summary>
        /// Questions included in this visual retention test
        /// </summary>
        public List<QuestionResponseDto> Questions { get; set; } = new();
    }
}