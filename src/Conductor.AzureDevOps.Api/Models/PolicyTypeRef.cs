using System.Collections.Generic;
using Newtonsoft.Json;

namespace Conductor.AzureDevOps.Api.Models
{
	public class PolicyTypeRef
	{
		public const string BuildPolicyType = "0609b952-1397-4640-95ec-e00a01b2c241";

		public string DisplayName { get; set; } = "Build";
		public string Id { get; set; } = BuildPolicyType;
		public string Url { get; set; }
	}
}
