namespace ProductMonitoring.API.SignalRsetup
{
    using Microsoft.AspNetCore.SignalR;

    public class SolutionNotificationHub : Hub
    {
        // Optional: track connections/logging
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }

}
