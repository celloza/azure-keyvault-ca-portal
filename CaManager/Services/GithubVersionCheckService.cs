using System.Net.Http.Json;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;

namespace CaManager.Services
{
    /// <summary>
    /// Implementation of <see cref="IVersionCheckService"/> that checks GitHub Releases.
    /// </summary>
    public class GithubVersionCheckService : IVersionCheckService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<GithubVersionCheckService> _logger;
        private const string CacheKey = "GithubVersionCheck";
        // Cache for 4 hours to respect GitHub rate limits
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(4);

        /// <summary>
        /// Initializes a new instance of the <see cref="GithubVersionCheckService"/> class.
        /// </summary>
        public GithubVersionCheckService(HttpClient httpClient, IMemoryCache cache, ILogger<GithubVersionCheckService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            // GitHub API requires a User-Agent
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AzureKeyVaultCaManager");
        }

        /// <inheritdoc/>
        public async Task<VersionCheckResult> CheckForUpdateAsync()
        {
            if (_cache.TryGetValue(CacheKey, out VersionCheckResult? cachedResult) && cachedResult != null)
            {
                return cachedResult;
            }

            var result = await CheckGithubAsync();
            
            // Cache the result regardless of success/fail to prevent spamming on error
            _cache.Set(CacheKey, result, CacheDuration);

            return result;
        }

        private async Task<VersionCheckResult> CheckGithubAsync()
        {
            var currentVersionStr = GetCurrentVersion();
            var result = new VersionCheckResult { CurrentVersion = currentVersionStr };

            try
            {
                var release = await _httpClient.GetFromJsonAsync<GithubRelease>("https://api.github.com/repos/celloza/azure-keyvault-ca-portal/releases/latest");
                
                if (release != null && !string.IsNullOrEmpty(release.TagName))
                {
                    result.LatestVersion = release.TagName;
                    result.ReleaseUrl = release.HtmlUrl;

                    if (Version.TryParse(CleanVersion(currentVersionStr), out var current) && 
                        Version.TryParse(CleanVersion(release.TagName), out var latest))
                    {
                        result.IsUpdateAvailable = latest > current;
                    }
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // No releases found or repo is private - not an error worth logging as 'Error'
                _logger.LogInformation("No releases found for celloza/azure-keyvault-ca-portal (404).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check for updates from GitHub.");
            }

            return result;
        }

        /// <summary>
        /// Gets the current application version.
        /// Virtual for testing purposes.
        /// </summary>
        /// <returns>The semantic version string.</returns>
        protected virtual string GetCurrentVersion()
        {
            var version = Assembly.GetEntryAssembly()
                ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;
            
            // If built with -p:Version=1.0.0, InformationalVersion might include commit hash (e.g. 1.0.0+sha)
            // We just want the semantic version part.
            return version?.Split('+')[0] ?? "0.0.0";
        }

        private string CleanVersion(string version)
        {
            if (string.IsNullOrEmpty(version)) return "0.0.0";
            return version.TrimStart('v', 'V').Split('-')[0]; // Handle v1.0.0 or 1.0.0-beta
        }

        private class GithubRelease
        {
            [System.Text.Json.Serialization.JsonPropertyName("tag_name")]
            public string TagName { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("html_url")]
            public string HtmlUrl { get; set; } = string.Empty;
        }
    }
}
