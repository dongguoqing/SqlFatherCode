using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Daemon.Model;

namespace Daemon.Service.Contract
{
    public interface IDictService:IService
    {
         Dict FindById(int id);
    }
}