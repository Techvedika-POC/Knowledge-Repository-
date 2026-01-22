using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Knowledge_Repository.SignalR.Hubs
{
    public class JuryHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var http = Context.GetHttpContext();
            var eventId = http?.Request.Query["eventId"].ToString();
            if (!string.IsNullOrWhiteSpace(eventId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"event-{eventId}");
            }
            await base.OnConnectedAsync();
        }

        public Task JoinEventGroup(string eventId) =>
            Groups.AddToGroupAsync(Context.ConnectionId, $"event-{eventId}");

        public Task LeaveEventGroup(string eventId) =>
            Groups.RemoveFromGroupAsync(Context.ConnectionId, $"event-{eventId}");
    }
}
