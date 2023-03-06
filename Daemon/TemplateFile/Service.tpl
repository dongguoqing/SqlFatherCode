
using Daemon.Service.Contract;
using Daemon.Model;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;
using Microsoft.AspNetCore.Mvc;
using Daemon.Repository.Contract;
namespace Daemon.Service
{
    public class {0}Service : I{0}Service
    {
        private readonly I{0}Repository _repository;
        public BlogSysInfoService(I{0}Repository repository)
        {
            _repository = repository;
        }
    }
}
