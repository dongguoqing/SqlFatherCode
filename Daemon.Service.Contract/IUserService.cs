namespace Daemon.Service.Contract
{
    using Daemon.Model;
    using Microsoft.AspNetCore.Mvc;
    using Daemon.Model.ViewModel;
    public interface IUserService : IService
    {
        ActionResult Register(User user);

        ActionResult GetUserInfo();

        ActionResult Login(BaseUser user);
    }
}
