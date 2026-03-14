using SanuApi.Domain.Entities;

namespace SanuApi.Domain.Interfaces
{
    public interface ICustomerMembershipRepository
    {
        Task<int> AddAsync(CustomerMembership entity);
        Task<bool> DeleteByCustomerIdAsync(int customerId);
    }
}
