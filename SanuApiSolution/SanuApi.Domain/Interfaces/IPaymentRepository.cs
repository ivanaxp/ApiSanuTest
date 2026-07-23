using SanuApi.Domain.Entities;

namespace SanuApi.Domain.Interfaces
{
    public class ActiveMembershipRow
    {
        public int membershipid { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
    }

    public class MonthlySummaryRow
    {
        public decimal totalexpected { get; set; }
        public decimal totalpaid { get; set; }
        public decimal totaldebt { get; set; }
        public decimal totalcredit { get; set; }
        public int paymentcount { get; set; }
        public int customersindebtcount { get; set; }
        public int customerswithcreditcount { get; set; }
    }

    public interface IPaymentRepository
    {
        Task<int> AddAsync(Payment entity);
        Task AddDetailsAsync(int paymentId, IEnumerable<PaymentDetail> details);
        Task<Payment?> FindByIdAsync(int id);
        Task<bool> UpdateAsync(Payment entity);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<(Payment Payment, IEnumerable<PaymentDetail> Details)>> GetByCustomerAsync(int customerId);
        Task<decimal> GetBalanceAsync(int customerId);
        Task<IEnumerable<ActiveMembershipRow>> GetActiveMembershipsAsync(int customerId, DateTime asOfDate);
        Task<IEnumerable<(Payment Payment, IEnumerable<PaymentDetail> Details)>> GetAllAsync(
            int? periodMonth, int? periodYear, string? estado, int? customerId, DateTime? desde, DateTime? hasta);
        Task<MonthlySummaryRow> GetMonthlySummaryAsync(int periodMonth, int periodYear);
    }
}
