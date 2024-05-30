using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conductor.AzureDevOps.Api.Models
{
    public class RunResourcesParameters
    {
        public String Builds { get; set; }
        public String Containers { get; set; }
        public String Packages { get; set; }
        public String Pipelines { get; set; }
        public Dictionary<string, RepositoryResourceParameters> Repositories { get; set; }
    }
}
