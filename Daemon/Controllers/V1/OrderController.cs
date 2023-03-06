using Daemon.Model;
using Microsoft.AspNetCore.Mvc;
using Daemon.Repository.Contract;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
namespace Daemon.Controllers.V1
{
    [ApiController]
    [Route("blueearth/order")]
    public class OrderController: BaseApiController<Order, IOrderRepository>
    {
        public OrderController(IOrderRepository repository, IHttpContextAccessor httpContextAccessor) : base(repository, httpContextAccessor)
        {
        }
    }
}
