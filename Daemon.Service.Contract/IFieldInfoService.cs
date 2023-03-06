using Daemon.Model;
using Microsoft.AspNetCore.Mvc;
using Daemon.Model.ViewModel;
using System.Linq;

namespace Daemon.Service.Contract
{
    public interface IFieldInfoService : IService
    {
        string GenerateCreateSql(int id);

        FieldInfo FindById(int id);

        IQueryable<FieldInfo> GetFieldInfos();
    }
}