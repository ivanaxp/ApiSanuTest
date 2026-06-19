using SanuApi.Application.DTOs.Class;

namespace SanuApi.Application.Interfaces
{
    public interface IClassService
    {
        Task<IEnumerable<ClassFindResponseDto>> GetAllAsync();
        Task<ClassFindResponseDto?> FindByIdAsync(int id);
        Task<ClassWithCustomersResponseDto?> GetWithCustomersAsync(int classId);
        Task<int> AddAsync(AddClassRequestDto dto);
        Task<bool> UpdateAsync(UpdateClassRequestDto dto);
        Task<bool> DeleteAsync(int id);
        Task<ClassAttendanceResponseDto?> GetAttendanceByDateAsync(int classId, DateTime date);
        Task<IEnumerable<ClassAttendanceRecordResponseDto>> GetAttendanceRecordsAsync(int classId);
    }
}
