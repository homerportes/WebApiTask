using System.Collections.Generic;
using System.Threading.Tasks;

namespace InfrastuctureLayer.Repositorio.Commons
{
    public interface ICommonsProcess<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetIdAsync(int id);   
        Task<(bool IsSuccess, string Message)> AddAsync(T entry);
        Task<(bool IsSuccess, string Message)> UpdateAsync(T entry);
        Task<(bool IsSuccess, string Message)> DeleteAsync(int id);
    }
}
