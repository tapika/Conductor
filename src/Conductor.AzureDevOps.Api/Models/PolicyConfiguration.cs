using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Conductor.AzureDevOps.Api.Models
{
	public class PolicyConfiguration
	{
		[JsonProperty("_links")]
		public ReferenceLinks Links { get; set; }
		public IdentityRef CreatedBy { get; set; }
		public string CreatedDate { get; set; }
		public int Id { get; set; }
		public bool IsBlocking { get; set; } = true;
		public bool IsDeleted { get; set; }
		public bool IsEnabled { get; set; } = true;
		public bool IsEnterpriseManaged { get; set; }
		public int Revision { get; set; }
		public PolicyConfigurationSettings Settings { get; set; } = new PolicyConfigurationSettings();
		public PolicyTypeRef type { get; set; } = new PolicyTypeRef();
		public string Url { get; set; }

        public override string ToString()
        {
			string payload = "";
			var scopes = Settings.Scope.ToArray();
			if (scopes.Length == 1)
			{
				payload = $", BuildDefinitionId = {Settings.BuildDefinitionId}, ScopeBranch: '{scopes[0].RefName}' ";
			}
			
			return $"{nameof(PolicyConfiguration)}{{ Id = {Id}, IsEnabled = {IsEnabled} {payload}}}";
        }
    }
}
