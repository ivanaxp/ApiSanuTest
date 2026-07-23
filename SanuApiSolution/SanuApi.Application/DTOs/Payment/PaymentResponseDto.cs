namespace SanuApi.Application.DTOs.Payment
{
    public class PaymentResponseDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public decimal ExpectedAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? Note { get; set; }
        public string Estado { get; set; }
        public List<PaymentDetailResponseDto> Details { get; set; } = new();
    }
}
