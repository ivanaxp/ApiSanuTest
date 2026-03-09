using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SanuApi.Api.Controllers;
using SanuApi.Application.DTOs.Goal;
using SanuApi.Application.Interfaces;

namespace SanuApi.Api.Tests.Controllers;

[TestFixture]
public class GoalControllerTests
{
    private Mock<IGoalService> _serviceMock;
    private GoalController _controller;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<IGoalService>();
        _controller = new GoalController(_serviceMock.Object);
    }

    // ─── GetAll ────────────────────────────────────────────────────────────────

    [Test]
    public async Task GetAll_ReturnsOkWithList()
    {
        var goals = new List<GoalFindResponseDto>
        {
            new GoalFindResponseDto { Id = 1, GoalName = "Perder peso" },
            new GoalFindResponseDto { Id = 2, GoalName = "Ganar masa" }
        };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(goals);

        var actionResult = await _controller.GetAll();

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.StatusCode, Is.EqualTo(200));
        var data = ok.Value as IEnumerable<GoalFindResponseDto>;
        Assert.That(data!.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetAll_EmptyList_ReturnsOkWithEmpty()
    {
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<GoalFindResponseDto>());

        var actionResult = await _controller.GetAll();

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var data = ok!.Value as IEnumerable<GoalFindResponseDto>;
        Assert.That(data, Is.Empty);
    }

    // ─── GetById ───────────────────────────────────────────────────────────────

    [Test]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        var goal = new GoalFindResponseDto { Id = 1, GoalName = "Perder peso" };
        _serviceMock.Setup(s => s.FindByIdAsync(1)).ReturnsAsync(goal);

        var actionResult = await _controller.GetById(1);

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.FindByIdAsync(99)).ReturnsAsync((GoalFindResponseDto?)null);

        var actionResult = await _controller.GetById(99);

        Assert.That(actionResult.Result, Is.InstanceOf<NotFoundResult>());
    }

    // ─── Create ────────────────────────────────────────────────────────────────

    [Test]
    public async Task Create_ValidDto_ReturnsCreatedAtAction()
    {
        var dto = new AddGoalRequestDto { GoalName = "Resistencia" };
        _serviceMock.Setup(s => s.AddAsync(dto)).ReturnsAsync(7);

        var actionResult = await _controller.Create(dto);

        var created = actionResult.Result as CreatedAtActionResult;
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.StatusCode, Is.EqualTo(201));
        Assert.That(created.Value, Is.EqualTo(7));
    }

    [Test]
    public async Task Create_CallsServiceWithCorrectDto()
    {
        var dto = new AddGoalRequestDto { GoalName = "Flexibilidad" };
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
        _serviceMock.Setup(s => s.DeleteAsync(3)).ReturnsAsync(true);

        await _controller.Delete(3);

        _serviceMock.Verify(s => s.DeleteAsync(3), Times.Once);
    }
}
