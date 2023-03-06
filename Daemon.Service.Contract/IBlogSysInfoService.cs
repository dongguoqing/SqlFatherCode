using Daemon.Model;
namespace Daemon.Service.Contract
{
    public interface IBlogSysInfoService : IService
    {
        BlogSysInfo GetByInfoId(string infoId);
    }
}
