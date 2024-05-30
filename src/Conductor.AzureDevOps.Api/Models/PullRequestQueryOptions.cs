using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conductor.AzureDevOps.Api.Models
{
    /// <summary>
    /// Can set pull requet query options, for more information see:
    ///     https://docs.microsoft.com/en-us/rest/api/azure/devops/git/pull-requests/get-pull-requests?view=azure-devops-rest-6.0&tabs=HTTP
    /// </summary>
    public class PullRequestQueryOptions
    {
        public int? Skip { get; set; }
        public int? Top { get; set; }
        public PullRequestStatus? Status { get; set; }
    }
}
