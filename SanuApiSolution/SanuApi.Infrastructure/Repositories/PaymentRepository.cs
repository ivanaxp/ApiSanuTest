using Dapper;
using SanuApi.Domain.Entities;
using SanuApi.Domain.Interfaces;
using System.Data;

namespace SanuApi.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly IDbConnection _db;

        public PaymentRepository(IDbConnection db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<int> AddAsync(Payment entity)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = @"
                INSERT INTO payment (customerid, periodmonth, periodyear, expectedamount, paidamount, paymentdate, note)
                VALUES (@CustomerId, @PeriodMonth, @PeriodYear, @ExpectedAmount, @PaidAmount, @PaymentDate, @Note)
                RETURNING id;";

            try
            {
                var id = await _db.ExecuteScalarAsync<int>(sql, new
                {
                    CustomerId = entity.customerid,
                    PeriodMonth = entity.periodmonth,
                    PeriodYear = entity.periodyear,
                    ExpectedAmount = entity.expectedamount,
                    PaidAmount = entity.paidamount,
                    PaymentDate = entity.paymentdate,
                    Note = entity.note
                });
                return id;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error al insertar el pago: {e.Message}", e);
            }
        }

        public async Task AddDetailsAsync(int paymentId, IEnumerable<PaymentDetail> details)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = @"
                INSERT INTO payment_detail (paymentid, membershipid, membershipname, membershipprice)
                VALUES (@PaymentId, @MembershipId, @MembershipName, @MembershipPrice);";

            try
            {
                foreach (var detail in details)
                {
                    await _db.ExecuteAsync(sql, new
                    {
                        PaymentId = paymentId,
                        MembershipId = detail.membershipid,
                        MembershipName = detail.membershipname,
                        MembershipPrice = detail.membershipprice
                    });
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error al insertar el detalle del pago: {e.Message}", e);
            }
        }

        public async Task<Payment?> FindByIdAsync(int id)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = "SELECT id, customerid, periodmonth, periodyear, expectedamount, paidamount, paymentdate, note FROM payment WHERE id = @Id";
            var payment = await _db.QuerySingleOrDefaultAsync<Payment>(sql, new { Id = id });
            if (payment == null)
                return null;

            var details = await LoadDetailsAsync(new[] { payment.id });
            payment.Details = details.TryGetValue(payment.id, out var list) ? list : new List<PaymentDetail>();
            return payment;
        }

        public async Task<bool> UpdateAsync(Payment entity)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = @"
                UPDATE payment
                SET periodmonth = @PeriodMonth,
                    periodyear  = @PeriodYear,
                    paidamount  = @PaidAmount,
                    note        = @Note
                WHERE id = @Id";

            var rows = await _db.ExecuteAsync(sql, new
            {
                PeriodMonth = entity.periodmonth,
                PeriodYear = entity.periodyear,
                PaidAmount = entity.paidamount,
                Note = entity.note,
                Id = entity.id
            });
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = "DELETE FROM payment WHERE id = @Id";
            var rows = await _db.ExecuteAsync(sql, new { Id = id });
            return rows > 0;
        }

        public async Task<IEnumerable<(Payment Payment, IEnumerable<PaymentDetail> Details)>> GetByCustomerAsync(int customerId)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = @"
                SELECT id, customerid, periodmonth, periodyear, expectedamount, paidamount, paymentdate, note
                FROM payment
                WHERE customerid = @CustomerId
                ORDER BY paymentdate DESC";

            var payments = (await _db.QueryAsync<Payment>(sql, new { CustomerId = customerId })).ToList();
            var detailsByPayment = await LoadDetailsAsync(payments.Select(p => p.id));

            return payments.Select(p => (
                p,
                (IEnumerable<PaymentDetail>)(detailsByPayment.TryGetValue(p.id, out var details) ? details : new List<PaymentDetail>())
            ));
        }

        public async Task<decimal> GetBalanceAsync(int customerId)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = "SELECT COALESCE(SUM(paidamount - expectedamount), 0) FROM payment WHERE customerid = @CustomerId";
            return await _db.ExecuteScalarAsync<decimal>(sql, new { CustomerId = customerId });
        }

        public async Task<IEnumerable<ActiveMembershipRow>> GetActiveMembershipsAsync(int customerId, DateTime asOfDate)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = @"
                SELECT m.id AS membershipid, m.name, m.price
                FROM customer_x_membership cm
                INNER JOIN membership m ON m.id = cm.membershipid
                WHERE cm.customerid = @CustomerId
                  AND cm.startdate <= @AsOfDate
                  AND (cm.enddate IS NULL OR cm.enddate > @AsOfDate)";

            return await _db.QueryAsync<ActiveMembershipRow>(sql, new { CustomerId = customerId, AsOfDate = asOfDate });
        }

        public async Task<IEnumerable<(Payment Payment, IEnumerable<PaymentDetail> Details)>> GetAllAsync(
            int? periodMonth, int? periodYear, string? estado, int? customerId, DateTime? desde, DateTime? hasta)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var conditions = new List<string>();
            var parameters = new DynamicParameters();

            if (periodMonth.HasValue)
            {
                conditions.Add("periodmonth = @PeriodMonth");
                parameters.Add("PeriodMonth", periodMonth.Value);
            }
            if (periodYear.HasValue)
            {
                conditions.Add("periodyear = @PeriodYear");
                parameters.Add("PeriodYear", periodYear.Value);
            }
            if (customerId.HasValue)
            {
                conditions.Add("customerid = @CustomerId");
                parameters.Add("CustomerId", customerId.Value);
            }
            if (desde.HasValue)
            {
                conditions.Add("paymentdate >= @Desde");
                parameters.Add("Desde", desde.Value);
            }
            if (hasta.HasValue)
            {
                conditions.Add("paymentdate <= @Hasta");
                parameters.Add("Hasta", hasta.Value);
            }
            if (!string.IsNullOrWhiteSpace(estado))
            {
                conditions.Add(@"
                    (CASE
                        WHEN paidamount < expectedamount THEN 'deuda'
                        WHEN paidamount > expectedamount THEN 'favor'
                        ELSE 'completo'
                    END) = @Estado");
                parameters.Add("Estado", estado);
            }

            var whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "";
            var sql = $@"
                SELECT id, customerid, periodmonth, periodyear, expectedamount, paidamount, paymentdate, note
                FROM payment
                {whereClause}
                ORDER BY paymentdate DESC";

            var payments = (await _db.QueryAsync<Payment>(sql, parameters)).ToList();
            var detailsByPayment = await LoadDetailsAsync(payments.Select(p => p.id));

            return payments.Select(p => (
                p,
                (IEnumerable<PaymentDetail>)(detailsByPayment.TryGetValue(p.id, out var details) ? details : new List<PaymentDetail>())
            ));
        }

        public async Task<MonthlySummaryRow> GetMonthlySummaryAsync(int periodMonth, int periodYear)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = @"
                SELECT
                    COALESCE(SUM(expectedamount), 0) AS totalexpected,
                    COALESCE(SUM(paidamount), 0) AS totalpaid,
                    COALESCE(SUM(CASE WHEN paidamount < expectedamount THEN expectedamount - paidamount ELSE 0 END), 0) AS totaldebt,
                    COALESCE(SUM(CASE WHEN paidamount > expectedamount THEN paidamount - expectedamount ELSE 0 END), 0) AS totalcredit,
                    COUNT(*) AS paymentcount,
                    COUNT(DISTINCT CASE WHEN paidamount < expectedamount THEN customerid END) AS customersindebtcount,
                    COUNT(DISTINCT CASE WHEN paidamount > expectedamount THEN customerid END) AS customerswithcreditcount
                FROM payment
                WHERE periodmonth = @PeriodMonth AND periodyear = @PeriodYear";

            return await _db.QuerySingleAsync<MonthlySummaryRow>(sql, new { PeriodMonth = periodMonth, PeriodYear = periodYear });
        }

        private async Task<Dictionary<int, List<PaymentDetail>>> LoadDetailsAsync(IEnumerable<int> paymentIds)
        {
            var ids = paymentIds.ToArray();
            if (!ids.Any())
                return new Dictionary<int, List<PaymentDetail>>();

            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = "SELECT id, paymentid, membershipid, membershipname, membershipprice FROM payment_detail WHERE paymentid = ANY(@Ids)";
            var details = await _db.QueryAsync<PaymentDetail>(sql, new { Ids = ids });

            return details
                .GroupBy(d => d.paymentid)
                .ToDictionary(g => g.Key, g => g.ToList());
        }
    }
}
