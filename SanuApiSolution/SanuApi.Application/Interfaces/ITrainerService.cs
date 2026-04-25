using SanuApi.Application.DTOs.Trainer;

namespace SanuApi.Application.Interfaces
{
    public interface ITrainerService
    {
        Task<int> AddAsync(TrainerAddRequestDto dto);
        Task<int> AddClassesAsync(int trainerId, List<int> classIds);
        Task<IEnumerable<TrainerClassWithStudentsResponseDto>> GetClassesWithStudentsAsync(int trainerId);
        Task<bool> UpdateAsync(int trainerId, TrainerUpdateRequestDto dto);
        Task<bool> DeleteAsync(int trainerId);
    }
}
