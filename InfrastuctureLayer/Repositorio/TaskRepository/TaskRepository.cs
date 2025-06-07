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

        public TaskRepository(WebApiTaskContext context) => _context = context;

        public async Task<IEnumerable<Tareas>> GetAllAsync()
            => await _context.Tarea.ToListAsync();

        public async Task<Tareas?> GetIdAsync(int id)
            => await _context.Tarea.FirstOrDefaultAsync(x => x.Id == id);

        public async Task<(bool IsSuccess, string Message)> AddAsync(Tareas entry)
        {
            try
            {
                await _context.Tarea.AddAsync(entry);
                await _context.SaveChangesAsync();
                return (true, "Tarea guardada correctamente");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return (false, $"Conflicto de concurrencia: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                return (false, $"Error al guardar en BD: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}");
            }
        }

        public async Task<(bool IsSuccess, string Message)> UpdateAsync(Tareas entry)
        {
            try
            {
                _context.Tarea.Update(entry);
                await _context.SaveChangesAsync();
                return (true, "Tarea actualizada correctamente");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return (false, $"Conflicto de concurrencia: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                return (false, $"Error al actualizar en BD: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}");
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
                return (true, "Tarea eliminada correctamente");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return (false, $"Conflicto de concurrencia: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                return (false, $"Error al eliminar en BD: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}");
            }
        }
    }
}
