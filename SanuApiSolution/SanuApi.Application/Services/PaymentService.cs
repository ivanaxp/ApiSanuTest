using SanuApi.Application.DTOs.Payment;
using SanuApi.Application.Interfaces;
using SanuApi.Domain.Entities;
using SanuApi.Domain.Interfaces;

namespace SanuApi.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private static readonly string[] ValidEstados = { "deuda", "favor", "completo" };

        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<int> AddAsync(PaymentAddRequestDto dto)
        {
            ValidatePeriodMonth(dto.PeriodMonth);

            var paymentDate = dto.PaymentDate ?? DateTime.UtcNow;
            var activeMemberships = (await _paymentRepository.GetActiveMembershipsAsync(dto.CustomerId, paymentDate)).ToList();
            var expectedAmount = activeMemberships.Sum(m => m.price);

            var payment = new Payment
            {
                customerid = dto.CustomerId,
                periodmonth = dto.PeriodMonth,
                periodyear = dto.PeriodYear,
                expectedamount = expectedAmount,
                paidamount = dto.PaidAmount,
                paymentdate = paymentDate,
                note = dto.Note
            };
            var id = await _paymentRepository.AddAsync(payment);

            if (activeMemberships.Any())
            {
                var details = activeMemberships.Select(m => new PaymentDetail
                {
                    paymentid = id,
                    membershipid = m.membershipid,
                    membershipname = m.name,
                    membershipprice = m.price
                });
                await _paymentRepository.AddDetailsAsync(id, details);
            }

            return id;
        }

        public async Task<bool> UpdateAsync(int paymentId, PaymentUpdateRequestDto dto)
        {
            var payment = await _paymentRepository.FindByIdAsync(paymentId);
            if (payment == null) return false;

            if (dto.PeriodMonth.HasValue)
            {
                ValidatePeriodMonth(dto.PeriodMonth.Value);
                payment.periodmonth = dto.PeriodMonth.Value;
            }

            if (dto.PeriodYear.HasValue)
                payment.periodyear = dto.PeriodYear.Value;

            if (dto.PaidAmount.HasValue)
                payment.paidamount = dto.PaidAmount.Value;

            if (!string.IsNullOrWhiteSpace(dto.Note))
                payment.note = dto.Note;

            return await _paymentRepository.UpdateAsync(payment);
        }

        public async Task<bool> DeleteAsync(int paymentId)
        {
            var payment = await _paymentRepository.FindByIdAsync(paymentId);
            if (payment == null) return false;

            return await _paymentRepository.DeleteAsync(paymentId);
        }

        public async Task<CustomerPaymentHistoryResponseDto> GetCustomerHistoryAsync(int customerId)
        {
            var results = await _paymentRepository.GetByCustomerAsync(customerId);
            var balance = await _paymentRepository.GetBalanceAsync(customerId);

            return new CustomerPaymentHistoryResponseDto
            {
                CustomerId = customerId,
                Balance = balance,
                Payments = results.Select(r => ToResponseDto(r.Payment, r.Details)).ToList()
            };
        }

        public async Task<CustomerBalanceResponseDto> GetCustomerBalanceAsync(int customerId)
        {
            var balance = await _paymentRepository.GetBalanceAsync(customerId);
            return new CustomerBalanceResponseDto { CustomerId = customerId, Balance = balance };
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetAllAsync(int? periodMonth, int? periodYear, string? estado, int? customerId, DateTime? desde, DateTime? hasta)
        {
            if (!string.IsNullOrWhiteSpace(estado) && !ValidEstados.Contains(estado, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"El estado '{estado}' no es válido. Valores permitidos: {string.Join(", ", ValidEstados)}.");

            var results = await _paymentRepository.GetAllAsync(periodMonth, periodYear, estado, customerId, desde, hasta);
            return results.Select(r => ToResponseDto(r.Payment, r.Details));
        }

        public async Task<MonthlySummaryResponseDto> GetMonthlySummaryAsync(int? periodMonth, int? periodYear)
        {
            var now = DateTime.UtcNow;
            var month = periodMonth ?? now.Month;
            var year = periodYear ?? now.Year;

            var row = await _paymentRepository.GetMonthlySummaryAsync(month, year);

            return new MonthlySummaryResponseDto
            {
                PeriodMonth = month,
                PeriodYear = year,
                TotalExpected = row.totalexpected,
                TotalPaid = row.totalpaid,
                TotalDebt = row.totaldebt,
                TotalCredit = row.totalcredit,
                PaymentCount = row.paymentcount,
                CustomersInDebtCount = row.customersindebtcount,
                CustomersWithCreditCount = row.customerswithcreditcount
            };
        }

        private static void ValidatePeriodMonth(int periodMonth)
        {
            if (periodMonth < 1 || periodMonth > 12)
                throw new ArgumentException("PeriodMonth debe estar entre 1 y 12.");
        }

        private static PaymentResponseDto ToResponseDto(Payment payment, IEnumerable<PaymentDetail> details)
        {
            var estado = payment.paidamount < payment.expectedamount
                ? "deuda"
                : payment.paidamount > payment.expectedamount
                    ? "favor"
                    : "completo";

            return new PaymentResponseDto
            {
                Id = payment.id,
                CustomerId = payment.customerid,
                PeriodMonth = payment.periodmonth,
                PeriodYear = payment.periodyear,
                ExpectedAmount = payment.expectedamount,
                PaidAmount = payment.paidamount,
                PaymentDate = payment.paymentdate,
                Note = payment.note,
                Estado = estado,
                Details = details.Select(d => new PaymentDetailResponseDto
                {
                    MembershipId = d.membershipid,
                    MembershipName = d.membershipname,
                    MembershipPrice = d.membershipprice
                }).ToList()
            };
        }
    }
}
