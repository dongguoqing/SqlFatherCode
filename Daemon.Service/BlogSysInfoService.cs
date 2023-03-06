
using Daemon.Service.Contract;
using Daemon.Model;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;
using Microsoft.AspNetCore.Mvc;
using Daemon.Repository.Contract;
namespace Daemon.Service
{
    public class BlogSysInfoService : IBlogSysInfoService
    {
        private readonly IBlogSysInfoRepository _repository;
        public BlogSysInfoService(IBlogSysInfoRepository repository)
        {
            _repository = repository;
        }
        public BlogSysInfo GetByInfoId(string infoId)
        {
            return _repository.FindAll().FirstOrDefault(a => a.InfoId == infoId);
        }
    }
}
