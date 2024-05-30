using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conductor.AzureDevOps.Api.Models
{
    public class RepositoryResourceParameters
    {
        public string RefName { get; set; }
        public string Token { get; set; }
        public string TokenType { get; set; }
        public string Version { get; set; }
    }
}
