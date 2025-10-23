namespace Dignus.Candidate.Back.Configuration
{
    /// <summary>
    /// Configuration settings for video upload functionality
    /// </summary>
    public class MediaUploadSettings
    {
        /// <summary>
        /// Maximum file size in MB for video uploads
        /// </summary>
        public int MaxFileSizeMB { get; set; }

        /// <summary>
        /// Allowed video file extensions
        /// </summary>
        public string[] AllowedExtensions { get; set; } = null!;

        /// <summary>
        /// Allowed video MIME types
        /// </summary>
        public string[] AllowedMimeTypes { get; set; } = null!;
    }
}