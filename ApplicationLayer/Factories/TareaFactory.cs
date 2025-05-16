using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Factories
{
    using DomainLayer.Models;

    public class TareaFactory : ITareaFactory
    {
        public Tareas CrearTareaAltaPrioridad(string descripcion)
        {
            return new Tareas
            {
                Description = descripcion,
                Status = "Alta Prioridad",
                DueDate = DateTime.Now.AddHours(12),
                AdditionalData = "Esta tarea requiere atención urgente."
            };
        }

        public Tareas CrearTareaNormal(string descripcion)
        {
            return new Tareas
            {
                Description = descripcion,
                Status = "Pendiente",
                DueDate = DateTime.Now.AddDays(3),
                AdditionalData = "Tarea con prioridad estándar."
            };
        }
    }

}
