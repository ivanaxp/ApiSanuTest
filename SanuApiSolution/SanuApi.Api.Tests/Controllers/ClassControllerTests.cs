using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SanuApi.Api.Controllers;
using SanuApi.Application.DTOs.Class;
using SanuApi.Application.Interfaces;

namespace SanuApi.Api.Tests.Controllers;

[TestFixture]
public class ClassControllerTests
{
    private Mock<IClassService> _serviceMock;
    private ClassController _controller;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<IClassService>();
        _controller = new ClassController(_serviceMock.Object);
    }

    // ─── GetAll ────────────────────────────────────────────────────────────────

    [Test]
    public async Task GetAll_ReturnsOkWithList()
    {
        var classes = new List<ClassFindResponseDto>
        {
            new ClassFindResponseDto { Id = 1, Name = "Yoga", Day = "Lunes", Hour = "08:00", Capacity = 10 },
            new ClassFindResponseDto { Id = 2, Name = "Pilates", Day = "Martes", Hour = "09:00", Capacity = 15 }
        };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(classes);

        var actionResult = await _controller.GetAll();

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.StatusCode, Is.EqualTo(200));
        var data = ok.Value as IEnumerable<ClassFindResponseDto>;
        Assert.That(data!.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetAll_EmptyList_ReturnsOkWithEmptyList()
    {
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<ClassFindResponseDto>());

        var actionResult = await _controller.GetAll();

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var data = ok!.Value as IEnumerable<ClassFindResponseDto>;
        Assert.That(data, Is.Empty);
    }

    // ─── GetById ───────────────────────────────────────────────────────────────

    [Test]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        var clase = new ClassFindResponseDto { Id = 1, Name = "Yoga" };
        _serviceMock.Setup(s => s.FindByIdAsync(1)).ReturnsAsync(clase);

        var actionResult = await _controller.GetById(1);

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.FindByIdAsync(99)).ReturnsAsync((ClassFindResponseDto?)null);

        var actionResult = await _controller.GetById(99);

        Assert.That(actionResult.Result, Is.InstanceOf<NotFoundResult>());
    }

    // ─── Create ────────────────────────────────────────────────────────────────

    [Test]
    public async Task Create_ValidDto_ReturnsCreatedAtAction()
    {
        var dto = new AddClassRequestDto { Name = "Yoga", Day = "Lunes", Hour = "08:00", Capacity = 10 };
        _serviceMock.Setup(s => s.AddAsync(dto)).ReturnsAsync(3);

        var actionResult = await _controller.Create(dto);

        var created = actionResult.Result as CreatedAtActionResult;
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.StatusCode, Is.EqualTo(201));
        Assert.That(created.Value, Is.EqualTo(3));
    }

    [Test]
    public async Task Create_CallsServiceWithCorrectDto()
    {
        var dto = new AddClassRequestDto { Name = "Crossfit", Day = "Miercoles", Hour = "07:00", Capacity = 20 };
        _serviceMock.Setup(s => s.AddAsync(dto)).ReturnsAsync(1);

        await _controller.Create(dto);

        _serviceMock.Verify(s => s.AddAsync(dto), Times.Once);
    }

    // ─── Delete ────────────────────────────────────────────────────────────────

    [Test]
    public async Task Delete_ExistingId_ReturnsCreatedAtAction()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        var actionResult = await _controller.Delete(1);

        var result = actionResult.Result as CreatedAtActionResult;
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task Delete_CallsServiceWithCorrectId()
    {
        _serviceMock.Setup(s => s.DeleteAsync(5)).ReturnsAsync(true);

        await _controller.Delete(5);

        _serviceMock.Verify(s => s.DeleteAsync(5), Times.Once);
    }
}
