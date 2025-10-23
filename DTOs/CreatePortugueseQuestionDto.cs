using System.ComponentModel.DataAnnotations;

namespace Dignus.Candidate.Back.DTOs
{
    /// <summary>
    /// DTO for creating a new Portuguese question
    /// </summary>
    public class CreatePortugueseQuestionDto
    {
        /// <summary>
        /// Question text
        /// </summary>
        [Required(ErrorMessage = "Question text is required")]
        [StringLength(1000, ErrorMessage = "Question text cannot exceed 1000 characters")]
        public string QuestionText { get; set; } = string.Empty;

        /// <summary>
        /// First answer option
        /// </summary>
        [Required(ErrorMessage = "Option A is required")]
        [StringLength(500, ErrorMessage = "Option A cannot exceed 500 characters")]
        public string OptionA { get; set; } = string.Empty;

        /// <summary>
        /// Second answer option
        /// </summary>
        [Required(ErrorMessage = "Option B is required")]
        [StringLength(500, ErrorMessage = "Option B cannot exceed 500 characters")]
        public string OptionB { get; set; } = string.Empty;

        /// <summary>
        /// Third answer option
        /// </summary>
        [Required(ErrorMessage = "Option C is required")]
        [StringLength(500, ErrorMessage = "Option C cannot exceed 500 characters")]
        public string OptionC { get; set; } = string.Empty;

        /// <summary>
        /// Fourth answer option
        /// </summary>
        [Required(ErrorMessage = "Option D is required")]
        [StringLength(500, ErrorMessage = "Option D cannot exceed 500 characters")]
        public string OptionD { get; set; } = string.Empty;

        /// <summary>
        /// Correct answer (A, B, C, or D)
        /// </summary>
        [Required(ErrorMessage = "Correct answer is required")]
        [RegularExpression("^[ABCD]$", ErrorMessage = "Correct answer must be A, B, C, or D")]
        public string CorrectAnswer { get; set; } = string.Empty;

        /// <summary>
        /// Difficulty level (1-5)
        /// </summary>
        [Range(1, 5, ErrorMessage = "Difficulty must be between 1 and 5")]
        public int Difficulty { get; set; } = 1;

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