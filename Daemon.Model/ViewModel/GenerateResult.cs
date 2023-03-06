using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daemon.Model.ViewModel
{
    public class GenerateResult
    {
        public string ModelEntity { get; set; }

        public string ControllerStr { get; set; }

        public string RepositoryStr { get; set; }

        public string IRepositoryStr { get; set; }

        public string ServiceStr { get; set; }

        public string IServiceStr { get; set; }

        public string GenerateDto { get; set; }

        public string RepositoryName { get; set; }

        public string IRepositoryName { get; set; }

        public string ServiceName { get; set; }

        public string IServiceName { get; set; }
    }
}