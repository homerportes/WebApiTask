using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ApplicationLayer.Services.TaskServices;
using ApplicationLayer.Factories;
using ApplicationLayer.Services.Reactive;
using DomainLayer.DTO;
using DomainLayer.Models;

namespace WebApiTask.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TareasController : ControllerBase
    {
        private readonly ITaskServices _svc;
        private readonly ITareaFactory _factory;
        private readonly IReactiveTaskQueueService _queue;
        private readonly ILogger<TareasController> _log;

        public TareasController(
            ITaskServices svc,
            ITareaFactory factory,
            IReactiveTaskQueueService queue,
            ILogger<TareasController> log)
        {
            _svc = svc;
            _factory = factory;
            _queue = queue;
            _log = log;
        }

        [HttpPost("queue/create")]
        public IActionResult QueueCreate([FromBody] Tareas t)
        {
            _queue.EnqueueCreate(t);
            _log.LogInformation("Enqueued CREATE {Desc}", t.Description);
            return Accepted();
        }

        [HttpPost("queue/update")]
        public IActionResult QueueUpdate([FromBody] Tareas t)
        {
            _queue.EnqueueUpdate(t);
            _log.LogInformation("Enqueued UPDATE {Id}", t.Id);
            return Accepted();
        }

        [HttpPost("queue/delete/{id:int}")]
        public IActionResult QueueDelete(int id)
        {
            _queue.EnqueueDelete(id);
            _log.LogInformation("Enqueued DELETE {Id}", id);
            return Accepted();
        }

        [HttpGet]
        public async Task<IActionResult> All()
        {
            var r = await _svc.GetTaskAllAsync();
            return r.HasError ? StatusCode(500, r) : Ok(r);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ById(int id)
        {
            var r = await _svc.GetTaskByIdAllAsync(id);
            if (r.HasError) return StatusCode(500, r);
            return r.Successful ? Ok(r) : NotFound(r);
        }

        [HttpGet("filtrar")]
        public async Task<IActionResult> Filtrar([FromQuery] string estado)
        {
            var r = await _svc.FiltrarTareasPorEstado(estado);
            if (r.HasError) return StatusCode(500, r);
            return r.Successful ? Ok(r) : BadRequest(r);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Tareas t)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var r = await _svc.AddTaskAllAsync(t);
            if (r.HasError) return StatusCode(500, r);
            if (!r.Successful) return BadRequest(r);

            return CreatedAtAction(nameof(ById), new { id = t.Id }, r);
        }

        [HttpPost("crear/alta")]
        public async Task<IActionResult> Alta([FromBody] string descripcion)
        {
            var tarea = _factory.CrearTareaAltaPrioridad(descripcion);
            var r = await _svc.AddTaskAllAsync(tarea);
            if (r.HasError) return StatusCode(500, r);
            if (!r.Successful) return BadRequest(r);

            return CreatedAtAction(nameof(ById), new { id = tarea.Id }, r);
        }

        [HttpPost("crear/normal")]
        public async Task<IActionResult> Normal([FromBody] string descripcion)
        {
            var tarea = _factory.CrearTareaNormal(descripcion);
            var r = await _svc.AddTaskAllAsync(tarea);
            if (r.HasError) return StatusCode(500, r);
            if (!r.Successful) return BadRequest(r);

            return CreatedAtAction(nameof(ById), new { id = tarea.Id }, r);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Tareas t)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var r = await _svc.UpdateTaskAllAsync(t);
            if (r.HasError) return StatusCode(500, r);
            return r.Successful ? Ok(r) : BadRequest(r);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _svc.DeleteTaskAllAsync(id);
            if (r.HasError) return StatusCode(500, r);
            return r.Successful ? Ok(r) : NotFound(r);
        }
    }
}
