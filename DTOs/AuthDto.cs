using System.ComponentModel.DataAnnotations;

namespace Dignus.Candidate.Back.DTOs
{
    /// <summary>
    /// DTO for candidate login request
    /// </summary>
    public class LoginDto
    {
        [Required]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF must contain 11 digits")]
        public string Cpf { get; set; } = null!;
    }

    /// <summary>
    /// DTO for login response
    /// </summary>
    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;
        public CandidateDto Candidate { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// DTO for candidate progress tracking
    /// </summary>
    public class ProgressDto
    {
        public Guid CandidateId { get; set; }
        public decimal CompletionPercentage { get; set; }
        public int CompletedTests { get; set; }
        public int TotalTests { get; set; }
        public Dictionary<string, TestProgressDto> TestProgress { get; set; } = new();
    }

    /// <summary>
    /// DTO for individual test progress
    /// </summary>
    public class TestProgressDto
    {
        public string TestType { get; set; } = null!;
        public string Status { get; set; } = null!; // "NotStarted", "InProgress", "Completed"
        public bool IsCompleted { get; set; }
        public decimal? Score { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// DTO for candidate's job information
    /// </summary>
    public class CandidateJobDto
    {
        public Guid CandidateId { get; set; }
        public JobDto Job { get; set; } = null!;
        public DateTime AppliedAt { get; set; }
    }
}