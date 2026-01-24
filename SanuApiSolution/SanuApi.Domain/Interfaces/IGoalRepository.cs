using SanuApi.Domain.Entities;

namespace SanuApi.Domain.Interfaces
{
    public interface IGoalRepository
    {
        Task<IEnumerable<Goal>> GetAllAsync();
        Task<Goal?> FindByIdAsync(int id);
        Task<int> AddAsync(Goal entity);
        Task<int> AddCustomerGoalAsync(CustomerGoal entity);
        Task<bool> DeleteAsync(Goal entity);
    }
}
