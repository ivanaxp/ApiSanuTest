using SanuApi.Domain.Entities;

namespace SanuApi.Domain.Interfaces { 
    public interface ICustomerRepository
    {
        Task<IEnumerable<Entities.Customer>> GetAllAsync(bool? active);
        Task<Entities.Customer?> FindByIdAsync(int id);
        Task<int> AddAsync(Entities.Customer entity);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Entities.Classes>> GetClassesAsync(int id);
        Task<int> AddClassesAsync(ClassCustomer entity);
        Task<bool> UpdateAsync(Customer entity);
    }

}