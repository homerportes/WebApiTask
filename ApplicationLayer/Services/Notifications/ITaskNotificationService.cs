using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.Notifications
{
    using System.Threading.Tasks;
    using DomainLayer.Models;

    public interface ITaskNotificationService
    {
        Task NotifyTaskCreatedAsync(Tareas tarea);
    }
}
