using SanuApi.Application.DTOs.Trainer;

namespace SanuApi.Application.Interfaces
{
    public interface ITrainerService
    {
        Task<IEnumerable<TrainerResponseDto>> GetAllAsync();
        Task<int> AddAsync(TrainerAddRequestDto dto);
        Task<int> AddClassDatesAsync(int trainerId, int classId, List<int> classDateIds);
        Task<IEnumerable<TrainerClassWithStudentsResponseDto>> GetClassesWithStudentsAsync(int trainerId);
        Task<bool> UpdateAsync(int trainerId, TrainerUpdateRequestDto dto);
        Task<bool> DeleteAsync(int trainerId);
    }
}
