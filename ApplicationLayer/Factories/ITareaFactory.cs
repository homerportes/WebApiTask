using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Factories
{
    using DomainLayer.Models;

    public interface ITareaFactory
    {
        Tareas CrearTareaAltaPrioridad(string descripcion);
        Tareas CrearTareaNormal(string descripcion);
    }
}
