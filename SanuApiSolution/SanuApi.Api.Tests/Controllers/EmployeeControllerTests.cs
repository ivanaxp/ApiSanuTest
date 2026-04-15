using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SanuApi.Api.Controllers;
using SanuApi.Application.DTOs.Trainer;
using SanuApi.Application.Interfaces;

namespace SanuApi.Api.Tests.Controllers;

[TestFixture]
public class EmployeeControllerTests
{
    private Mock<ITrainerService> _serviceMock;
    private EmployeeController _controller;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<ITrainerService>();
        _controller = new EmployeeController(_serviceMock.Object);
    }

    // ─── CreateTrainer ─────────────────────────────────────────────────────────

    [Test]
    public async Task CreateTrainer_ValidDto_Returns201WithId()
    {
        var dto = new TrainerAddRequestDto { TrainerName = "Carlos", TrainerLastName = "Gomez" };
        _serviceMock.Setup(s => s.AddAsync(dto)).ReturnsAsync(7);

        var actionResult = await _controller.CreateTrainer(dto);

        var result = actionResult.Result as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(201));
        Assert.That(result.Value, Is.EqualTo(7));
    }

    [Test]
    public async Task CreateTrainer_CallsServiceWithCorrectDto()
    {
        var dto = new TrainerAddRequestDto { TrainerName = "Ana", TrainerLastName = "Lopez" };
        _serviceMock.Setup(s => s.AddAsync(dto)).ReturnsAsync(3);

        await _controller.CreateTrainer(dto);

        _serviceMock.Verify(s => s.AddAsync(dto), Times.Once);
    }

    [Test]
    public async Task CreateTrainer_WhenServiceThrowsArgumentException_ReturnsBadRequest()
    {
        var dto = new TrainerAddRequestDto { TrainerName = "", TrainerLastName = "Lopez" };
        _serviceMock.Setup(s => s.AddAsync(dto))
            .ThrowsAsync(new ArgumentException("El nombre es requerido."));

        var actionResult = await _controller.CreateTrainer(dto);

        var badRequest = actionResult.Result as BadRequestObjectResult;
        Assert.That(badRequest, Is.Not.Null);
        Assert.That(badRequest!.StatusCode, Is.EqualTo(400));
    }

    // ─── AssignClasses ─────────────────────────────────────────────────────────

    [Test]
    public async Task AssignClasses_ValidRequest_Returns201WithCount()
    {
        var dto = new TrainerAssignClassesRequestDto { ClassIds = new List<int> { 1, 2, 3 } };
        _serviceMock.Setup(s => s.AddClassesAsync(1, dto.ClassIds)).ReturnsAsync(3);

        var actionResult = await _controller.AssignClasses(1, dto);

        var result = actionResult.Result as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(201));
        Assert.That(result.Value, Is.EqualTo(3));
    }

    [Test]
    public async Task AssignClasses_CallsServiceWithCorrectArguments()
    {
        var dto = new TrainerAssignClassesRequestDto { ClassIds = new List<int> { 5, 6 } };
        _serviceMock.Setup(s => s.AddClassesAsync(2, dto.ClassIds)).ReturnsAsync(2);

        await _controller.AssignClasses(2, dto);

        _serviceMock.Verify(s => s.AddClassesAsync(2, dto.ClassIds), Times.Once);
    }

    [Test]
    public async Task AssignClasses_WhenServiceThrowsArgumentException_ReturnsBadRequest()
    {
        var dto = new TrainerAssignClassesRequestDto { ClassIds = new List<int>() };
        _serviceMock.Setup(s => s.AddClassesAsync(1, dto.ClassIds))
            .ThrowsAsync(new ArgumentException("Debe especificar al menos una clase."));

        var actionResult = await _controller.AssignClasses(1, dto);

        var badRequest = actionResult.Result as BadRequestObjectResult;
        Assert.That(badRequest, Is.Not.Null);
        Assert.That(badRequest!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task AssignClasses_WhenServiceThrowsInvalidOperationException_ReturnsBadRequest()
    {
        var dto = new TrainerAssignClassesRequestDto { ClassIds = new List<int> { 99 } };
        _serviceMock.Setup(s => s.AddClassesAsync(1, dto.ClassIds))
            .ThrowsAsync(new InvalidOperationException("Error al asignar la clase al trainer"));

        var actionResult = await _controller.AssignClasses(1, dto);

        var badRequest = actionResult.Result as BadRequestObjectResult;
        Assert.That(badRequest, Is.Not.Null);
        Assert.That(badRequest!.StatusCode, Is.EqualTo(400));
    }

    // ─── GetTrainerClasses ─────────────────────────────────────────────────────

    [Test]
    public async Task GetTrainerClasses_ReturnsOkWithList()
    {
        var classes = new List<TrainerClassWithStudentsResponseDto>
        {
            new TrainerClassWithStudentsResponseDto
            {
                ClassId = 1, ClassName = "Yoga", Day = "Lunes", Hour = "08:00", Capacity = 10,
                Students = new List<StudentInClassDto>
                {
                    new StudentInClassDto { CustomerId = 1, CustomerName = "Juan", CustomerLastName = "Perez" }
                }
            }
        };
        _serviceMock.Setup(s => s.GetClassesWithStudentsAsync(1)).ReturnsAsync(classes);

        var actionResult = await _controller.GetTrainerClasses(1);

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.StatusCode, Is.EqualTo(200));
        var data = ok.Value as IEnumerable<TrainerClassWithStudentsResponseDto>;
        Assert.That(data!.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetTrainerClasses_ReturnsClassDetailWithStudents()
    {
        var student = new StudentInClassDto { CustomerId = 5, CustomerName = "Maria", CustomerLastName = "Lopez" };
        var classes = new List<TrainerClassWithStudentsResponseDto>
        {
            new TrainerClassWithStudentsResponseDto
            {
                ClassId = 2, ClassName = "Pilates", Day = "Martes", Hour = "10:00", Capacity = 15,
                Students = new List<StudentInClassDto> { student }
            }
        };
        _serviceMock.Setup(s => s.GetClassesWithStudentsAsync(3)).ReturnsAsync(classes);

        var actionResult = await _controller.GetTrainerClasses(3);

        var ok = actionResult.Result as OkObjectResult;
        var data = (ok!.Value as IEnumerable<TrainerClassWithStudentsResponseDto>)!.First();
        Assert.That(data.ClassName, Is.EqualTo("Pilates"));
        Assert.That(data.Day, Is.EqualTo("Martes"));
        Assert.That(data.Hour, Is.EqualTo("10:00"));
        Assert.That(data.Students, Has.Count.EqualTo(1));
        Assert.That(data.Students[0].CustomerName, Is.EqualTo("Maria"));
    }

    [Test]
    public async Task GetTrainerClasses_WhenNoClasses_ReturnsOkWithEmptyList()
    {
        _serviceMock.Setup(s => s.GetClassesWithStudentsAsync(99))
            .ReturnsAsync(new List<TrainerClassWithStudentsResponseDto>());

        var actionResult = await _controller.GetTrainerClasses(99);

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.StatusCode, Is.EqualTo(200));
        var data = ok.Value as IEnumerable<TrainerClassWithStudentsResponseDto>;
        Assert.That(data, Is.Empty);
    }

    [Test]
    public async Task GetTrainerClasses_CallsServiceWithCorrectTrainerId()
    {
        _serviceMock.Setup(s => s.GetClassesWithStudentsAsync(7))
            .ReturnsAsync(new List<TrainerClassWithStudentsResponseDto>());

        await _controller.GetTrainerClasses(7);

        _serviceMock.Verify(s => s.GetClassesWithStudentsAsync(7), Times.Once);
    }
}
