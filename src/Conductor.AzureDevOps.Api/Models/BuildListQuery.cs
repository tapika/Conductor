using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conductor.AzureDevOps.Api.Models
{
    /// <summary>
    /// Can set build list query options, for more information see:
    ///     https://docs.microsoft.com/en-us/rest/api/azure/devops/build/builds/list?view=azure-devops-rest-6.0
    /// </summary>
    public class BuildListQuery
    {
		[JsonProperty("$top")]
        public int? NumberOfBuilds { get; set; }
        public string BranchName { get; set; }
        public string BuildIds { get; set; }
        public string BuildNumber { get; set; }
        public string ContinuationToken { get; set; }
        public string Definitions { get; set; }
        public string DeletedFilter { get; set; }
        public int? MaxBuildsPerDefinition { get; set; }
        public string MaxTime { get; set; }
        public string MinTime { get; set; }
        public string Properties { get; set; }
        public string QueryOrder { get; set; }
        public string Queues { get; set; }
        public string ReasonFilter { get; set; }
        public string RepositoryId { get; set; }
        public string RepositoryType { get; set; }
        public string RequestedFor { get; set; }
        public BuildStatus? ResultFilter { get; set; }
        public BuildStatus? StatusFilter { get; set; }
        public string TagFilters { get; set; }
    }
}
