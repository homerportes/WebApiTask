using ApplicationLayer.Services.Notifications;
using DomainLayer.Models;
using Microsoft.AspNetCore.SignalR;
using WebApiTask.Hubs;

namespace WebApiTask.Services
{
    public class SignalRTaskNotificationService : ITaskNotificationService
    {
        private readonly IHubContext<TaskHub> _hub;
        private readonly ILogger<SignalRTaskNotificationService> _log;

        public SignalRTaskNotificationService(IHubContext<TaskHub> hub, ILogger<SignalRTaskNotificationService> log)
        {
            _hub = hub;
            _log = log;
        }

        public async Task NotifyTaskCreatedAsync(Tareas tarea)
        {
            try
            {
                await _hub.Clients.All.SendAsync("TaskCreated", tarea);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "SignalR notification failed");
            }
        }
    }
}