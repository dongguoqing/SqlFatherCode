using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
namespace Daemon.Hubs
{
    public class BlogHub : Hub
    {
        public async Task NotifyScheduleChanged(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
