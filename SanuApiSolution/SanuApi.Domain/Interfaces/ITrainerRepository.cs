using SanuApi.Domain.Entities;

namespace SanuApi.Domain.Interfaces
{
    public interface ITrainerRepository
    {
        Task<IEnumerable<Trainer>> GetAllAsync();
        Task<int> AddAsync(Trainer entity);
        Task<bool> AddClassAsync(TrainerClasses entity);
        Task<IEnumerable<(Classes Class, IEnumerable<Customer> Students)>> GetClassesWithStudentsAsync(int trainerId);
        Task<Trainer?> FindByIdAsync(int id);
        Task<bool> UpdateAsync(Trainer entity);
        Task<bool> DeleteAsync(int id);
    }
}
