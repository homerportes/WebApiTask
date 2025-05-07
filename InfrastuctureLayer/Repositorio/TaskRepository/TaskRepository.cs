using DomainLayer.Models;
using InfrastuctureLayer.Repositorio.Commons;
using InfrastuctureLayer;
using Microsoft.EntityFrameworkCore;

public class TaskRepository : ICommonsProcess<Tareas>
{
    private readonly WebApiTaskContext _context;

    public TaskRepository(WebApiTaskContext webApiTaskContext)
    {
        _context = webApiTaskContext;
    }

    public async Task<IEnumerable<Tareas>> GetAllAsync()
        => await _context.Tarea.ToListAsync();

    public async Task<Tareas> GetIdAsync(int id)
        => await _context.Tarea.FirstOrDefaultAsync(x=>x.Id ==id);

    public async Task<(bool IsSuccess, string Message)> AddAsync(Tareas entry)
    {
        try 
        {
            await _context.Tarea.AddAsync(entry);
            await _context.SaveChangesAsync();
            return (true, "la tarea se guardo bien.");

        }

        catch (Exception)
        {
            return (false, "La tarea no se guardo");
        }
    }


    public async Task<(bool IsSuccess, string Message)> UpdateAsync(Tareas entry)
    {

        try
        {
            _context.Tarea.Update(entry);
            await _context.SaveChangesAsync();
            return (true, "la tarea se actualizo bien.");

        }

        catch (Exception)
        {
            return (false, "La tarea no se actualizo");
        }
    }
    public async Task<(bool IsSuccess, string Message)> DeleteAsync(int id)
    {

        try
        {
            var tarea = await _context.Tarea.FindAsync(id);

            if (tarea != null)
            {
                _context.Tarea.Remove(tarea);
                await _context.SaveChangesAsync();
                return (true, "la tarea se elimino bien.");
            }

            else
            {
                return (false, "la tarea no existe.");
            }
        }

        catch (Exception)
        {
            return (false, "La tarea no se elimino");
        }

    }


}
