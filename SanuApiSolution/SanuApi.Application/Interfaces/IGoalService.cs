using SanuApi.Application.DTOs.Goal;

namespace SanuApi.Application.Interfaces
{
    public interface IGoalService
    {
        Task<IEnumerable<GoalFindResponseDto>> GetAllAsync();
        Task<GoalFindResponseDto?> FindByIdAsync(int id);
        Task<int> AddAsync(AddGoalRequestDto coustomer);
        Task<bool> DeleteAsync(int id);
    }
}
