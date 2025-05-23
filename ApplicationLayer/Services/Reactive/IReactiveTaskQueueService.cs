using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Models;
using DomainLayer.DTO;
using System;

namespace ApplicationLayer.Services.Reactive
{
    public interface IReactiveTaskQueueService
    {
        void EnqueueCreate(Tareas tarea);
        void EnqueueUpdate(Tareas tarea);
        void EnqueueDelete(int id);
        IObservable<Response<string>> CreateResponses { get; }
        IObservable<Response<string>> UpdateResponses { get; }
        IObservable<Response<string>> DeleteResponses { get; }
    }
}
