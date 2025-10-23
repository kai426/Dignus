using System.ComponentModel.DataAnnotations;

namespace Dignus.Candidate.Back.DTOs
{
    /// <summary>
    /// DTO for Job listing display (for job search and listing pages)
    /// </summary>
    public class JobListingDto
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public List<JobTagDto> Tags { get; set; } = new();
        public string PublishedAt { get; set; } = null!;
        public string ExpiresAt { get; set; } = null!;
        public string Owner { get; set; } = null!;
        public string? Description { get; set; }
        public List<string> Requirements { get; set; } = new();
        public string? Location { get; set; }
        public string? Company { get; set; }
        public string Status { get; set; } = null!;
    }

    /// <summary>
    /// DTO for Job tags with tone styling
    /// </summary>
    public class JobTagDto
    {
        public string Label { get; set; } = null!;
        public string Tone { get; set; } = "neutral"; // "brand", "success", "neutral"
    }

    /// <summary>
    /// DTO for Job search and filter parameters
    /// </summary>
    public class JobSearchRequest
    {
        public string? SearchQuery { get; set; }
        public string? Company { get; set; }
        public string? Status { get; set; }
        public string? Location { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "publishedAt"; // "publishedAt", "title"
        public string SortDirection { get; set; } = "desc"; // "asc", "desc"
    }

    /// <summary>
    /// DTO for Job statistics on dashboard
    /// </summary>
    public class JobStatisticsDto
    {
        public int TotalOpenJobs { get; set; }
        public int TotalApplicationsReceived { get; set; }
        public int JobsAboutToExpire { get; set; }
        public int FrozenJobs { get; set; }
    }

    /// <summary>
    /// DTO for applying to a job
    /// </summary>
    public class ApplyToJobDto
    {
        [Required]
        public Guid CandidateId { get; set; }
        
        public string? CoverLetter { get; set; }
        public List<string> AdditionalDocuments { get; set; } = new();
    }

    /// <summary>
    /// DTO for paginated results
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
    }

    /// <summary>
    /// DTO for pagination information
    /// </summary>
    public class PaginationInfo
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}