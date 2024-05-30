namespace Conductor.AzureDevOps.Api.Models
{
	public class PipelineReference
	{
		public string Folder { get; set; }
		public int Id { get; set; }
		public string Name { get; set; }
		public int Revision { get; set; }
		public string Url { get; set; }
	}
}
