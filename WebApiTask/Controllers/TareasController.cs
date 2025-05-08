using ApplicationLayer.Services.TaskServices;
using DomainLayer.DTO;
using DomainLayer.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApiTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TareasController : ControllerBase
    {
        private readonly TaskServices _service;

        public TareasController(TaskServices service)
        {
            _service = service;

            _service.Validador = t =>
            {
                return t.Description?.Length >= 5 && t.DueDate > DateTime.Now;
            };

            _service.Notificador = mensaje => Console.WriteLine($"Notificación: {mensaje}");
        }

        [HttpGet("filtrar")]
        public async Task<ActionResult<Response<Tareas>>> Filtrar([FromQuery] string estado)
        {
            return await _service.FiltrarTareas(t => t.Status == estado);
        }

        [HttpGet]
        public async Task<ActionResult<Response<Tareas>>> GetTaskAllAsync()
            => await _service.GetTaskAllAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Response<Tareas>>> GetTaskByIdAllAsync(int id)
            => await _service.GetTaskByIdAllAsync(id);

        [HttpPost]
        public async Task<ActionResult<Response<string>>> AddTaskAllAsync(Tareas tarea)
            => await _service.AddTaskAllAsync(tarea);

        [HttpPut]
        public async Task<ActionResult<Response<string>>> UpdateTaskAllAsync(Tareas tarea)
            => await _service.UpdateTaskAllAsync(tarea);

        [HttpDelete("{id}")]
        public async Task<ActionResult<Response<string>>> DeleteTaskAllAsync(int id)
            => await _service.DeleteTaskAllAsync(id);
    }
}