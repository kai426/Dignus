using System.ComponentModel.DataAnnotations;
using Dignus.Data.Models;

namespace Dignus.Candidate.Back.DTOs
{
    /// <summary>
    /// Base DTO for all test types
    /// </summary>
    public abstract class BaseTestDto
    {
        public Guid Id { get; set; }
        public decimal? Score { get; set; }
        public TestStatus Status { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public Guid CandidateId { get; set; }
    }

    /// <summary>
    /// DTO for Math Test
    /// </summary>
    public class MathTestDto : BaseTestDto
    {
        public List<QuestionResponseDto> Questions { get; set; } = new();
    }

    /// <summary>
    /// DTO for Portuguese Test
    /// </summary>
    public class PortugueseTestDto : BaseTestDto
    {
        public List<QuestionResponseDto> Questions { get; set; } = new();
        // AudioSubmission removed - use unified TestVideoResponse instead
    }

    /// <summary>
    /// DTO for Psychology Test
    /// </summary>
    public class PsychologyTestDto : BaseTestDto
    {
        public List<QuestionResponseDto> Questions { get; set; } = new();
        // VideoSubmission removed - use unified TestVideoResponse instead
    }

    /// <summary>
    /// DTO for creating a test
    /// </summary>
    public class CreateTestDto
    {
        [Required]
        public Guid CandidateId { get; set; }
        
        [Required]
        public string TestType { get; set; } = null!; // "Math", "Portuguese", "Psychology"
    }

    /// <summary>
    /// DTO for starting a test
    /// </summary>
    public class StartTestDto
    {
        [Required]
        public Guid TestId { get; set; }
    }

    /// <summary>
    /// DTO for submitting test answers
    /// </summary>
    public class SubmitTestDto
    {
        [Required]
        public Guid TestId { get; set; }
        
        [Required]
        public Guid CandidateId { get; set; }
        
        [Required]
        public List<QuestionResponseDto> Answers { get; set; } = new();
    }
}