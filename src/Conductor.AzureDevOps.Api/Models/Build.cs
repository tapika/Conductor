using Newtonsoft.Json;
using System.Collections.Generic;

namespace Conductor.AzureDevOps.Api.Models
{
    public class Build
	{
		[JsonProperty("_links")]
		public ReferenceLinks Links { get; set; }
		public string AgentSpecification { get; set; }
		public string BuildNumber { get; set; }
		public int BuildNumberRevision { get; set; }
		public object Controller { get; set; }
		public object Definition { get; set; }
		public bool Deleted { get; set; }
		public IdentityRef DeletedBy { get; set; }
		public string DeletedDate { get; set; }
		public string DeletedReason { get; set; }
		public object Demands { get; set; }
		public string FinishTime { get; set; }
		public int Id { get; set; }
		public bool KeepForever { get; set; }
		public IdentityRef LastChangedBy { get; set; }
		public string LastChangedDate { get; set; }
		public object Logs { get; set; }
		public object OrchestrationPlan { get; set; }
		public string Parameters { get; set; }
		public object Plans { get; set; }

		/// <summary>
		/// REST API 7.0: Parameters to template expression evaluation
		/// </summary>
		public Dictionary<string, string> TemplateParameters { get; set; }
		public object Priority { get; set; }
		public object Project { get; set; }
		public object Properties { get; set; }
		public string Quality { get; set; }
		public object Queue { get; set; }
		public object QueueOptions { get; set; }
		public int QueuePosition { get; set; }
		public string QueueTime { get; set; }
		public object Reason { get; set; }
		public object Repository { get; set; }
		public IdentityRef RequestedBy { get; set; }
		public IdentityRef RequestedFor { get; set; }
		public object Result { get; set; }
		public bool RetainedByRelease { get; set; }
		public string SourceBranch { get; set; }
		public string SourceVersion { get; set; }
		public string SourceTime { get; set; }
		public BuildStatus Status { get; set; }
		public object Tags { get; set; }
		public object TriggerInfo { get; set; }
		public object TriggeredByBuild { get; set; }
		public string Uri { get; set; }
		public string Url { get; set; }
		public object ValidationResults { get; set; }

        public override string ToString()
        {
			string extra = "";
			if (TemplateParameters != null && TemplateParameters.Count != 0)
			{
				extra = $", Parameters: {JsonConvert.SerializeObject(TemplateParameters)}";
			}
			
			return $"{nameof(Build)} {{ Id = {Id}{extra} }}";
        }
    }
}
