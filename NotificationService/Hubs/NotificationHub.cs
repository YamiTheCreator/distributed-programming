using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Hubs;

public class NotificationHub : Hub
{
    public async Task JoinAnalysisGroup(string analysisId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"analysis_{analysisId}");
    }

    public async Task LeaveAnalysisGroup(string analysisId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"analysis_{analysisId}");
    }
}