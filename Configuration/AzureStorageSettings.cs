namespace Dignus.Candidate.Back.Configuration
{
    /// <summary>
    /// Configuration settings for Azure Storage
    /// </summary>
    public class AzureStorageSettings
    {
        /// <summary>
        /// Azure Storage connection string
        /// </summary>
        public string ConnectionString { get; set; } = null!;
        
        /// <summary>
        /// Container names for different media types
        /// </summary>
        public ContainerNames ContainerNames { get; set; } = null!;
    }
    
    /// <summary>
    /// Azure Storage container names configuration
    /// </summary>
    public class ContainerNames
    {
        /// <summary>
        /// Container name for audio submissions
        /// </summary>
        public string Audio { get; set; } = null!;
        
        /// <summary>
        /// Container name for video interviews
        /// </summary>
        public string Video { get; set; } = null!;
    }
}