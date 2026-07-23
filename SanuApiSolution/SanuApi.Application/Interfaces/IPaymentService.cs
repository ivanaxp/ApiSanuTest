using SanuApi.Application.DTOs.Payment;

namespace SanuApi.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<int> AddAsync(PaymentAddRequestDto dto);
        Task<bool> UpdateAsync(int paymentId, PaymentUpdateRequestDto dto);
        Task<bool> DeleteAsync(int paymentId);
        Task<CustomerPaymentHistoryResponseDto> GetCustomerHistoryAsync(int customerId);
        Task<CustomerBalanceResponseDto> GetCustomerBalanceAsync(int customerId);
        Task<IEnumerable<PaymentResponseDto>> GetAllAsync(int? periodMonth, int? periodYear, string? estado, int? customerId, DateTime? desde, DateTime? hasta);
        Task<MonthlySummaryResponseDto> GetMonthlySummaryAsync(int? periodMonth, int? periodYear);
    }
}
