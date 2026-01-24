using SanuApi.Application.DTOs.Class;
using SanuApi.Application.Interfaces;
using SanuApi.Domain.Interfaces;

namespace SanuApi.Application.Services
{
    public class ClassService : IClassService
    {
        private readonly IClassRepository _classRepository;
        public ClassService(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }
        public async Task<int> AddAsync(AddClassRequestDto coustomer)
        {
            var newClasses = new Domain.Entities.Classes
            {
                day= coustomer.Day,
                hour= coustomer.Hour,
                name= coustomer.Name,
                capacity= coustomer.Capacity
            };
            return await _classRepository.AddAsync(newClasses);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var classes = _classRepository.FindByIdAsync(id);
            return await classes.ContinueWith(t =>
            {
                var g = t;
                if (g == null) throw new InvalidOperationException("El objetivo no existe.");
                return _classRepository.DeleteAsync(g.Result);
            }).Unwrap();
        }

        public async Task<ClassFindResponseDto?> FindByIdAsync(int id)
        {
            var clase = _classRepository.FindByIdAsync(id);
            return await clase.ContinueWith(t =>
            {
                var g = t.Result;
                if (g == null) return null;
                return new ClassFindResponseDto
                {
                   Id = g.id,
                   Capacity= g.capacity,
                   Day= g.day,
                   Hour= g.hour,
                   Name= g.name
                };
            });
        }

        public async Task<IEnumerable<ClassFindResponseDto>> GetAllAsync()
        {
            var classes = await _classRepository.GetAllAsync();
            return classes.Select(g => new ClassFindResponseDto
            {
                Id = g.id,
                Capacity = g.capacity,
                Day = g.day,
                Hour = g.hour,
                Name = g.name
            });
        }
    }
}
