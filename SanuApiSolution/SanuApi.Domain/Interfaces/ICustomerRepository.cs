using SanuApi.Domain.Entities;

namespace SanuApi.Domain.Interfaces { 
    public interface ICustomerRepository
    {
        Task<IEnumerable<Entities.Customer>> GetAllAsync(bool? active);
        Task<Entities.Customer?> FindByIdAsync(int id);
        Task<int> AddAsync(Entities.Customer entity);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Classes>> GetClassesAsync(int id);
        Task<int> AddClassesAsync(ClassCustomer entity);
        Task<bool> DeleteClassesAsync(int customerId);
        Task<bool> DeleteGoalsAsync(int customerId);
        Task<bool> UpdateAsync(Customer entity);
        Task<int> AddAbsenceAsync(Absences entity);
    }
}