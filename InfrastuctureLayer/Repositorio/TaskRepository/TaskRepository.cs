using DomainLayer.Models;
using InfrastuctureLayer.Repositorio.Commons;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InfrastuctureLayer
{
    public class TaskRepository : ICommonsProcess<Tareas>
    {
        private readonly WebApiTaskContext _context;

        public TaskRepository(WebApiTaskContext webApiTaskContext)
            => _context = webApiTaskContext;

        public async Task<IEnumerable<Tareas>> GetAllAsync()
            => await _context.Tarea.ToListAsync();

        public async Task<Tareas> GetIdAsync(int id)
            => await _context.Tarea.FirstOrDefaultAsync(x => x.Id == id);

        public async Task<(bool IsSuccess, string Message)> AddAsync(Tareas entry)
        {
            try
            {
                await _context.Tarea.AddAsync(entry);
                await _context.SaveChangesAsync();
                return (true, "Tarea guardada");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool IsSuccess, string Message)> UpdateAsync(Tareas entry)
        {
            try
            {
                _context.Tarea.Update(entry);
                await _context.SaveChangesAsync();
                return (true, "La tarea se actualizó correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool IsSuccess, string Message)> DeleteAsync(int id)
        {
            try
            {
                var tarea = await _context.Tarea.FindAsync(id);
                if (tarea == null)
                    return (false, "La tarea no existe");
                _context.Tarea.Remove(tarea);
                await _context.SaveChangesAsync();
                return (true, "La tarea se eliminó correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }
    }
}
