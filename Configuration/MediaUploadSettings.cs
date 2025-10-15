namespace Dignus.Candidate.Back.Configuration
{
    /// <summary>
    /// Configuration settings for media upload functionality
    /// </summary>
    public class MediaUploadSettings
    {
        /// <summary>
        /// Maximum file sizes in MB
        /// </summary>
        public MaxFileSizes MaxFileSizes { get; set; } = null!;
        
        /// <summary>
        /// Allowed file extensions
        /// </summary>
        public AllowedExtensions AllowedExtensions { get; set; } = null!;
        
        /// <summary>
        /// Allowed MIME types
        /// </summary>
        public AllowedMimeTypes AllowedMimeTypes { get; set; } = null!;
    }
    
    /// <summary>
    /// Maximum file sizes configuration
    /// </summary>
    public class MaxFileSizes
    {
        /// <summary>
        /// Maximum audio file size in MB
        /// </summary>
        public int AudioMB { get; set; }
        
        /// <summary>
        /// Maximum video file size in MB
        /// </summary>
        public int VideoMB { get; set; }
    }
    
    /// <summary>
    /// Allowed file extensions configuration
    /// </summary>
    public class AllowedExtensions
    {
        /// <summary>
        /// Allowed audio file extensions
        /// </summary>
        public string[] Audio { get; set; } = null!;
        
        /// <summary>
        /// Allowed video file extensions
        /// </summary>
        public string[] Video { get; set; } = null!;
    }
    
    /// <summary>
    /// Allowed MIME types configuration
    /// </summary>
    public class AllowedMimeTypes
    {
        /// <summary>
        /// Allowed audio MIME types
        /// </summary>
        public string[] Audio { get; set; } = null!;
        
        /// <summary>
        /// Allowed video MIME types
        /// </summary>
        public string[] Video { get; set; } = null!;
    }
}