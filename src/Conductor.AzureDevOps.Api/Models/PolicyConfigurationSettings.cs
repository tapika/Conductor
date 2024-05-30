using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Conductor.AzureDevOps.Api.Models
{
	public class PolicyConfigurationSettings
	{
		public int BuildDefinitionId { get; set; }
		public bool QueueOnSourceUpdateOnly { get; set; } = true;
		public bool ManualQueueOnly { get; set; }
		public string DisplayName { get; set; }
		public double ValidDuration { get; set; }
        //public object Scope { get; set; }
        public IEnumerable<SettingsScope> Scope { get; set; }
    }
}
