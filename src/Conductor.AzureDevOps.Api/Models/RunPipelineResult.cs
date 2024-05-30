using System.Collections.Generic;
using Newtonsoft.Json;

namespace Conductor.AzureDevOps.Api.Models
{
	public class RunPipelineResult
	{
		[JsonProperty("_links")]
		public ReferenceLinks Links { get; set; }
		public string CreatedDate { get; set; }
		public string FinalYaml { get; set; }
		public string FinishedDate { get; set; }
		public int Id { get; set; }
		public string Name { get; set; }
		public PipelineReference Pipeline { get; set; }
		//resources
		public string Result { get; set; }
		public string State { get; set; }
		public string Url { get; set; }
		public IEnumerable<string> Variables { get; set; }
	}
}
