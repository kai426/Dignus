using System.ComponentModel.DataAnnotations;
using Dignus.Data.Models;

namespace Dignus.Candidate.Back.DTOs
{
    /// <summary>
    /// Data Transfer Object for Candidate information
    /// </summary>
    public class CandidateDto
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Name { get; set; } = null!;
        
        [Required]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF must contain 11 digits")]
        public string Cpf { get; set; } = null!;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        
        [Phone]
        public string? Phone { get; set; }
        
        [Required]
        public DateTime BirthDate { get; set; }
        
        public CandidateStatus Status { get; set; }
        
        public DateTimeOffset CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating a new candidate
    /// </summary>
    public class CreateCandidateDto
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Name { get; set; } = null!;
        
        [Required]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF must contain 11 digits")]
        public string Cpf { get; set; } = null!;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        
        [Phone]
        public string? Phone { get; set; }
        
        [Required]
        public DateTime BirthDate { get; set; }
    }

    /// <summary>
    /// DTO for updating candidate information
    /// </summary>
    public class UpdateCandidateDto
    {
        [StringLength(200, MinimumLength = 2)]
        public string? Name { get; set; }
        
        [Phone]
        public string? Phone { get; set; }
        
        public CandidateStatus? Status { get; set; }
    }
}