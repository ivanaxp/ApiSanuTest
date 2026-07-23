using SanuApi.Domain.Entities;

namespace SanuApi.Domain.Interfaces
{
    public interface IClassRepository
    {
        Task<IEnumerable<Classes>> GetAllAsync();
        Task<Classes?> FindByIdAsync(int id);
        Task<(Classes? Class, IEnumerable<(Customer Customer, int? ClassDateId, string? Day, string? Hour)> Customers)> GetWithCustomersAsync(int classId);
        Task<int> AddAsync(Classes entity);
        Task AddDatesAsync(int classId, IEnumerable<ClassDate> dates);
        Task ReplaceDatesAsync(int classId, IEnumerable<ClassDate> dates);
        Task AddMembershipsAsync(int classId, IEnumerable<int> membershipIds);
        Task ReplaceMembershipsAsync(int classId, IEnumerable<int> membershipIds);
        Task<bool> UpdateAsync(Classes entity);
        Task<int> AddCustomerClassesAsync(ClassCustomer entity);
        Task<bool> DeleteAsync(Classes entity);
        Task<(Classes? Class, IEnumerable<(Customer Customer, string? Status)> Students)> GetAttendanceByDateAsync(int classId, DateTime date);
        Task<IEnumerable<(int AttendanceId, DateTime Date, string Status, Customer Customer)>> GetAttendanceRecordsAsync(int classId);
        Task<IEnumerable<Classes>> GetByMembershipIdAsync(int membershipId);
    }
}
