using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SanuApi.Api.Controllers;
using SanuApi.Application.DTOs.Payment;
using SanuApi.Application.Interfaces;

namespace SanuApi.Api.Tests.Controllers;

[TestFixture]
public class PaymentControllerTests
{
    private Mock<IPaymentService> _serviceMock;
    private PaymentController _controller;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<IPaymentService>();
        _controller = new PaymentController(_serviceMock.Object);
    }

    // ─── GetCustomerHistory ─────────────────────────────────────────────────

    [Test]
    public async Task GetCustomerHistory_ReturnsOkWithHistory()
    {
        var history = new CustomerPaymentHistoryResponseDto { CustomerId = 1, Balance = -50m, Payments = new List<PaymentResponseDto>() };
        _serviceMock.Setup(s => s.GetCustomerHistoryAsync(1)).ReturnsAsync(history);

        var actionResult = await _controller.GetCustomerHistory(1);

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.StatusCode, Is.EqualTo(200));
        Assert.That(ok.Value, Is.EqualTo(history));
    }

    // ─── GetCustomerBalance ──────────────────────────────────────────────────

    [Test]
    public async Task GetCustomerBalance_ReturnsOkWithBalance()
    {
        var balance = new CustomerBalanceResponseDto { CustomerId = 1, Balance = 30m };
        _serviceMock.Setup(s => s.GetCustomerBalanceAsync(1)).ReturnsAsync(balance);

        var actionResult = await _controller.GetCustomerBalance(1);

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(balance));
    }

    // ─── Create ──────────────────────────────────────────────────────────────

    [Test]
    public async Task Create_ValidDto_Returns201WithId()
    {
        var dto = new PaymentAddRequestDto { CustomerId = 1, PeriodMonth = 7, PeriodYear = 2026, PaidAmount = 100m };
        _serviceMock.Setup(s => s.AddAsync(dto)).ReturnsAsync(9);

        var actionResult = await _controller.Create(dto);

        var result = actionResult.Result as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(201));
        Assert.That(result.Value, Is.EqualTo(9));
    }

    [Test]
    public async Task Create_WhenServiceThrowsArgumentException_ReturnsBadRequest()
    {
        var dto = new PaymentAddRequestDto { CustomerId = 1, PeriodMonth = 13, PeriodYear = 2026, PaidAmount = 100m };
        _serviceMock.Setup(s => s.AddAsync(dto)).ThrowsAsync(new ArgumentException("PeriodMonth debe estar entre 1 y 12."));

        var actionResult = await _controller.Create(dto);

        var badRequest = actionResult.Result as BadRequestObjectResult;
        Assert.That(badRequest, Is.Not.Null);
        Assert.That(badRequest!.StatusCode, Is.EqualTo(400));
    }

    // ─── Update ──────────────────────────────────────────────────────────────

    [Test]
    public async Task Update_ExistingPayment_Returns204()
    {
        var dto = new PaymentUpdateRequestDto { PaidAmount = 120m };
        _serviceMock.Setup(s => s.UpdateAsync(1, dto)).ReturnsAsync(true);

        var actionResult = await _controller.Update(1, dto);

        Assert.That(actionResult, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task Update_NonExistentPayment_Returns404()
    {
        var dto = new PaymentUpdateRequestDto { PaidAmount = 120m };
        _serviceMock.Setup(s => s.UpdateAsync(99, dto)).ReturnsAsync(false);

        var actionResult = await _controller.Update(99, dto);

        Assert.That(actionResult, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Update_WhenServiceThrowsArgumentException_ReturnsBadRequest()
    {
        var dto = new PaymentUpdateRequestDto { PeriodMonth = 15 };
        _serviceMock.Setup(s => s.UpdateAsync(1, dto)).ThrowsAsync(new ArgumentException("PeriodMonth debe estar entre 1 y 12."));

        var actionResult = await _controller.Update(1, dto);

        Assert.That(actionResult, Is.InstanceOf<BadRequestObjectResult>());
    }

    // ─── Delete ──────────────────────────────────────────────────────────────

    [Test]
    public async Task Delete_ExistingPayment_Returns204()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        var actionResult = await _controller.Delete(1);

        Assert.That(actionResult, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task Delete_NonExistentPayment_Returns404()
    {
        _serviceMock.Setup(s => s.DeleteAsync(99)).ReturnsAsync(false);

        var actionResult = await _controller.Delete(99);

        Assert.That(actionResult, Is.InstanceOf<NotFoundResult>());
    }

    // ─── GetAll ──────────────────────────────────────────────────────────────

    [Test]
    public async Task GetAll_ReturnsOkWithList()
    {
        var payments = new List<PaymentResponseDto> { new PaymentResponseDto { Id = 1, Estado = "deuda" } };
        _serviceMock.Setup(s => s.GetAllAsync(7, 2026, "deuda", 123, null, null)).ReturnsAsync(payments);

        var actionResult = await _controller.GetAll(7, 2026, "deuda", 123, null, null);

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(payments));
    }

    [Test]
    public async Task GetAll_WhenServiceThrowsArgumentException_ReturnsBadRequest()
    {
        _serviceMock.Setup(s => s.GetAllAsync(null, null, "invalido", null, null, null))
            .ThrowsAsync(new ArgumentException("El estado 'invalido' no es válido."));

        var actionResult = await _controller.GetAll(null, null, "invalido", null, null, null);

        Assert.That(actionResult.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetAll_CallsServiceWithCorrectFilters()
    {
        var desde = new DateTime(2026, 1, 1);
        var hasta = new DateTime(2026, 12, 31);
        _serviceMock.Setup(s => s.GetAllAsync(7, 2026, "favor", 5, desde, hasta)).ReturnsAsync(new List<PaymentResponseDto>());

        await _controller.GetAll(7, 2026, "favor", 5, desde, hasta);

        _serviceMock.Verify(s => s.GetAllAsync(7, 2026, "favor", 5, desde, hasta), Times.Once);
    }

    // ─── GetMonthlySummary ───────────────────────────────────────────────────

    [Test]
    public async Task GetMonthlySummary_ReturnsOkWithSummary()
    {
        var summary = new MonthlySummaryResponseDto { PeriodMonth = 7, PeriodYear = 2026, TotalExpected = 500m };
        _serviceMock.Setup(s => s.GetMonthlySummaryAsync(7, 2026)).ReturnsAsync(summary);

        var actionResult = await _controller.GetMonthlySummary(7, 2026);

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(summary));
    }
}
