using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ApplicationLayer.Factories;
using ApplicationLayer.Services.TaskServices;
using ApplicationLayer.Services.Reactive;
using DomainLayer.DTO;
using DomainLayer.Models;

namespace WebApiTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TareasController : ControllerBase
    {
        private readonly TaskServices _service;
        private readonly ITareaFactory _factory;
        private readonly IReactiveTaskQueueService _queue;

        public TareasController(
            TaskServices service,
            ITareaFactory factory,
            IReactiveTaskQueueService queue)
        {
            _service = service;
            _factory = factory;
            _queue = queue;

            _service.Validador = t => t.Description.Length >= 5 && t.DueDate > DateTime.Now;
            _service.Notificador = msg => Console.WriteLine($"Notificación: {msg}");
        }

        [HttpGet("filtrar")]
        public async Task<ActionResult<Response<Tareas>>> Filtrar([FromQuery] string estado)
        {
            if (string.IsNullOrEmpty(estado))
                return BadRequest(new Response<Tareas>
                {
                    Message = "El parámetro 'estado' es obligatorio",
                    Successful = false
                });

            var response = await _service.FiltrarTareas(t => t.Status == estado);
            if (response.ThrereIsError)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<Response<Tareas>>> GetTaskAllAsync()
        {
            var response = await _service.GetTaskAllAsync();
            if (response.ThrereIsError)
                return StatusCode(500, response);
            return Ok(response);
        }

        [HttpGet("{id}")]
        [ActionName("GetTaskById")]
        public async Task<ActionResult<Response<Tareas>>> GetTaskByIdAllAsync(int id)
        {
            if (id <= 0)
                return BadRequest(new Response<Tareas>
                {
                    Message = "El ID debe ser mayor que 0",
                    Successful = false
                });

            var response = await _service.GetTaskByIdAllAsync(id);
            if (!response.Successful && !response.ThrereIsError)
                return NotFound(response);
            if (response.ThrereIsError)
                return StatusCode(500, response);

            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<Response<string>>> AddTaskAllAsync([FromBody] Tareas tarea)
        {
            if (tarea == null)
                return BadRequest(new Response<string>
                {
                    Message = "Los datos de la tarea son obligatorios",
                    Successful = false
                });
            if (string.IsNullOrEmpty(tarea.Description))
                return BadRequest(new Response<string>
                {
                    Message = "La descripción es obligatoria",
                    Successful = false
                });
            if (string.IsNullOrEmpty(tarea.Status))
                return BadRequest(new Response<string>
                {
                    Message = "El estado es obligatorio",
                    Successful = false
                });

            var response = await _service.AddTaskAllAsync(tarea);
            if (!response.Successful && !response.ThrereIsError)
                return BadRequest(response);
            if (response.ThrereIsError)
                return StatusCode(500, response);

            return CreatedAtAction("GetTaskById", new { id = tarea.Id }, response);
        }

        [HttpPost("crear/alta")]
        public async Task<ActionResult<Response<string>>> CrearTareaAlta([FromBody] string descripcion)
        {
            var tarea = _factory.CrearTareaAltaPrioridad(descripcion);
            var response = await _service.AddTaskAllAsync(tarea);
            if (!response.Successful && !response.ThrereIsError)
                return BadRequest(response);
            if (response.ThrereIsError)
                return StatusCode(500, response);

            return CreatedAtAction("GetTaskById", new { id = tarea.Id }, response);
        }

        [HttpPost("crear/normal")]
        public async Task<ActionResult<Response<string>>> CrearTareaNormal([FromBody] string descripcion)
        {
            var tarea = _factory.CrearTareaNormal(descripcion);
            var response = await _service.AddTaskAllAsync(tarea);
            if (!response.Successful && !response.ThrereIsError)
                return BadRequest(response);
            if (response.ThrereIsError)
                return StatusCode(500, response);

            return CreatedAtAction("GetTaskById", new { id = tarea.Id }, response);
        }

        [HttpPut]
        public async Task<ActionResult<Response<string>>> UpdateTaskAllAsync([FromBody] Tareas tarea)
        {
            if (tarea == null)
                return BadRequest(new Response<string>
                {
                    Message = "Los datos de la tarea son obligatorios",
                    Successful = false
                });
            if (tarea.Id <= 0)
                return BadRequest(new Response<string>
                {
                    Message = "El ID debe ser mayor que 0",
                    Successful = false
                });
            if (string.IsNullOrEmpty(tarea.Description))
                return BadRequest(new Response<string>
                {
                    Message = "La descripción es obligatoria",
                    Successful = false
                });
            if (string.IsNullOrEmpty(tarea.Status))
                return BadRequest(new Response<string>
                {
                    Message = "El estado es obligatorio",
                    Successful = false
                });

            var response = await _service.UpdateTaskAllAsync(tarea);
            if (!response.Successful && !response.ThrereIsError)
                return BadRequest(response);
            if (response.ThrereIsError)
                return StatusCode(500, response);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Response<string>>> DeleteTaskAllAsync(int id)
        {
            if (id <= 0)
                return BadRequest(new Response<string>
                {
                    Message = "El ID debe ser mayor que 0",
                    Successful = false
                });

            var response = await _service.DeleteTaskAllAsync(id);
            if (!response.Successful && response.Message.Contains("no existe"))
                return NotFound(response);
            if (response.ThrereIsError)
                return StatusCode(500, response);

            return Ok(response);
        }

        [HttpPost("queue/create")]
        public ActionResult QueueCreate([FromBody] Tareas tarea)
        {
            _queue.EnqueueCreate(tarea);
            return Accepted();
        }

        [HttpPost("queue/update")]
        public ActionResult QueueUpdate([FromBody] Tareas tarea)
        {
            _queue.EnqueueUpdate(tarea);
            return Accepted();
        }

        [HttpPost("queue/delete/{id}")]
        public ActionResult QueueDelete(int id)
        {
            _queue.EnqueueDelete(id);
            return Accepted();
        }
    }
}
