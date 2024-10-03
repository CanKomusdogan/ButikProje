using ButikProje.Models;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Microsoft.AspNet.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ButikProje
{
    [Authorize(Roles = "Admin")]
    public class NotificationHub : Hub
    {
        private const string NotifTitle = "P&D Boutique";
        public async Task NewOrder(string[] deviceTokens)
        {
            string notifBody = "Yeni Siparişler Var!";
            List<Task> tasks = new List<Task>();

            foreach (string token in deviceTokens)
            {
                Message messageToSend = new Message
                {
                    Token = token,
                    Notification = new Notification
                    {
                        Title = NotifTitle,
                        Body = notifBody
                    }
                };

                FirebaseMessaging messaging = FirebaseMessaging.GetMessaging(Startup.FirebaseApp);
                tasks.Add(messaging.SendAsync(messageToSend));
            }
            await Task.WhenAll(tasks);

            await Clients.All.SendAsync("NewOrder", notifBody);
        }
    }
}