using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DomainLayer.Models;
using DomainLayer.DTO;
using Svc = ApplicationLayer.Services.TaskServices.TaskServices;

namespace ApplicationLayer.Services.Reactive
{
    public class ReactiveTaskQueueService : IReactiveTaskQueueService, IDisposable
    {
        private readonly Subject<Tareas> _create = new();
        private readonly Subject<Tareas> _update = new();
        private readonly Subject<int> _delete = new();

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReactiveTaskQueueService> _logger;

        private readonly IDisposable _subCreate;
        private readonly IDisposable _subUpdate;
        private readonly IDisposable _subDelete;

        public IObservable<Response<string>> CreateResponses { get; }
        public IObservable<Response<string>> UpdateResponses { get; }
        public IObservable<Response<string>> DeleteResponses { get; }

        public ReactiveTaskQueueService(
            IServiceScopeFactory scopeFactory,
            ILogger<ReactiveTaskQueueService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            CreateResponses = BuildPipeline(
                _create,
                (svc, t) => svc.AddTaskAllAsync(t),
                "CREATE");

            UpdateResponses = BuildPipeline(
                _update,
                (svc, t) => svc.UpdateTaskAllAsync(t),
                "UPDATE");

            DeleteResponses = BuildPipeline(
                _delete,
                (svc, id) => svc.DeleteTaskAllAsync(id),
                "DELETE");

            _subCreate = SubscribeWithLog(CreateResponses, "CREATE");
            _subUpdate = SubscribeWithLog(UpdateResponses, "UPDATE");
            _subDelete = SubscribeWithLog(DeleteResponses, "DELETE");
        }

        private IObservable<Response<string>> BuildPipeline<T>(
            IObservable<T> source,
            Func<Svc, T, Task<Response<string>>> operation,
            string tag)
        {
            return source
                .Select(item => Observable.FromAsync(async () =>
                {
                    _logger.LogInformation("{Tag}: received item", tag);
                    using var scope = _scopeFactory.CreateScope();
                    var svc = scope.ServiceProvider.GetRequiredService<Svc>();
                    return await operation(svc, item);
                }))
                .Concat()
                .Retry(3)
                .Catch<Response<string>, Exception>(ex =>
                {
                    _logger.LogError(ex, "{Tag}: pipeline failed after retries", tag);
                    return Observable.Empty<Response<string>>();
                });
        }

        private IDisposable SubscribeWithLog(
            IObservable<Response<string>> stream, string tag)
        {
            return stream.Subscribe(
                r => _logger.LogInformation("{Tag}: {Msg}", tag, r.Message),
                e => _logger.LogError(e, "{Tag}: observable error", tag));
        }

    
        public void EnqueueCreate(Tareas tarea) => _create.OnNext(tarea);
        public void EnqueueUpdate(Tareas tarea) => _update.OnNext(tarea);
        public void EnqueueDelete(int id) => _delete.OnNext(id);


        public void Dispose()
        {
            _subCreate.Dispose();
            _subUpdate.Dispose();
            _subDelete.Dispose();

            _create.OnCompleted(); _create.Dispose();
            _update.OnCompleted(); _update.Dispose();
            _delete.OnCompleted(); _delete.Dispose();
        }
    }
}
