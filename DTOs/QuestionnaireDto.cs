using System.ComponentModel.DataAnnotations;

namespace Dignus.Candidate.Back.DTOs
{
    /// <summary>
    /// DTO for Questionnaire system - matches frontend Section structure
    /// </summary>
    public class QuestionnaireDto
    {
        public List<QuestionnaireSectionDto> Sections { get; set; } = new();
        public int CurrentSection { get; set; } = 0;
        public Dictionary<string, object> Responses { get; set; } = new();
        public Guid CandidateId { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
    }

    /// <summary>
    /// DTO for Questionnaire Section - matches frontend Section type
    /// </summary>
    public class QuestionnaireSectionDto
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public List<QuestionnaireQuestionDto> Questions { get; set; } = new();
        public int Order { get; set; }
        public bool IsCompleted { get; set; }
    }

    /// <summary>
    /// DTO for Questionnaire Question - matches frontend Question type  
    /// </summary>
    public class QuestionnaireQuestionDto
    {
        public string Id { get; set; } = null!;
        public string Prompt { get; set; } = null!;
        public List<QuestionnaireOptionDto> Options { get; set; } = new();
        public string Type { get; set; } = "single"; // "single", "multi", "text"
        public int? MaxSelections { get; set; }
        public string? Placeholder { get; set; }
        public bool IsRequired { get; set; } = true;
    }

    /// <summary>
    /// DTO for Questionnaire Option - matches frontend Option type
    /// </summary>
    public class QuestionnaireOptionDto
    {
        public string Id { get; set; } = null!;
        public string Label { get; set; } = null!;
    }

    /// <summary>
    /// DTO for saving section responses
    /// </summary>
    public class SaveSectionResponseDto
    {
        [Required]
        public Guid CandidateId { get; set; }
        
        [Required]
        public int SectionId { get; set; }
        
        [Required]
        public Dictionary<string, object> Responses { get; set; } = new();
    }

    /// <summary>
    /// DTO for questionnaire progress tracking
    /// </summary>
    public class QuestionnaireProgressDto
    {
        public Guid CandidateId { get; set; }
        public int TotalSections { get; set; }
        public int CompletedSections { get; set; }
        public int CurrentSection { get; set; }
        public int ProgressPercentage { get; set; }
        public Dictionary<string, object> AllResponses { get; set; } = new();
        public bool IsCompleted { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? LastSavedAt { get; set; }
    }

    /// <summary>
    /// DTO for submitting completed questionnaire
    /// </summary>
    public class SubmitQuestionnaireDto
    {
        [Required]
        public Guid CandidateId { get; set; }
        
        [Required]
        public Dictionary<string, object> Responses { get; set; } = new();
        
        public DateTimeOffset CompletedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}