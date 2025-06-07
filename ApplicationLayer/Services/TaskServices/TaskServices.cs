using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainLayer.DTO;
using DomainLayer.Models;
using InfrastuctureLayer.Repositorio.Commons;
using Microsoft.Extensions.Logging;

namespace ApplicationLayer.Services.TaskServices
{
    public delegate bool ValidarTareaDelegate(Tareas t);

    public class TaskServices : ITaskServices
    {
        private readonly ICommonsProcess<Tareas> _repo;
        private readonly ILogger<TaskServices> _logger;
        private readonly ConcurrentDictionary<string, IEnumerable<Tareas>> _cache = new();

        public ValidarTareaDelegate Validador { get; set; }
        public Action<string>? Notificador { get; set; }

        public TaskServices(
            ICommonsProcess<Tareas> repo,
            ILogger<TaskServices> logger)
        {
            _repo = repo;
            _logger = logger;
            Validador = ValidacionPorDefecto;
        }

        private bool ValidacionPorDefecto(Tareas t) =>
            !(t is null) &&
            !string.IsNullOrWhiteSpace(t.Description) &&
            !string.IsNullOrWhiteSpace(t.Status) &&
            t.DueDate > DateTime.Now;


        public async Task<Response<Tareas>> GetTaskAllAsync()
        {
            var res = new Response<Tareas>();
            try
            {
                res.DataList = await _repo.GetAllAsync();
                res.Successful = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetTaskAllAsync error");
                res.Errors.Add(ex.Message);
            }
            return res;
        }

        public async Task<Response<Tareas>> GetTaskByIdAllAsync(int id)
        {
            var res = new Response<Tareas>();
            if (id <= 0)
            {
                res.Errors.Add("ID inválido"); return res;
            }

            try
            {
                var t = await _repo.GetIdAsync(id);
                res.SingleData = t;
                res.Successful = t != null;
                if (t == null) res.Message = "Tarea no encontrada";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetTaskById error");
                res.Errors.Add(ex.Message);
            }
            return res;
        }

        public async Task<Response<Tareas>> FiltrarTareasPorEstado(string estado)
        {
            var res = new Response<Tareas>();
            if (string.IsNullOrWhiteSpace(estado))
            {
                res.Errors.Add("Estado obligatorio"); return res;
            }

            try
            {
                if (_cache.TryGetValue(estado, out var cached))
                {
                    res.DataList = cached; res.Successful = true; return res;
                }

                var all = await _repo.GetAllAsync();
                var filt = all.Where(t => t.Status == estado).ToList();
                _cache[estado] = filt;
                res.DataList = filt;
                res.Successful = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Filtrar error");
                res.Errors.Add(ex.Message);
            }
            return res;
        }

        public async Task<Response<string>> AddTaskAllAsync(Tareas t) =>
            await ProcesarCrud(async () => await _repo.AddAsync(t), t, "Add");

        public async Task<Response<string>> UpdateTaskAllAsync(Tareas t) =>
            await ProcesarCrud(async () => await _repo.UpdateAsync(t), t, "Update");

        public async Task<Response<string>> DeleteTaskAllAsync(int id)
        {
            var res = new Response<string>();
            try
            {
                var result = await _repo.DeleteAsync(id);
                res.Successful = result.IsSuccess;
                res.Message = result.Message;
                if (!result.IsSuccess) _logger.LogWarning("Delete fail: {Msg}", result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete error");
                res.Errors.Add(ex.Message);
            }
            _cache.Clear();
            return res;
        }

        private async Task<Response<string>> ProcesarCrud(
            Func<Task<(bool IsSuccess, string Message)>> op,
            Tareas t, string tag)
        {
            var res = new Response<string>();

            if (!Validador(t))
            {
                res.Errors.Add("Validación fallida");
                _logger.LogWarning("{Tag}: invalid task data", tag);
                return res;
            }

            try
            {
                var result = await op();
                res.Successful = result.IsSuccess;
                res.Message = result.Message;

                if (result.IsSuccess)
                {
                    Notificador?.Invoke($"{tag}: {t.Description}");
                    _logger.LogInformation("{Tag}: OK - {Desc}", tag, t.Description);
                }
                else
                {
                    _logger.LogWarning("{Tag} repo fail: {Msg}", tag, result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Tag} error", tag);
                res.Errors.Add(ex.Message);
            }
            _cache.Clear();
            return res;
        }
    }
}
