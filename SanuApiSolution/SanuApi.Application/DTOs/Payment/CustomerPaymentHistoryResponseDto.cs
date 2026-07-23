namespace SanuApi.Application.DTOs.Payment
{
    public class CustomerPaymentHistoryResponseDto
    {
        public int CustomerId { get; set; }
        public decimal Balance { get; set; }
        public List<PaymentResponseDto> Payments { get; set; } = new();
    }
}
