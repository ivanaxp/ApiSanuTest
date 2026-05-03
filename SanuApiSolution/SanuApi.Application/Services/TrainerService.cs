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
            var trainer = new Trainer
            {
                name = dto.TrainerName,
                lastName = dto.TrainerLastName,
                email = dto.Email,
                telephone = dto.Telephone
            };
            return await _trainerRepository.AddAsync(trainer);
        }

        public async Task<int> AddClassesAsync(int trainerId, List<int> classIds)
        {
            if (!classIds.Any())
                throw new ArgumentException("Debe especificar al menos una clase.");

            var count = 0;
            foreach (var classId in classIds)
            {
                var trainerClass = new TrainerClasses
                {
                    idtrainer = trainerId,
                    idclass = classId
                };
                var inserted = await _trainerRepository.AddClassAsync(trainerClass);
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
                Day = r.Class.day,
                Hour = r.Class.hour,
                Capacity = r.Class.capacity,
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
