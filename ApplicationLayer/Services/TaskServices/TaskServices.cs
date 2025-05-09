using DomainLayer.DTO;
using DomainLayer.Models;
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
        public Action<string> Notificador { get; set; }

        public TaskServices(
            ICommonsProcess<Tareas> commonsProces,
            ValidarTareaDelegate? validador = null)
        {
            _commonsProcess = commonsProces;
            Validador = validador ?? ValidacionPorDefecto;
        }

        private bool ValidacionPorDefecto(Tareas tarea)
        {
            if (tarea == null)
                return false;

            if (string.IsNullOrEmpty(tarea.Description))
                return false;

            if (string.IsNullOrEmpty(tarea.Status))
                return false;

            if (tarea.DueDate == default)
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

                    Func<Tareas, int> calcularDias = t => (t.DueDate - DateTime.Now).Days;
                    response.CalculosExtra["DiasRestantes"] = calcularDias(result);
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
            }
            return response;
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
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
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
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
            }
            return response;
        }

        public async Task<Response<string>> DeleteTaskAllAsync(int id)
        {
            var response = new Response<string>();
            try
            {
                // Validación del ID
                if (id <= 0)
                {
                    response.Message = "Error: ID inválido";
                    response.Errors.Add("El ID debe ser mayor que 0");
                    response.Successful = false;
                    return response;
                }

                var result = await _commonsProcess.DeleteAsync(id);
                if (result.IsSuccess)
                {
                    Notificador?.Invoke($"Tarea eliminada: ID {id}");
                }
                response.Message = result.Message;
                response.Successful = result.IsSuccess;
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
            }
            return response;
        }

        public async Task<Response<Tareas>> FiltrarTareas(Func<Tareas, bool> filtro)
        {
            var response = new Response<Tareas>();
            try
            {
                if (filtro == null)
                {
                    response.Message = "Error: Filtro no proporcionado";
                    response.Errors.Add("El filtro es obligatorio");
                    response.Successful = false;
                    return response;
                }

                var todas = await _commonsProcess.GetAllAsync();
                response.DataList = todas.Where(filtro).ToList();
                response.Successful = true;
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
            }
            return response;
        }
    }
}