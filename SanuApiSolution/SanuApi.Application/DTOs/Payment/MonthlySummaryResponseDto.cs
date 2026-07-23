namespace SanuApi.Application.DTOs.Payment
{
    public class MonthlySummaryResponseDto
    {
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public decimal TotalExpected { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalDebt { get; set; }
        public decimal TotalCredit { get; set; }
        public int PaymentCount { get; set; }
        public int CustomersInDebtCount { get; set; }
        public int CustomersWithCreditCount { get; set; }
    }
}
