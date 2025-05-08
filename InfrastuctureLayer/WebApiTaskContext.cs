using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InfrastuctureLayer
{
   public class WebApiTaskContext: DbContext
    {
        public WebApiTaskContext(DbContextOptions options):
            base(options)
      
        {

        }

        public DbSet<Tareas> Tarea { get; set; }
    }
}
