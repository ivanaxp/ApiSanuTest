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

        public async Task<ClassWithCustomersResponseDto?> GetWithCustomersAsync(int classId)
        {
            var (clase, customers) = await _classRepository.GetWithCustomersAsync(classId);
            if (clase == null) return null;

            return new ClassWithCustomersResponseDto
            {
                ClassId = clase.id,
                ClassName = clase.name,
                Day = clase.day,
                Hour = clase.hour,
                Capacity = clase.capacity,
                Customers = customers.Select(c => new CustomerInClassDto
                {
                    CustomerId = c.id,
                    CustomerName = c.customername,
                    CustomerLastName = c.customerlastname,
                    Celphone = c.celphone
                }).ToList()
            };
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

        public async Task<ClassAttendanceResponseDto?> GetAttendanceByDateAsync(int classId, DateTime date)
        {
            var (cls, students) = await _classRepository.GetAttendanceByDateAsync(classId, date);
            if (cls == null) return null;

            var studentList = students.Select(s => new StudentAttendanceDto
            {
                CustomerId = s.Customer.id,
                CustomerName = s.Customer.customername,
                CustomerLastName = s.Customer.customerlastname,
                Status = s.Status
            }).ToList();

            return new ClassAttendanceResponseDto
            {
                ClassId = cls.id,
                ClassName = cls.name,
                Date = date.Date,
                Capacity = cls.capacity,
                TotalEnrolled = studentList.Count,
                FreeSpotsToday = studentList.Count(s => s.Status == "ausente_justificado"),
                Students = studentList
            };
        }
    }
}
