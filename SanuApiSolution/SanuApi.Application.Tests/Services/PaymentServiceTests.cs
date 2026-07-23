using Moq;
using NUnit.Framework;
using SanuApi.Application.DTOs.Payment;
using SanuApi.Application.Services;
using SanuApi.Domain.Entities;
using SanuApi.Domain.Interfaces;

namespace SanuApi.Application.Tests.Services;

[TestFixture]
public class PaymentServiceTests
{
    private Mock<IPaymentRepository> _repoMock;
    private PaymentService _service;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IPaymentRepository>();
        _service = new PaymentService(_repoMock.Object);
    }

    // ─── AddAsync ─────────────────────────────────────────────────────────────

    [Test]
    public async Task AddAsync_WithActiveMemberships_ComputesExpectedAmountAndCreatesDetails()
    {
        var dto = new PaymentAddRequestDto { CustomerId = 1, PeriodMonth = 7, PeriodYear = 2026, PaidAmount = 100m };
        var activeMemberships = new List<ActiveMembershipRow>
        {
            new ActiveMembershipRow { membershipid = 10, name = "Musculacion", price = 60m },
            new ActiveMembershipRow { membershipid = 11, name = "Yoga", price = 40m }
        };
        _repoMock.Setup(r => r.GetActiveMembershipsAsync(1, It.IsAny<DateTime>())).ReturnsAsync(activeMemberships);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Payment>())).ReturnsAsync(5);

        var id = await _service.AddAsync(dto);

        Assert.That(id, Is.EqualTo(5));
        _repoMock.Verify(r => r.AddAsync(It.Is<Payment>(p => p.expectedamount == 100m && p.customerid == 1 && p.paidamount == 100m)), Times.Once);
        _repoMock.Verify(r => r.AddDetailsAsync(5, It.Is<IEnumerable<PaymentDetail>>(d => d.Count() == 2)), Times.Once);
    }

    [Test]
    public async Task AddAsync_WithoutActiveMemberships_ExpectedAmountIsZeroAndNoDetailsCreated()
    {
        var dto = new PaymentAddRequestDto { CustomerId = 1, PeriodMonth = 7, PeriodYear = 2026, PaidAmount = 100m };
        _repoMock.Setup(r => r.GetActiveMembershipsAsync(1, It.IsAny<DateTime>())).ReturnsAsync(new List<ActiveMembershipRow>());
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Payment>())).ReturnsAsync(5);

        var id = await _service.AddAsync(dto);

        Assert.That(id, Is.EqualTo(5));
        _repoMock.Verify(r => r.AddAsync(It.Is<Payment>(p => p.expectedamount == 0m)), Times.Once);
        _repoMock.Verify(r => r.AddDetailsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<PaymentDetail>>()), Times.Never);
    }

    [Test]
    public void AddAsync_PeriodMonthOutOfRange_ThrowsArgumentException()
    {
        var dto = new PaymentAddRequestDto { CustomerId = 1, PeriodMonth = 13, PeriodYear = 2026, PaidAmount = 100m };

        Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(dto));
    }

    [Test]
    public async Task AddAsync_PeriodMonthOutOfRange_DoesNotCallRepository()
    {
        var dto = new PaymentAddRequestDto { CustomerId = 1, PeriodMonth = 0, PeriodYear = 2026, PaidAmount = 100m };

        try { await _service.AddAsync(dto); } catch { }

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Never);
    }

    [Test]
    public async Task AddAsync_UsesExplicitPaymentDate_WhenProvided()
    {
        var explicitDate = new DateTime(2026, 1, 15);
        var dto = new PaymentAddRequestDto { CustomerId = 1, PeriodMonth = 1, PeriodYear = 2026, PaidAmount = 50m, PaymentDate = explicitDate };
        _repoMock.Setup(r => r.GetActiveMembershipsAsync(1, explicitDate)).ReturnsAsync(new List<ActiveMembershipRow>());
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Payment>())).ReturnsAsync(1);

        await _service.AddAsync(dto);

        _repoMock.Verify(r => r.GetActiveMembershipsAsync(1, explicitDate), Times.Once);
        _repoMock.Verify(r => r.AddAsync(It.Is<Payment>(p => p.paymentdate == explicitDate)), Times.Once);
    }

    // ─── UpdateAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task UpdateAsync_PartialUpdate_OnlyChangesProvidedFields()
    {
        var existing = new Payment { id = 1, customerid = 2, periodmonth = 5, periodyear = 2026, expectedamount = 100m, paidamount = 80m, paymentdate = DateTime.UtcNow, note = "vieja nota" };
        _repoMock.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>())).ReturnsAsync(true);

        var dto = new PaymentUpdateRequestDto { PaidAmount = 100m };
        var result = await _service.UpdateAsync(1, dto);

        Assert.That(result, Is.True);
        _repoMock.Verify(r => r.UpdateAsync(It.Is<Payment>(p =>
            p.paidamount == 100m && p.periodmonth == 5 && p.periodyear == 2026 && p.note == "vieja nota")), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_NonExistentPayment_ReturnsFalse()
    {
        _repoMock.Setup(r => r.FindByIdAsync(99)).ReturnsAsync((Payment?)null);

        var result = await _service.UpdateAsync(99, new PaymentUpdateRequestDto());

        Assert.That(result, Is.False);
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Payment>()), Times.Never);
    }

    [Test]
    public void UpdateAsync_InvalidPeriodMonth_ThrowsArgumentException()
    {
        var existing = new Payment { id = 1, customerid = 2, periodmonth = 5, periodyear = 2026, expectedamount = 100m, paidamount = 80m, paymentdate = DateTime.UtcNow };
        _repoMock.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(existing);

        Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(1, new PaymentUpdateRequestDto { PeriodMonth = 15 }));
    }

    // ─── DeleteAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task DeleteAsync_ExistingPayment_ReturnsTrue()
    {
        var existing = new Payment { id = 1 };
        _repoMock.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _service.DeleteAsync(1);

        Assert.That(result, Is.True);
        _repoMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_NonExistentPayment_ReturnsFalseAndDoesNotCallDelete()
    {
        _repoMock.Setup(r => r.FindByIdAsync(99)).ReturnsAsync((Payment?)null);

        var result = await _service.DeleteAsync(99);

        Assert.That(result, Is.False);
        _repoMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    // ─── Saldo (balance) ──────────────────────────────────────────────────────

    [Test]
    public async Task GetCustomerBalanceAsync_PositiveBalance_ReturnsCredit()
    {
        _repoMock.Setup(r => r.GetBalanceAsync(1)).ReturnsAsync(50m);

        var result = await _service.GetCustomerBalanceAsync(1);

        Assert.That(result.Balance, Is.EqualTo(50m));
    }

    [Test]
    public async Task GetCustomerBalanceAsync_NegativeBalance_ReturnsDebt()
    {
        _repoMock.Setup(r => r.GetBalanceAsync(1)).ReturnsAsync(-30m);

        var result = await _service.GetCustomerBalanceAsync(1);

        Assert.That(result.Balance, Is.EqualTo(-30m));
    }

    [Test]
    public async Task GetCustomerBalanceAsync_NoPayments_ReturnsZero()
    {
        _repoMock.Setup(r => r.GetBalanceAsync(1)).ReturnsAsync(0m);

        var result = await _service.GetCustomerBalanceAsync(1);

        Assert.That(result.Balance, Is.EqualTo(0m));
    }

    // ─── GetCustomerHistoryAsync ──────────────────────────────────────────────

    [Test]
    public async Task GetCustomerHistoryAsync_ReturnsPaymentsAndBalance()
    {
        var payment = new Payment { id = 1, customerid = 1, periodmonth = 7, periodyear = 2026, expectedamount = 100m, paidamount = 80m, paymentdate = DateTime.UtcNow, note = null };
        var details = new List<PaymentDetail> { new PaymentDetail { id = 1, paymentid = 1, membershipid = 10, membershipname = "Yoga", membershipprice = 100m } };
        _repoMock.Setup(r => r.GetByCustomerAsync(1)).ReturnsAsync(new List<(Payment, IEnumerable<PaymentDetail>)> { (payment, details) });
        _repoMock.Setup(r => r.GetBalanceAsync(1)).ReturnsAsync(-20m);

        var result = await _service.GetCustomerHistoryAsync(1);

        Assert.That(result.CustomerId, Is.EqualTo(1));
        Assert.That(result.Balance, Is.EqualTo(-20m));
        Assert.That(result.Payments, Has.Count.EqualTo(1));
        Assert.That(result.Payments[0].Estado, Is.EqualTo("deuda"));
        Assert.That(result.Payments[0].Details, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetCustomerHistoryAsync_NoPayments_ReturnsEmptyListAndZeroBalance()
    {
        _repoMock.Setup(r => r.GetByCustomerAsync(1)).ReturnsAsync(new List<(Payment, IEnumerable<PaymentDetail>)>());
        _repoMock.Setup(r => r.GetBalanceAsync(1)).ReturnsAsync(0m);

        var result = await _service.GetCustomerHistoryAsync(1);

        Assert.That(result.Payments, Is.Empty);
        Assert.That(result.Balance, Is.EqualTo(0m));
    }

    // ─── GetAllAsync (filtros) ────────────────────────────────────────────────

    [Test]
    public async Task GetAllAsync_PassesFiltersToRepository()
    {
        var desde = new DateTime(2026, 1, 1);
        var hasta = new DateTime(2026, 12, 31);
        _repoMock.Setup(r => r.GetAllAsync(7, 2026, "deuda", 123, desde, hasta))
            .ReturnsAsync(new List<(Payment, IEnumerable<PaymentDetail>)>());

        await _service.GetAllAsync(7, 2026, "deuda", 123, desde, hasta);

        _repoMock.Verify(r => r.GetAllAsync(7, 2026, "deuda", 123, desde, hasta), Times.Once);
    }

    [Test]
    public async Task GetAllAsync_MapsEstadoPerPayment()
    {
        var favor = new Payment { id = 1, customerid = 1, expectedamount = 50m, paidamount = 80m, paymentdate = DateTime.UtcNow };
        var completo = new Payment { id = 2, customerid = 2, expectedamount = 50m, paidamount = 50m, paymentdate = DateTime.UtcNow };
        _repoMock.Setup(r => r.GetAllAsync(null, null, null, null, null, null))
            .ReturnsAsync(new List<(Payment, IEnumerable<PaymentDetail>)>
            {
                (favor, new List<PaymentDetail>()),
                (completo, new List<PaymentDetail>())
            });

        var result = (await _service.GetAllAsync(null, null, null, null, null, null)).ToList();

        Assert.That(result[0].Estado, Is.EqualTo("favor"));
        Assert.That(result[1].Estado, Is.EqualTo("completo"));
    }

    [Test]
    public void GetAllAsync_InvalidEstado_ThrowsArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(() => _service.GetAllAsync(null, null, "inexistente", null, null, null));
    }

    // ─── GetMonthlySummaryAsync ───────────────────────────────────────────────

    [Test]
    public async Task GetMonthlySummaryAsync_WithPayments_ReturnsAggregatedTotals()
    {
        var row = new MonthlySummaryRow
        {
            totalexpected = 500m,
            totalpaid = 420m,
            totaldebt = 100m,
            totalcredit = 20m,
            paymentcount = 5,
            customersindebtcount = 2,
            customerswithcreditcount = 1
        };
        _repoMock.Setup(r => r.GetMonthlySummaryAsync(7, 2026)).ReturnsAsync(row);

        var result = await _service.GetMonthlySummaryAsync(7, 2026);

        Assert.That(result.PeriodMonth, Is.EqualTo(7));
        Assert.That(result.PeriodYear, Is.EqualTo(2026));
        Assert.That(result.TotalExpected, Is.EqualTo(500m));
        Assert.That(result.TotalPaid, Is.EqualTo(420m));
        Assert.That(result.TotalDebt, Is.EqualTo(100m));
        Assert.That(result.TotalCredit, Is.EqualTo(20m));
        Assert.That(result.PaymentCount, Is.EqualTo(5));
        Assert.That(result.CustomersInDebtCount, Is.EqualTo(2));
        Assert.That(result.CustomersWithCreditCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetMonthlySummaryAsync_WithoutPayments_ReturnsAllZeros()
    {
        _repoMock.Setup(r => r.GetMonthlySummaryAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new MonthlySummaryRow());

        var result = await _service.GetMonthlySummaryAsync(7, 2026);

        Assert.That(result.TotalExpected, Is.EqualTo(0m));
        Assert.That(result.TotalPaid, Is.EqualTo(0m));
        Assert.That(result.PaymentCount, Is.EqualTo(0));
    }

    [Test]
    public async Task GetMonthlySummaryAsync_DefaultsToCurrentMonthAndYear_WhenNotProvided()
    {
        var now = DateTime.UtcNow;
        _repoMock.Setup(r => r.GetMonthlySummaryAsync(now.Month, now.Year)).ReturnsAsync(new MonthlySummaryRow());

        var result = await _service.GetMonthlySummaryAsync(null, null);

        Assert.That(result.PeriodMonth, Is.EqualTo(now.Month));
        Assert.That(result.PeriodYear, Is.EqualTo(now.Year));
        _repoMock.Verify(r => r.GetMonthlySummaryAsync(now.Month, now.Year), Times.Once);
    }
}
