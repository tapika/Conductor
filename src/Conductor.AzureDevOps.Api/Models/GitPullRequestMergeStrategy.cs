namespace Conductor.AzureDevOps.Api.Models
{
	/// <summary>
	/// See https://docs.microsoft.com/en-us/javascript/api/azure-devops-extension-api/gitpullrequestmergestrategy
	/// </summary>
	public enum GitPullRequestMergeStrategy
	{
		NoFastForward = 1,
		Squash = 2,
		Rebase = 3,
		RebaseMerge = 4
	}
}
