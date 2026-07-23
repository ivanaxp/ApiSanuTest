using SanuApi.Application.DTOs.Class;
using SanuApi.Application.DTOs.Trainer;
using SanuApi.Application.Interfaces;
using SanuApi.Domain.Entities;
using SanuApi.Domain.Interfaces;

namespace SanuApi.Application.Services
{
    public class TrainerService : ITrainerService
    {
        private readonly ITrainerRepository _trainerRepository;

        public TrainerService(ITrainerRepository trainerRepository)
        {
            _trainerRepository = trainerRepository;
        }

        public async Task<IEnumerable<TrainerResponseDto>> GetAllAsync()
        {
            var trainers = await _trainerRepository.GetAllAsync();
            return trainers.Select(t => new TrainerResponseDto
            {
                TrainerId = t.id,
                TrainerName = t.name,
                TrainerLastName = t.lastName,
                Email = t.email,
                Telephone = t.telephone
            });
        }

        public async Task<int> AddAsync(TrainerAddRequestDto dto)
        {
            var hasClassDates = dto.ClassDateIds != null && dto.ClassDateIds.Any();
            if (hasClassDates)
            {
                if (dto.ClassId == null)
                    throw new ArgumentException("Debe especificar ClassId junto con ClassDateIds.");
                await ValidateClassDateIdsAsync(dto.ClassId.Value, dto.ClassDateIds!);
            }

            var trainer = new Trainer
            {
                name = dto.TrainerName,
                lastName = dto.TrainerLastName,
                email = dto.Email,
                telephone = dto.Telephone
            };
            var id = await _trainerRepository.AddAsync(trainer);

            if (hasClassDates)
                await AssignClassDatesAsync(id, dto.ClassDateIds!);

            return id;
        }

        public async Task<int> AddClassDatesAsync(int trainerId, int classId, List<int> classDateIds)
        {
            await ValidateClassDateIdsAsync(classId, classDateIds);
            return await AssignClassDatesAsync(trainerId, classDateIds);
        }

        private async Task ValidateClassDateIdsAsync(int classId, List<int> classDateIds)
        {
            if (!classDateIds.Any())
                throw new ArgumentException("Debe especificar al menos un horario.");

            var existingIds = (await _trainerRepository.GetExistingClassDateIdsForClassAsync(classId, classDateIds)).ToHashSet();
            var invalidIds = classDateIds.Where(id => !existingIds.Contains(id)).ToList();
            if (invalidIds.Any())
                throw new ArgumentException($"Los siguientes horarios no pertenecen a la clase {classId} o no existen: {string.Join(", ", invalidIds)}");
        }

        private async Task<int> AssignClassDatesAsync(int trainerId, List<int> classDateIds)
        {
            var count = 0;
            foreach (var classDateId in classDateIds)
            {
                var trainerClassDate = new TrainerClassDate
                {
                    idtrainer = trainerId,
                    idclassdate = classDateId
                };
                var inserted = await _trainerRepository.AddClassDateAsync(trainerClassDate);
                if (inserted) count++;
            }
            return count;
        }

        public async Task<IEnumerable<TrainerClassWithStudentsResponseDto>> GetClassesWithStudentsAsync(int trainerId)
        {
            var results = await _trainerRepository.GetClassesWithStudentsAsync(trainerId);

            return results.Select(r => new TrainerClassWithStudentsResponseDto
            {
                ClassId = r.Class.id,
                ClassName = r.Class.name,
                Dates = r.Class.Dates.Select(d => new ClassDateResponseDto
                {
                    Day = d.day,
                    Hour = d.hour,
                    Capacity = d.capacity
                }).ToList(),
                Students = r.Students.Select(s => new StudentInClassDto
                {
                    CustomerId = s.id,
                    CustomerName = s.customername,
                    CustomerLastName = s.customerlastname
                }).ToList()
            });
        }

        public async Task<bool> UpdateAsync(int trainerId, TrainerUpdateRequestDto dto)
        {
            var trainer = await _trainerRepository.FindByIdAsync(trainerId);
            if (trainer == null) return false;

            if (!string.IsNullOrWhiteSpace(dto.TrainerName))
                trainer.name = dto.TrainerName;

            if (!string.IsNullOrWhiteSpace(dto.TrainerLastName))
                trainer.lastName = dto.TrainerLastName;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                trainer.email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.Telephone))
                trainer.telephone = dto.Telephone;

            return await _trainerRepository.UpdateAsync(trainer);
        }

        public async Task<bool> DeleteAsync(int trainerId)
        {
            var trainer = await _trainerRepository.FindByIdAsync(trainerId);
            if (trainer == null) return false;

            return await _trainerRepository.DeleteAsync(trainerId);
        }
    }
}
