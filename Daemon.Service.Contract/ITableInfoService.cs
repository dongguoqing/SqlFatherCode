using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daemon.Service.Contract
{
    public interface ITableInfoService:IService
    {
         string GenerateCreateSql(int id);
    }
}