using Daemon.Model;
using Microsoft.AspNetCore.Mvc;
using Daemon.Repository.Contract;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
namespace Daemon.Controllers.V1
{
    [ApiController]
    [Route("blueearth/customer")]
    public class CustomerController: BaseApiController<Customer, ICustomerRepository>
    {
        public CustomerController(ICustomerRepository repository, IHttpContextAccessor httpContextAccessor) : base(repository, httpContextAccessor)
        {
        }
    }
}
