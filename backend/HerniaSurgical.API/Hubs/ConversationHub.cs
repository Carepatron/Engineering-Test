using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace HerniaSurgical.API.Hubs
{
    public class ConversationHub : Hub
    {
        public async Task SendMessage(Guid conversationId, object message)
        {
            await Clients.All.SendAsync("ReceiveMessage", conversationId, message);
        }

        public async Task JoinConversation(Guid conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
            Console.WriteLine($"User joined conversation: {conversationId}");
        }

        public async Task LeaveConversation(Guid conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
            Console.WriteLine($"User left conversation: {conversationId}");
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}