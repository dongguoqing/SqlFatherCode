using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daemon.Model.ViewModel
{
    public class RequestConnectInfo
    {
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
    }
}