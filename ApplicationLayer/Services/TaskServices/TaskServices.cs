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
            return !string.IsNullOrEmpty(tarea.Description)
                && tarea.DueDate > DateTime.Now;
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
                if (!Validador(tarea)) 
                {
                    response.Message = "Error: Validación fallida";
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
                if (!Validador(tarea)) 
                {
                    response.Message = "Error: Validación fallida";
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