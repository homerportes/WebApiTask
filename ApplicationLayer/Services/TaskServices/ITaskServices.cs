using System.Threading.Tasks;
using DomainLayer.DTO;
using DomainLayer.Models;

namespace ApplicationLayer.Services.TaskServices
{
    public interface ITaskServices
    {
        Task<Response<Tareas>> GetTaskAllAsync();
        Task<Response<Tareas>> GetTaskByIdAllAsync(int id);
        Task<Response<Tareas>> FiltrarTareasPorEstado(string estado);
        Task<Response<string>> AddTaskAllAsync(Tareas tarea);
        Task<Response<string>> UpdateTaskAllAsync(Tareas tarea);
        Task<Response<string>> DeleteTaskAllAsync(int id);
    }
}
