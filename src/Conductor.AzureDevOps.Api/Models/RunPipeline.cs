using System.Collections.Generic;
using Newtonsoft.Json;

namespace Conductor.AzureDevOps.Api.Models
{
	public class RunPipeline
	{
		public bool PreviewRun { get; set; }
		public RunResourcesParameters Resources { get; set; }
		public IEnumerable<string> StagesToSkip { get; set; }
		public object TemplateParameters { get; set; }
		public IEnumerable<string> Variables { get; set; }
		public string YamlOverride { get; set; }
	}
}
