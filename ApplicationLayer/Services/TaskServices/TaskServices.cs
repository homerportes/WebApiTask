using Azure.Core;
using DomainLayer.DTO;
using DomainLayer.Models;
using InfrastuctureLayer.Repositorio.Commons;
using System;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.TaskServices
{
    public class TaskServices
    {
        private readonly ICommonsProcess<Tareas> _commonsProcess;

        public TaskServices(ICommonsProcess<Tareas> commonsProces)
        {
            _commonsProcess = commonsProces;
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
                }
                else
                {
                    response.Successful = false;
                    response.Message = ("No se encontró la tarea con el ID proporcionado.");
                }
                response.Successful = true;
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
                var result = await _commonsProcess.AddAsync(tarea);
                response.Message = result.Message;
                response.Successful = result.IsSuccess ;
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
                response.Message = result.Message;
                response.Successful = result.IsSuccess;
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
            }

            return response;
        }
    }
}
