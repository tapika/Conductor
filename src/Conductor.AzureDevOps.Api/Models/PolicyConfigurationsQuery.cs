using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conductor.AzureDevOps.Api.Models
{
    /// <summary>
    /// Can set policy query options, for more information see:
    ///     https://docs.microsoft.com/en-us/rest/api/azure/devops/git/policy-configurations/list?view=azure-devops-rest-5.0
    /// </summary>
    public class PolicyConfigurationsQuery
    {
        public string RepositoryId { get; set; }
        public string RefName { get; set; }
        public string PolicyType { get; set; }
    }
}
