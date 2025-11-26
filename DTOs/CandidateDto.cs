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

        public bool? IsPCD { get; set; }

        public string? PCDDocumentUrl { get; set; }

        public string? PCDDocumentFileName { get; set; }

        public DateTimeOffset? PCDDocumentUploadedAt { get; set; }

        public bool? IsForeigner { get; set; }

        [StringLength(100)]
        public string? CountryOfOrigin { get; set; }
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

        public bool? IsPCD { get; set; }

        public bool? IsForeigner { get; set; }

        [StringLength(100)]
        public string? CountryOfOrigin { get; set; }
    }
}