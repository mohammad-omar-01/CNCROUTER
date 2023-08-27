using GrblSpeaker.Utilites;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GrblSpeaker.Controllers
{
    public class NotficationController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotficationController(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public IActionResult SendNotificationToClients(string message)
        {
            _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
            return Ok();
        }
    }

}
