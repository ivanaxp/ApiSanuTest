using SanuApi.Domain.Entities;

namespace SanuApi.Domain.Interfaces
{
    public interface IClassRepository
    {
        Task<IEnumerable<Classes>> GetAllAsync();
        Task<Classes?> FindByIdAsync(int id);
        Task<(Classes? Class, IEnumerable<Customer> Customers)> GetWithCustomersAsync(int classId);
        Task<int> AddAsync(Classes entity);
        Task<int> AddCustomerClassesAsync(ClassCustomer entity);
        Task<bool> DeleteAsync(Classes entity);
        Task<(Classes? Class, IEnumerable<(Customer Customer, string? Status)> Students)> GetAttendanceByDateAsync(int classId, DateTime date);
    }
}
