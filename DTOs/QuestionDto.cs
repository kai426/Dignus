using System.ComponentModel.DataAnnotations;

namespace Dignus.Candidate.Back.DTOs
{
    /// <summary>
    /// DTO for Question information - Updated to match frontend expectations
    /// </summary>
    public class QuestionDto
    {
        public string Id { get; set; } = null!;
        
        [Required]
        public string Prompt { get; set; } = null!; // Changed from "Text" to match frontend
        
        [Required]
        public List<QuestionOptionDto> Options { get; set; } = new(); // Changed from OptionsJson to structured options
        
        public string Type { get; set; } = "single"; // "single", "multi", "text" - matches frontend
        
        public int? MaxSelections { get; set; } // For multi-select questions
        
        public string? Placeholder { get; set; } // For text input questions
        
        public string? CorrectAnswer { get; set; }
        
        public int Order { get; set; }
        
        public DateTimeOffset CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for Question Options - matches frontend Option type
    /// </summary>
    public class QuestionOptionDto
    {
        public string Id { get; set; } = null!;
        public string Label { get; set; } = null!;
    }

    /// <summary>
    /// DTO for Question Response
    /// </summary>
    public class QuestionResponseDto
    {
        public Guid Id { get; set; }
        
        [Required]
        public string QuestionId { get; set; } = null!;
        
        [Required]
        public string SelectedAnswers { get; set; } = null!;
        
        public DateTimeOffset AnsweredAt { get; set; }
        
        public Guid CandidateId { get; set; }
        
        public QuestionDto? Question { get; set; }
    }

    /// <summary>
    /// DTO for submitting an answer to a question
    /// </summary>
    public class SubmitAnswerDto
    {
        [Required]
        public string QuestionId { get; set; } = null!;
        
        [Required]
        public string SelectedAnswers { get; set; } = null!;
    }
}