namespace Dignus.Candidate.Back.DTOs
{
    /// <summary>
    /// DTO for interview questions
    /// </summary>
    public class InterviewQuestionDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = null!;
        public int Order { get; set; }
        public int MaxDurationSeconds { get; set; }
        public string? Category { get; set; }
        public bool IsRequired { get; set; } = true;
    }

    /// <summary>
    /// DTO for interview configuration
    /// </summary>
    public class InterviewConfigDto
    {
        public int MaxTotalDurationMinutes { get; set; }
        public int MaxQuestionDurationMinutes { get; set; }
        public string MinVideoQuality { get; set; } = null!;
        public int RequiredQuestions { get; set; }
        public List<string> Instructions { get; set; } = new();
        public Dictionary<string, object>? Settings { get; set; }
    }

    /// <summary>
    /// DTO for video interview submission
    /// </summary>
    public class VideoInterviewSubmissionDto
    {
        public Guid CandidateId { get; set; }
        public List<Guid> QuestionIds { get; set; } = new();
        public string VideoUrl { get; set; } = null!;
        public int DurationSeconds { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }
}