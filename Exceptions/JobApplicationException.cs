namespace Dignus.Candidate.Back.Exceptions
{
    /// <summary>
    /// Exception thrown when job application fails for specific business reasons
    /// </summary>
    public class JobApplicationException : Exception
    {
        public string ErrorCode { get; }
        public string UserFriendlyMessage { get; }

        public JobApplicationException(string errorCode, string userFriendlyMessage, string? technicalMessage = null)
            : base(technicalMessage ?? userFriendlyMessage)
        {
            ErrorCode = errorCode;
            UserFriendlyMessage = userFriendlyMessage;
        }

        public static class ErrorCodes
        {
            public const string JobNotFound = "JOB_NOT_FOUND";
            public const string JobExpired = "JOB_EXPIRED";
            public const string JobNotOpen = "JOB_NOT_OPEN";
            public const string InvalidJobId = "INVALID_JOB_ID";
            public const string AlreadyApplied = "ALREADY_APPLIED";
            public const string CandidateNotFound = "CANDIDATE_NOT_FOUND";
        }
    }
}