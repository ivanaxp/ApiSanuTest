
using SanuApi.Domain.Entities;

namespace SanuApi.Domain.Interfaces
{
    public interface IClassRepository
    {
        Task<IEnumerable<Classes>> GetAllAsync();
        Task<Classes?> FindByIdAsync(int id);
        Task<int> AddAsync(Classes entity);
        Task<int> AddCustomerClassesAsync(ClassCustomer entity);
        Task<bool> DeleteAsync(Classes entity);
    }
}
