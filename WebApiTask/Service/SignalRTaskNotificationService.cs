using ApplicationLayer.Services.Notifications;
using DomainLayer.Models;
using Microsoft.AspNetCore.SignalR;
using WebApiTask.Hubs;

namespace WebApiTask.Services
{
    public class SignalRTaskNotificationService : ITaskNotificationService
    {
        private readonly IHubContext<TaskHub> _hub;

        public SignalRTaskNotificationService(IHubContext<TaskHub> hub)
        {
            _hub = hub;
        }

        public Task NotifyTaskCreatedAsync(Tareas tarea)
        {
            return _hub.Clients.All.SendAsync("TaskCreated", tarea);
        }
    }
}