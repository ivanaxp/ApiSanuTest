
using SanuApi.Domain.Entities;

namespace SanuApi.Domain.Interfaces
{
    public interface IHealthCustomerRepository
    {
        Task<int> AddAsync(HealthCustomer entity);
        Task<bool> UpsertAsync(HealthCustomer entity);
    }
}
