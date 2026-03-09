using Moq;
using NUnit.Framework;
using SanuApi.Application.DTOs.Goal;
using SanuApi.Application.Services;
using SanuApi.Domain.Entities;
using SanuApi.Domain.Interfaces;

namespace SanuApi.Application.Tests.Services;

[TestFixture]
public class GoalServiceTests
{
    private Mock<IGoalRepository> _goalRepoMock;
    private GoalService _service;

    [SetUp]
    public void SetUp()
    {
        _goalRepoMock = new Mock<IGoalRepository>();
        _service = new GoalService(_goalRepoMock.Object);
    }

    // ─── GetAllAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task GetAllAsync_ReturnsMappedList()
    {
        var goals = new List<Goal>
        {
            new Goal { id = 1, goalname = "Perder peso" },
            new Goal { id = 2, goalname = "Ganar masa" }
        };
        _goalRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(goals);

        var result = await _service.GetAllAsync();

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetAllAsync_EmptyRepository_ReturnsEmpty()
    {
        _goalRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Goal>());

        var result = await _service.GetAllAsync();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetAllAsync_MapsFieldsCorrectly()
    {
        var goals = new List<Goal>
        {
            new Goal { id = 5, goalname = "Flexibilidad" }
        };
        _goalRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(goals);

        var result = (await _service.GetAllAsync()).First();

        Assert.That(result.Id, Is.EqualTo(5));
        Assert.That(result.GoalName, Is.EqualTo("Flexibilidad"));
    }

    // ─── FindByIdAsync ────────────────────────────────────────────────────────

    [Test]
    public async Task FindByIdAsync_WhenExists_ReturnsMappedDto()
    {
        var goal = new Goal { id = 1, goalname = "Perder peso" };
        _goalRepoMock.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(goal);

        var result = await _service.FindByIdAsync(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(1));
        Assert.That(result.GoalName, Is.EqualTo("Perder peso"));
    }

    [Test]
    public async Task FindByIdAsync_WhenNotExists_ReturnsNull()
    {
        _goalRepoMock.Setup(r => r.FindByIdAsync(99)).ReturnsAsync((Goal?)null);

        var result = await _service.FindByIdAsync(99);

        Assert.That(result, Is.Null);
    }

    // ─── AddAsync ─────────────────────────────────────────────────────────────

    [Test]
    public async Task AddAsync_ValidDto_ReturnsNewId()
    {
        var dto = new AddGoalRequestDto { GoalName = "Resistencia" };
        _goalRepoMock.Setup(r => r.AddAsync(It.IsAny<Goal>())).ReturnsAsync(7);

        var result = await _service.AddAsync(dto);

        Assert.That(result, Is.EqualTo(7));
    }

    [Test]
    public async Task AddAsync_MapsGoalNameCorrectly()
    {
        var dto = new AddGoalRequestDto { GoalName = "Definicion" };
        Goal? capturedGoal = null;
        _goalRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Goal>()))
            .Callback<Goal>(g => capturedGoal = g)
            .ReturnsAsync(1);

        await _service.AddAsync(dto);

        Assert.That(capturedGoal!.goalname, Is.EqualTo("Definicion"));
        Assert.That(capturedGoal.fechaBaja, Is.Null);
    }

    // ─── DeleteAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task DeleteAsync_ExistingGoal_ReturnsTrue()
    {
        var goal = new Goal { id = 1, goalname = "Perder peso" };
        _goalRepoMock.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(goal);
        _goalRepoMock.Setup(r => r.DeleteAsync(goal)).ReturnsAsync(true);

        var result = await _service.DeleteAsync(1);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task DeleteAsync_NotExistingGoal_ReturnsFalse()
    {
        _goalRepoMock.Setup(r => r.FindByIdAsync(99)).ReturnsAsync((Goal?)null);
        _goalRepoMock.Setup(r => r.DeleteAsync(It.IsAny<Goal>())).ReturnsAsync(false);

        var result = await _service.DeleteAsync(99);

        Assert.That(result, Is.False);
    }
}
