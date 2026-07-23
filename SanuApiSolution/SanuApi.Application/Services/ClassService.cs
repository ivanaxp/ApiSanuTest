using SanuApi.Application.DTOs.Class;
using SanuApi.Application.Interfaces;
using SanuApi.Domain.Entities;
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

        public async Task<int> AddAsync(AddClassRequestDto dto)
        {
            var newClass = new Classes
            {
                name = dto.Name
            };
            var classId = await _classRepository.AddAsync(newClass);

            if (dto.Dates?.Count > 0)
            {
                var dates = dto.Dates.Select(d => new ClassDate
                {
                    idclass = classId,
                    day = d.Day,
                    hour = d.Hour,
                    capacity = d.Capacity
                });
                await _classRepository.AddDatesAsync(classId, dates);
            }

            if (dto.MembershipIds?.Count > 0)
                await _classRepository.AddMembershipsAsync(classId, dto.MembershipIds);

            return classId;
        }

        public async Task<bool> UpdateAsync(UpdateClassRequestDto dto)
        {
            var entity = new Classes
            {
                id = dto.Id,
                name = dto.Name
            };
            var updated = await _classRepository.UpdateAsync(entity);
            if (!updated) return false;

            var dates = (dto.Dates ?? new()).Select(d => new ClassDate
            {
                idclass = dto.Id,
                day = d.Day,
                hour = d.Hour,
                capacity = d.Capacity
            });
            await _classRepository.ReplaceDatesAsync(dto.Id, dates);

            await _classRepository.ReplaceMembershipsAsync(dto.Id, dto.MembershipIds ?? new());
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var cls = await _classRepository.FindByIdAsync(id);
            if (cls == null) throw new InvalidOperationException("La clase no existe.");
            return await _classRepository.DeleteAsync(cls);
        }

        public async Task<ClassFindResponseDto?> FindByIdAsync(int id)
        {
            var cls = await _classRepository.FindByIdAsync(id);
            if (cls == null) return null;
            return MapToDto(cls);
        }

        public async Task<IEnumerable<ClassFindResponseDto>> GetAllAsync()
        {
            var classes = await _classRepository.GetAllAsync();
            return classes.Select(MapToDto);
        }

        public async Task<ClassWithCustomersResponseDto?> GetWithCustomersAsync(int classId)
        {
            var (cls, customers) = await _classRepository.GetWithCustomersAsync(classId);
            if (cls == null) return null;

            return new ClassWithCustomersResponseDto
            {
                ClassId = cls.id,
                ClassName = cls.name,
                MembershipIds = cls.Memberships.Select(m => m.id).ToList(),
                Dates = cls.Dates.Select(MapDateToDto).ToList(),
                Customers = customers.Select(c => new CustomerInClassDto
                {
                    CustomerId = c.Customer.id,
                    CustomerName = c.Customer.customername,
                    CustomerLastName = c.Customer.customerlastname,
                    Celphone = c.Customer.celphone,
                    ClassDateId = c.ClassDateId,
                    Day = c.Day,
                    Hour = c.Hour
                }).ToList()
            };
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
                Capacity = GetCapacityForDate(cls, date),
                TotalEnrolled = studentList.Count,
                FreeSpotsToday = studentList.Count(s => s.Status == "ausente_justificado"),
                Students = studentList
            };
        }

        public async Task<IEnumerable<ClassAttendanceRecordResponseDto>> GetAttendanceRecordsAsync(int classId)
        {
            var records = await _classRepository.GetAttendanceRecordsAsync(classId);
            return records.Select(r => new ClassAttendanceRecordResponseDto
            {
                AttendanceId = r.AttendanceId,
                Date = r.Date,
                Status = r.Status,
                Customer = new AttendanceCustomerDto
                {
                    CustomerId = r.Customer.id,
                    CustomerName = r.Customer.customername,
                    CustomerLastName = r.Customer.customerlastname,
                    Celphone = r.Customer.celphone
                }
            });
        }

        private static ClassFindResponseDto MapToDto(Classes cls) => new()
        {
            Id = cls.id,
            Name = cls.name,
            MembershipIds = cls.Memberships.Select(m => m.id).ToList(),
            Dates = cls.Dates.Select(MapDateToDto).ToList()
        };

        private static ClassDateResponseDto MapDateToDto(ClassDate d) => new()
        {
            Id = d.id,
            Day = d.day,
            Hour = d.hour,
            Capacity = d.capacity
        };

        // Busca la capacidad del día que corresponde a la fecha de la consulta.
        // Si el campo day no coincide con los nombres en español, usa el primero disponible.
        private static int GetCapacityForDate(Classes cls, DateTime date)
        {
            if (cls.Dates == null || cls.Dates.Count == 0) return 0;
            var spanishDays = new[] { "Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };
            var dayName = spanishDays[(int)date.DayOfWeek];
            var match = cls.Dates.FirstOrDefault(d => string.Equals(d.day, dayName, StringComparison.OrdinalIgnoreCase));
            return match?.capacity ?? cls.Dates[0].capacity;
        }
    }
}
