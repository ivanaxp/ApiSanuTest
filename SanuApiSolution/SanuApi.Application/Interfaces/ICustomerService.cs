using SanuApi.Application.DTOs.Customer;

namespace SanuApi.Aplication.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerFindResponseDto>> GetAllAsync(bool? active);
        Task<CutsomerFindByIdResponseDto?> FindByIdAsync(int id);
        Task<int> AddAsync(CustomerAddRequestDto coustomer);
        Task<bool> UpdateAsync(CustomerUpdateRequestDto coustomer);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<ClassCustomerResponseDto>> GetClasses(int id);
        Task<bool> AddClassesAsync(int customerId, AddCustomerClassRequestDto classCustomer);
        Task<bool> AddMembershipAsync(int customerId, AddCustomerMembershipRequestDto membershipCustomer);
        Task<bool> AddAbsenceAsync(int customerId, AddCustomerAbsenceRequestDto absence);
        Task<IEnumerable<CustomerAbsenceResponseDto>> GetAbsencesAsync(int customerId);
    }
}