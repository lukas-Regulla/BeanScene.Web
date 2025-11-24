using Microsoft.AspNetCore.SignalR;

namespace BeanScene.Web.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync(
                "ReceiveMessage",
                user,
                message,
                DateTime.Now.ToString("HH:mm")
            );
        }
    }
}
