namespace CaManager.Services
{
    /// <summary>
    /// Service for checking for application updates.
    /// </summary>
    public interface IVersionCheckService
    {
        /// <summary>
        /// Checks for available updates.
        /// </summary>
        Task<VersionCheckResult> CheckForUpdateAsync();
    }

    /// <summary>
    /// Result of a version check operation.
    /// </summary>
    public class VersionCheckResult
    {
        /// <summary>
        /// Indicates if a newer version is available.
        /// </summary>
        public bool IsUpdateAvailable { get; set; }

        /// <summary>
        /// The current running version of the application.
        /// </summary>
        public string CurrentVersion { get; set; } = string.Empty;

        /// <summary>
        /// The latest available version.
        /// </summary>
        public string LatestVersion { get; set; } = string.Empty;

        /// <summary>
        /// The URL to the release notes or download page.
        /// </summary>
        public string ReleaseUrl { get; set; } = string.Empty;
    }
}
