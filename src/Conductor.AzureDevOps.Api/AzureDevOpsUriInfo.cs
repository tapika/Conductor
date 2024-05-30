using System;

namespace Conductor.AzureDevOps.Api
{
    public class AzureDevOpsUriInfo
    {
        public string Organization { get; set; }
        public string ProjectName { get; set; }
        public string RepositoryId { get; set; }
        public Uri RepositoryUrl { get; set; }

        public AzureDevOpsUriInfo(string url)
        {
            RepositoryUrl = new Uri(url);
            var parts = RepositoryUrl.LocalPath.Split("/", StringSplitOptions.RemoveEmptyEntries);
            Organization = parts[0];
            ProjectName = parts[1];
            RepositoryId = parts[3];
        }

        public string GetPullRequestUrl(int pullRequestId)
        { 
            string url = $"https://dev.azure.com/{Uri.EscapeDataString(Organization)}/{Uri.EscapeDataString(ProjectName)}/_git/{Uri.EscapeDataString(RepositoryId)}/pullrequest/{pullRequestId}";
            return url;
        }

        public string GetBuildUrl(int buildId)
        {
            string url = $"https://dev.azure.com/{Uri.EscapeDataString(Organization)}/{Uri.EscapeDataString(ProjectName)}/_build/results?buildId={buildId}&view=results";
            return url;
        }
    }
}
