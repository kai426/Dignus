using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Dignus.Candidate.Back.DTOs
{
    /// <summary>
    /// DTO for Gupy candidate data from Databricks
    /// </summary>
    public class GupyCandidateDto
    {
        /// <summary>
        /// Gupy candidate ID
        /// </summary>
        [Required]
        public string GupyId { get; set; } = string.Empty;

        /// <summary>
        /// Candidate full name
        /// </summary>
        [Required]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Candidate email address
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Candidate CPF (Brazilian tax ID)
        /// </summary>
        public string? Cpf { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Date of birth
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Application status in Gupy
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Job ID that candidate applied for
        /// </summary>
        public string JobId { get; set; } = string.Empty;

        /// <summary>
        /// Job title
        /// </summary>
        public string JobTitle { get; set; } = string.Empty;

        /// <summary>
        /// Application date
        /// </summary>
        public DateTimeOffset? ApplicationDate { get; set; }

        /// <summary>
        /// Last update date from Gupy
        /// </summary>
        public DateTimeOffset? LastUpdated { get; set; }

        /// <summary>
        /// Resume/CV URL
        /// </summary>
        public string? ResumeUrl { get; set; }

        /// <summary>
        /// Source of application (e.g., website, referral)
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Additional metadata from Gupy
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();

        /// <summary>
        /// Custom fields from Gupy form
        /// </summary>
        public Dictionary<string, string> CustomFields { get; set; } = new();
    }

    /// <summary>
    /// DTO for Gupy job listings from Databricks
    /// </summary>
    public class GupyJobDto
    {
        /// <summary>
        /// Gupy job ID
        /// </summary>
        [Required]
        public string GupyJobId { get; set; } = string.Empty;

        /// <summary>
        /// Job title
        /// </summary>
        [Required]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Job description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Department or area
        /// </summary>
        public string? Department { get; set; }

        /// <summary>
        /// Job location
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Employment type (full-time, part-time, etc.)
        /// </summary>
        public string? EmploymentType { get; set; }

        /// <summary>
        /// Experience level required
        /// </summary>
        public string? ExperienceLevel { get; set; }

        /// <summary>
        /// Job status (active, inactive, etc.)
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Date when job was created
        /// </summary>
        public DateTimeOffset? CreatedAt { get; set; }

        /// <summary>
        /// Date when job was last updated
        /// </summary>
        public DateTimeOffset? UpdatedAt { get; set; }

        /// <summary>
        /// Number of applications received
        /// </summary>
        public int ApplicationCount { get; set; }

        /// <summary>
        /// Required skills for the job
        /// </summary>
        public List<string> RequiredSkills { get; set; } = new();

        /// <summary>
        /// Preferred skills for the job
        /// </summary>
        public List<string> PreferredSkills { get; set; } = new();

        /// <summary>
        /// Salary range if available
        /// </summary>
        public GupySalaryRangeDto? SalaryRange { get; set; }
    }

    /// <summary>
    /// DTO for salary range information
    /// </summary>
    public class GupySalaryRangeDto
    {
        /// <summary>
        /// Minimum salary
        /// </summary>
        public decimal? MinSalary { get; set; }

        /// <summary>
        /// Maximum salary
        /// </summary>
        public decimal? MaxSalary { get; set; }

        /// <summary>
        /// Currency code (e.g., BRL, USD)
        /// </summary>
        public string Currency { get; set; } = "BRL";

        /// <summary>
        /// Salary period (monthly, yearly)
        /// </summary>
        public string Period { get; set; } = "monthly";
    }

    /// <summary>
    /// DTO for synchronization status
    /// </summary>
    public class SyncStatusDto
    {
        /// <summary>
        /// Synchronization operation ID
        /// </summary>
        public Guid SyncId { get; set; }

        /// <summary>
        /// Type of synchronization (candidates, jobs, etc.)
        /// </summary>
        public SyncType Type { get; set; }

        /// <summary>
        /// Current status of the sync operation
        /// </summary>
        public SyncStatus Status { get; set; }

        /// <summary>
        /// Start time of the sync operation
        /// </summary>
        public DateTimeOffset StartedAt { get; set; }

        /// <summary>
        /// Completion time of the sync operation
        /// </summary>
        public DateTimeOffset? CompletedAt { get; set; }

        /// <summary>
        /// Total records to process
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Records successfully processed
        /// </summary>
        public int ProcessedRecords { get; set; }

        /// <summary>
        /// Records that failed processing
        /// </summary>
        public int FailedRecords { get; set; }

        /// <summary>
        /// Error message if sync failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Duration of the sync operation
        /// </summary>
        public TimeSpan Duration =>
            CompletedAt.HasValue ? CompletedAt.Value - StartedAt : DateTimeOffset.UtcNow - StartedAt;

        /// <summary>
        /// Progress percentage (0-100)
        /// </summary>
        public decimal ProgressPercentage =>
            TotalRecords > 0 ? Math.Round((decimal)ProcessedRecords / TotalRecords * 100, 2) : 0;

        /// <summary>
        /// Details about sync results
        /// </summary>
        public List<SyncResultDetailDto> Details { get; set; } = new();
    }

    /// <summary>
    /// DTO for detailed sync results
    /// </summary>
    public class SyncResultDetailDto
    {
        /// <summary>
        /// Record identifier
        /// </summary>
        public string RecordId { get; set; } = string.Empty;

        /// <summary>
        /// Action performed (created, updated, skipped, failed)
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Result status
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Error message if failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Processing timestamp
        /// </summary>
        public DateTimeOffset ProcessedAt { get; set; }
    }

    /// <summary>
    /// DTO for Databricks job execution request
    /// </summary>
    public class DatabricksJobRequestDto
    {
        /// <summary>
        /// Job type (sync candidates, sync jobs, etc.)
        /// </summary>
        [Required]
        public string JobType { get; set; } = string.Empty;

        /// <summary>
        /// Parameters for the job execution
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; } = new();

        /// <summary>
        /// Cluster ID to run the job on
        /// </summary>
        public string? ClusterId { get; set; }

        /// <summary>
        /// Notebook path to execute
        /// </summary>
        public string? NotebookPath { get; set; }

        /// <summary>
        /// Maximum execution time in minutes
        /// </summary>
        public int TimeoutMinutes { get; set; } = 30;

        /// <summary>
        /// Priority of the job (1-10, 10 being highest)
        /// </summary>
        public int Priority { get; set; } = 5;
    }

    /// <summary>
    /// DTO for Databricks job execution response
    /// </summary>
    public class DatabricksJobResponseDto
    {
        /// <summary>
        /// Databricks run ID
        /// </summary>
        public long RunId { get; set; }

        /// <summary>
        /// Job execution status
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Job result state
        /// </summary>
        public string? ResultState { get; set; }

        /// <summary>
        /// Job execution URL
        /// </summary>
        public string? RunPageUrl { get; set; }

        /// <summary>
        /// Start time of job execution
        /// </summary>
        public DateTimeOffset? StartTime { get; set; }

        /// <summary>
        /// End time of job execution
        /// </summary>
        public DateTimeOffset? EndTime { get; set; }

        /// <summary>
        /// Job output/results
        /// </summary>
        public Dictionary<string, object> Output { get; set; } = new();

        /// <summary>
        /// Error message if job failed
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// DTO for field mapping between systems
    /// </summary>
    public class FieldMappingDto
    {
        /// <summary>
        /// Source field name (from Gupy)
        /// </summary>
        public string SourceField { get; set; } = string.Empty;

        /// <summary>
        /// Target field name (in Dignus system)
        /// </summary>
        public string TargetField { get; set; } = string.Empty;

        /// <summary>
        /// Data transformation rule
        /// </summary>
        public string? TransformationRule { get; set; }

        /// <summary>
        /// Default value if source is null/empty
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// Whether this field is required
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Data type of the field
        /// </summary>
        public string DataType { get; set; } = "string";
    }

    #region Enums

    /// <summary>
    /// Types of synchronization operations
    /// </summary>
    public enum SyncType
    {
        Candidates,
        Jobs,
        Applications,
        Full
    }

    /// <summary>
    /// Status of synchronization operations
    /// </summary>
    public enum SyncStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Cancelled,
        PartiallyCompleted
    }

    /// <summary>
    /// Databricks job execution states
    /// </summary>
    public enum DatabricksJobState
    {
        Queued,
        Running,
        Terminating,
        Terminated,
        Skipped,
        InternalError
    }

    #endregion

    /// <summary>
    /// Basic job DTO for mapping from Gupy jobs
    /// </summary>
    public class JobDto
    {
        /// <summary>
        /// Job identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Job title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Job description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Department
        /// </summary>
        public string Department { get; set; } = string.Empty;

        /// <summary>
        /// Job location
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Whether the job is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// When the job was created
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }
    }
}