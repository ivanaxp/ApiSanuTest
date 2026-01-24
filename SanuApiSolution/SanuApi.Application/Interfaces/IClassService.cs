using SanuApi.Application.DTOs.Class;

namespace SanuApi.Application.Interfaces
{
    public interface IClassService
    {
        Task<IEnumerable<ClassFindResponseDto>> GetAllAsync();
        Task<ClassFindResponseDto?> FindByIdAsync(int id);
        Task<int> AddAsync(AddClassRequestDto coustomer);
        Task<bool> DeleteAsync(int id);
    }
}
