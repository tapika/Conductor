using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Conductor.AzureDevOps.Api.Models
{
	public class SettingsScope
	{
		public string RefName { get; set; }
		public MatchKind MatchKind { get; set; }
		public string RepositoryId { get; set; }
	}
}
