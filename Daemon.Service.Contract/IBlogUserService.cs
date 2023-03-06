
using Daemon.Model;
using Microsoft.AspNetCore.Mvc;
using Daemon.Model.ViewModel;

namespace Daemon.Service.Contract
{
    public interface IBlogUserService : IService
    {
        ActionResult Register(BlogUser user);

        ActionResult Login(BaseUser user);

        ActionResult ChangePassWord(BlogUser user);

        // BlogUser CurrentUser();
    }
}
