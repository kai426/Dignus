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
        /// Base container name for test videos (optional - defaults to dynamic naming per test)
        /// </summary>
        public string? ContainerName { get; set; }
    }
}