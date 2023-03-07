using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
namespace Daemon.Hubs
{
    public class BlogHub : Hub
    {
        public static void NotifyScheduleChanged()
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<BlogHub>();
            hub.Clients.All.update(new { UpdateType = scheduleUpdateType.ToString(), StartDate = startDate, EndDate = endDate });
        }
    }
}
