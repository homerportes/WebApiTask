using DomainLayer.Models;
using DomainLayer.DTO;
using InfrastuctureLayer.Repositorio.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.TaskServices
{
    public delegate bool ValidarTareaDelegate(Tareas tarea);

    public class TaskServices
    {
        private readonly ICommonsProcess<Tareas> _commonsProcess;

        public ValidarTareaDelegate Validador { get; set; }
        public Action<string>? Notificador { get; set; }

        private readonly Dictionary<string, IEnumerable<Tareas>> _filtroCache = new Dictionary<string, IEnumerable<Tareas>>();

        public TaskServices(
            ICommonsProcess<Tareas> commonsProcess,
            ValidarTareaDelegate? validador = null,
            Action<string>? notificador = null)
        {
            _commonsProcess = commonsProcess;
            Validador = validador ?? ValidacionPorDefecto;
            Notificador = notificador;
        }

        private bool ValidacionPorDefecto(Tareas tarea)
        {
            if (tarea == null
                || string.IsNullOrEmpty(tarea.Description)
                || string.IsNullOrEmpty(tarea.Status)
                || tarea.DueDate == default)
                return false;
            return tarea.DueDate > DateTime.Now;
        }

        public async Task<Response<Tareas>> GetTaskAllAsync()
        {
            var response = new Response<Tareas>();
            try
            {
                response.DataList = await _commonsProcess.GetAllAsync();
                response.Successful = true;
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
                response.Successful = false;
            }
            return response;
        }

        public async Task<Response<Tareas>> GetTaskByIdAllAsync(int id)
        {
            var response = new Response<Tareas>();
            try
            {
                if (id <= 0)
                {
                    response.Message = "ID de tarea inválido";
                    response.Errors.Add("El ID debe ser mayor que 0");
                    response.Successful = false;
                    return response;
                }

                var result = await _commonsProcess.GetIdAsync(id);
                if (result != null)
                {
                    response.SingleData = result;
                    response.Successful = true;
                    response.CalculosExtra["DiasRestantes"] = (result.DueDate - DateTime.Now).Days;
                }
                else
                {
                    response.Message = "Tarea no encontrada";
                    response.Successful = false;
                }
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
                response.Successful = false;
            }
            return response;
        }

        public async Task<Response<Tareas>> FiltrarTareasPorEstado(string estado)
        {
            var response = new Response<Tareas>();
            if (string.IsNullOrEmpty(estado))
            {
                response.Message = "Error: El parámetro 'estado' es obligatorio";
                response.Errors.Add("El estado no puede estar vacío");
                response.Successful = false;
                return response;
            }

            try
            {
                string clave = $"Status:{estado}";

                if (_filtroCache.ContainsKey(clave))
                {
                    response.DataList = _filtroCache[clave];
                    response.Successful = true;
                    return response;
                }

                var todas = await _commonsProcess.GetAllAsync();
                var filtradas = todas.Where(t => t.Status == estado).ToList();
                _filtroCache[clave] = filtradas;
                response.DataList = filtradas;
                response.Successful = true;
                return response;
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
                response.Successful = false;
                return response;
            }
        }

        public async Task<Response<string>> AddTaskAllAsync(Tareas tarea)
        {
            var response = new Response<string>();
            try
            {
                if (tarea == null)
                {
                    response.Message = "Error: Datos de tarea no proporcionados";
                    response.Errors.Add("El objeto tarea no puede ser nulo");
                    response.Successful = false;
                    return response;
                }

                if (string.IsNullOrEmpty(tarea.Description))
                {
                    response.Message = "Error: Falta descripción";
                    response.Errors.Add("La descripción es obligatoria");
                    response.Successful = false;
                    return response;
                }

                if (string.IsNullOrEmpty(tarea.Status))
                {
                    response.Message = "Error: Falta estado";
                    response.Errors.Add("El estado es obligatorio");
                    response.Successful = false;
                    return response;
                }

                if (tarea.DueDate == default)
                {
                    response.Message = "Error: Falta fecha de vencimiento";
                    response.Errors.Add("La fecha de vencimiento es obligatoria");
                    response.Successful = false;
                    return response;
                }

                if (!Validador(tarea))
                {
                    response.Message = "Error: Validación fallida";
                    response.Errors.Add("La tarea no cumple con los criterios de validación");
                    response.Successful = false;
                    return response;
                }

                var result = await _commonsProcess.AddAsync(tarea);
                Notificador?.Invoke($"Tarea creada: {tarea.Description}");
                response.Message = result.Message;
                response.Successful = result.IsSuccess;

                _filtroCache.Clear();
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
                response.Successful = false;
            }
            return response;
        }

        public async Task<Response<string>> UpdateTaskAllAsync(Tareas tarea)
        {
            var response = new Response<string>();
            try
            {
                if (tarea == null)
                {
                    response.Message = "Error: Datos de tarea no proporcionados";
                    response.Errors.Add("El objeto tarea no puede ser nulo");
                    response.Successful = false;
                    return response;
                }

                if (tarea.Id <= 0)
                {
                    response.Message = "Error: ID inválido";
                    response.Errors.Add("El ID debe ser mayor que 0");
                    response.Successful = false;
                    return response;
                }

                if (string.IsNullOrEmpty(tarea.Description))
                {
                    response.Message = "Error: Falta descripción";
                    response.Errors.Add("La descripción es obligatoria");
                    response.Successful = false;
                    return response;
                }

                if (string.IsNullOrEmpty(tarea.Status))
                {
                    response.Message = "Error: Falta estado";
                    response.Errors.Add("El estado es obligatorio");
                    response.Successful = false;
                    return response;
                }

                if (!Validador(tarea))
                {
                    response.Message = "Error: Validación fallida";
                    response.Errors.Add("La tarea no cumple con los criterios de validación");
                    response.Successful = false;
                    return response;
                }

                var result = await _commonsProcess.UpdateAsync(tarea);
                response.Message = result.Message;
                response.Successful = result.IsSuccess;

                _filtroCache.Clear();
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
                response.Successful = false;
            }
            return response;
        }

        public async Task<Response<string>> DeleteTaskAllAsync(int id)
        {
            var response = new Response<string>();
            try
            {
                if (id <= 0)
                {
                    response.Message = "Error: ID inválido";
                    response.Errors.Add("El ID debe ser mayor que 0");
                    response.Successful = false;
                    return response;
                }

                var result = await _commonsProcess.DeleteAsync(id);
                if (result.IsSuccess)
                    Notificador?.Invoke($"Tarea eliminada: ID {id}");
                response.Message = result.Message;
                response.Successful = result.IsSuccess;

                _filtroCache.Clear();
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
                response.Successful = false;
            }
            return response;
        }
    }
}
