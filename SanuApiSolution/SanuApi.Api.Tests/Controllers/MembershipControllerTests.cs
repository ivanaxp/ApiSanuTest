using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SanuApi.Api.Controllers;
using SanuApi.Application.DTOs.Membership;
using SanuApi.Application.Interfaces;

namespace SanuApi.Api.Tests.Controllers;

[TestFixture]
public class MembershipControllerTests
{
    private Mock<IMembershipService> _serviceMock;
    private MembershipController _controller;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<IMembershipService>();
        _controller = new MembershipController(_serviceMock.Object);
    }

    // ─── GetAll ────────────────────────────────────────────────────────────────

    [Test]
    public async Task GetAll_ReturnsOkWithList()
    {
        var memberships = new List<MembershipFindResponseDto>
        {
            new MembershipFindResponseDto { Id = 1, Name = "Mensual", Price = 5000m, Frecuency = 1 },
            new MembershipFindResponseDto { Id = 2, Name = "Anual",   Price = 50000m, Frecuency = 12 }
        };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(memberships);

        var actionResult = await _controller.GetAll();

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.StatusCode, Is.EqualTo(200));
        var data = ok.Value as IEnumerable<MembershipFindResponseDto>;
        Assert.That(data!.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetAll_EmptyList_ReturnsOkWithEmpty()
    {
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<MembershipFindResponseDto>());

        var actionResult = await _controller.GetAll();

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var data = ok!.Value as IEnumerable<MembershipFindResponseDto>;
        Assert.That(data, Is.Empty);
    }

    // ─── GetById ───────────────────────────────────────────────────────────────

    [Test]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        var membership = new MembershipFindResponseDto { Id = 1, Name = "Mensual" };
        _serviceMock.Setup(s => s.FindByIdAsync(1)).ReturnsAsync(membership);

        var actionResult = await _controller.GetById(1);

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.FindByIdAsync(99)).ReturnsAsync((MembershipFindResponseDto?)null);

        var actionResult = await _controller.GetById(99);

        Assert.That(actionResult.Result, Is.InstanceOf<NotFoundResult>());
    }

    // ─── Create ────────────────────────────────────────────────────────────────

    [Test]
    public async Task Create_ValidDto_ReturnsCreatedAtAction()
    {
        var dto = new MembershipAddRequestDto { Name = "Trimestral", Price = 12000m, Frecuency = 3 };
        _serviceMock.Setup(s => s.AddAsync(dto)).ReturnsAsync(4);

        var actionResult = await _controller.Create(dto);

        var created = actionResult.Result as CreatedAtActionResult;
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.StatusCode, Is.EqualTo(201));
        Assert.That(created.Value, Is.EqualTo(4));
    }

    [Test]
    public async Task Create_CallsServiceWithCorrectDto()
    {
        var dto = new MembershipAddRequestDto { Name = "Semestral", Price = 25000m, Frecuency = 6 };
        _serviceMock.Setup(s => s.AddAsync(dto)).ReturnsAsync(1);

        await _controller.Create(dto);

        _serviceMock.Verify(s => s.AddAsync(dto), Times.Once);
    }

    // ─── Update ────────────────────────────────────────────────────────────────

    [Test]
    public async Task Update_ValidRequest_ReturnsOk()
    {
        var request = new MembershipUpdateRequestDto { Id = 1, Name = "Mensual Plus", Price = 6000m, Frecuency = 1 };
        _serviceMock.Setup(s => s.UpdateAsync(request)).ReturnsAsync(true);

        var actionResult = await _controller.Update(request);

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task Update_WhenFails_ReturnsOkWithFalse()
    {
        var request = new MembershipUpdateRequestDto { Id = 99, Name = "X" };
        _serviceMock.Setup(s => s.UpdateAsync(request)).ReturnsAsync(false);

        var actionResult = await _controller.Update(request);

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(false));
    }

    // ─── Delete ────────────────────────────────────────────────────────────────

    [Test]
    public async Task Delete_ExistingId_ReturnsOkWithTrue()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        var actionResult = await _controller.Delete(1);

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(true));
    }

    [Test]
    public async Task Delete_NotExistingId_ReturnsOkWithFalse()
    {
        _serviceMock.Setup(s => s.DeleteAsync(99)).ReturnsAsync(false);

        var actionResult = await _controller.Delete(99);

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(false));
    }

    [Test]
    public async Task Delete_CallsServiceWithCorrectId()
    {
        _serviceMock.Setup(s => s.DeleteAsync(5)).ReturnsAsync(true);

        await _controller.Delete(5);

        _serviceMock.Verify(s => s.DeleteAsync(5), Times.Once);
    }
}
