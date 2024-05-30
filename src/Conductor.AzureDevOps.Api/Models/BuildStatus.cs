namespace Conductor.AzureDevOps.Api.Models
{
	public enum BuildStatus
	{
		None = 0,
		InProgress = 1,
		Completed = 2,
		Cancelling = 4,
		Postponed = 8,
		NotStarted = 32,
		All = 47
	}
}
