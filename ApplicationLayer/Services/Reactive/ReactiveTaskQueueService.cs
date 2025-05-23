using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.DependencyInjection;
using DomainLayer.Models;
using DomainLayer.DTO;
using Svc = ApplicationLayer.Services.TaskServices.TaskServices;

namespace ApplicationLayer.Services.Reactive
{
    public class ReactiveTaskQueueService : IReactiveTaskQueueService, IDisposable
    {
        private readonly Subject<Tareas> _createSubject = new Subject<Tareas>();
        private readonly Subject<Tareas> _updateSubject = new Subject<Tareas>();
        private readonly Subject<int> _deleteSubject = new Subject<int>();
        private readonly IDisposable _createSub;
        private readonly IDisposable _updateSub;
        private readonly IDisposable _deleteSub;
        private readonly IServiceScopeFactory _scopeFactory;

        public IObservable<Response<string>> CreateResponses { get; }
        public IObservable<Response<string>> UpdateResponses { get; }
        public IObservable<Response<string>> DeleteResponses { get; }

        public ReactiveTaskQueueService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            CreateResponses = _createSubject
                .Select(t => Observable.FromAsync(async () =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var svc = scope.ServiceProvider.GetRequiredService<Svc>();
                    return await svc.AddTaskAllAsync(t);
                }))
                .Concat();

            UpdateResponses = _updateSubject
                .Select(t => Observable.FromAsync(async () =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var svc = scope.ServiceProvider.GetRequiredService<Svc>();
                    return await svc.UpdateTaskAllAsync(t);
                }))
                .Concat();

            DeleteResponses = _deleteSubject
                .Select(id => Observable.FromAsync(async () =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var svc = scope.ServiceProvider.GetRequiredService<Svc>();
                    return await svc.DeleteTaskAllAsync(id);
                }))
                .Concat();

            _createSub = CreateResponses.Subscribe();
            _updateSub = UpdateResponses.Subscribe();
            _deleteSub = DeleteResponses.Subscribe();
        }

        public void EnqueueCreate(Tareas tarea) => _createSubject.OnNext(tarea);
        public void EnqueueUpdate(Tareas tarea) => _updateSubject.OnNext(tarea);
        public void EnqueueDelete(int id) => _deleteSubject.OnNext(id);

        public void Dispose()
        {
            _createSub.Dispose();
            _updateSub.Dispose();
            _deleteSub.Dispose();
            _createSubject.OnCompleted();
            _updateSubject.OnCompleted();
            _deleteSubject.OnCompleted();
        }
    }
}
