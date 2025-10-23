using System.ComponentModel.DataAnnotations;
using Dignus.Data.Models;

namespace Dignus.Candidate.Back.DTOs
{
    /// <summary>
    /// DTO for Evaluation information
    /// </summary>
    public class EvaluationDto
    {
        public Guid Id { get; set; }
        
        public int? PortugueseTestFeedback { get; set; }
        
        public string? MathTestFeedback { get; set; }
        
        public string? VideoInterviewFeedback { get; set; }
        
        public string? PsychologyTestFeedback { get; set; }
        
        public DateTimeOffset EvaluatedAt { get; set; }
        
        public Guid CandidateId { get; set; }
    }

    /// <summary>
    /// DTO for creating an evaluation
    /// </summary>
    public class CreateEvaluationDto
    {
        [Required]
        public Guid CandidateId { get; set; }
        
        public int? PortugueseTestFeedback { get; set; }
        
        public string? MathTestFeedback { get; set; }
        
        public string? VideoInterviewFeedback { get; set; }
        
        public string? PsychologyTestFeedback { get; set; }
    }

    /// <summary>
    /// DTO for updating evaluation
    /// </summary>
    public class UpdateEvaluationDto
    {
        public int? PortugueseTestFeedback { get; set; }
        
        public string? MathTestFeedback { get; set; }
        
        public string? VideoInterviewFeedback { get; set; }
        
        public string? PsychologyTestFeedback { get; set; }
    }
}