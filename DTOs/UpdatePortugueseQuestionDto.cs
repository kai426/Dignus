using System.ComponentModel.DataAnnotations;

namespace Dignus.Candidate.Back.DTOs
{
    /// <summary>
    /// DTO for updating an existing Portuguese question
    /// </summary>
    public class UpdatePortugueseQuestionDto
    {
        /// <summary>
        /// Question text
        /// </summary>
        [StringLength(1000, ErrorMessage = "Question text cannot exceed 1000 characters")]
        public string? QuestionText { get; set; }

        /// <summary>
        /// First answer option
        /// </summary>
        [StringLength(500, ErrorMessage = "Option A cannot exceed 500 characters")]
        public string? OptionA { get; set; }

        /// <summary>
        /// Second answer option
        /// </summary>
        [StringLength(500, ErrorMessage = "Option B cannot exceed 500 characters")]
        public string? OptionB { get; set; }

        /// <summary>
        /// Third answer option
        /// </summary>
        [StringLength(500, ErrorMessage = "Option C cannot exceed 500 characters")]
        public string? OptionC { get; set; }

        /// <summary>
        /// Fourth answer option
        /// </summary>
        [StringLength(500, ErrorMessage = "Option D cannot exceed 500 characters")]
        public string? OptionD { get; set; }

        /// <summary>
        /// Correct answer (A, B, C, or D)
        /// </summary>
        [RegularExpression("^[ABCD]$", ErrorMessage = "Correct answer must be A, B, C, or D")]
        public string? CorrectAnswer { get; set; }

        /// <summary>
        /// Difficulty level (1-5)
        /// </summary>
        [Range(1, 5, ErrorMessage = "Difficulty must be between 1 and 5")]
        public int? Difficulty { get; set; }

        /// <summary>
        /// Question category/topic
        /// </summary>
        [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
        public string? Category { get; set; }

        /// <summary>
        /// Additional explanation or notes
        /// </summary>
        [StringLength(500, ErrorMessage = "Explanation cannot exceed 500 characters")]
        public string? Explanation { get; set; }
    }
}