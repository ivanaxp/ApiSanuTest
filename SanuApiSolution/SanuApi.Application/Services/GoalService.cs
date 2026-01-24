
using SanuApi.Application.DTOs.Goal;
using SanuApi.Application.Interfaces;
using SanuApi.Domain.Interfaces;

namespace SanuApi.Application.Services
{
    public class GoalService : IGoalService
    {
        private readonly IGoalRepository _goalRepository;
        public GoalService(IGoalRepository goalRepository) { 
        
            _goalRepository = goalRepository;
        }
        public Task<int> AddAsync(AddGoalRequestDto coustomer)
        {
            var newGoal = new Domain.Entities.Goal
            {
                goalname = coustomer.GoalName,
                fechaBaja = null
            };
            return _goalRepository.AddAsync(newGoal);
        }

        public Task<bool> DeleteAsync(int id)
        {
            var goal = _goalRepository.FindByIdAsync(id);
            return goal.ContinueWith(t =>
            {
                var g = t;
                if (g == null) throw new InvalidOperationException("El objetivo no existe.");
                return _goalRepository.DeleteAsync(g.Result);
            }).Unwrap();
        }

        public Task<GoalFindResponseDto?> FindByIdAsync(int id)
        {
            var goal = _goalRepository.FindByIdAsync(id);
            return goal.ContinueWith(t =>
            {
                var g = t.Result;
                if (g == null) return null;
                return new GoalFindResponseDto
                {
                    Id = g.id,
                    GoalName = g.goalname
                };
            });
        }

        public async Task<IEnumerable<GoalFindResponseDto>> GetAllAsync()
        {
            var goals = await _goalRepository.GetAllAsync();
            return goals.Select(g => new GoalFindResponseDto
            {
                Id = g.id,
                GoalName = g.goalname,
                
            }); 
        }
    }
}
