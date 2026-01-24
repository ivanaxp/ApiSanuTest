using SanuApi.Domain.Entities;

namespace SanuApi.Domain.Interfaces
{
    public interface IMembershipRepository
    {
        Task<int> AddAsync(Membership entity);
        Task<bool> UpdateAsync(Membership entity);
        Task<Membership?> FindByIdAsync(int id);
        Task<IEnumerable<Membership>> GetAllAsync();
        Task<bool> DeleteAsync(int idMembership);
    }
}
