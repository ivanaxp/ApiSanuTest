using Moq;
using NUnit.Framework;
using SanuApi.Application.DTOs.Class;
using SanuApi.Application.Services;
using SanuApi.Domain.Entities;
using SanuApi.Domain.Interfaces;

namespace SanuApi.Application.Tests.Services;

[TestFixture]
public class ClassServiceTests
{
    private Mock<IClassRepository> _classRepoMock;
    private ClassService _service;

    [SetUp]
    public void SetUp()
    {
        _classRepoMock = new Mock<IClassRepository>();
        _service = new ClassService(_classRepoMock.Object);
    }

    // ─── GetAllAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task GetAllAsync_ReturnsMappedList()
    {
        var classes = new List<Classes>
        {
            new Classes { id = 1, name = "Yoga", day = "Lunes", hour = "08:00", capacity = 10 },
            new Classes { id = 2, name = "Pilates", day = "Martes", hour = "09:00", capacity = 15 }
        };
        _classRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(classes);

        var result = await _service.GetAllAsync();

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetAllAsync_EmptyRepository_ReturnsEmpty()
    {
        _classRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Classes>());

        var result = await _service.GetAllAsync();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetAllAsync_MapsFieldsCorrectly()
    {
        var classes = new List<Classes>
        {
            new Classes { id = 1, name = "Yoga", day = "Lunes", hour = "08:00", capacity = 10 }
        };
        _classRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(classes);

        var result = (await _service.GetAllAsync()).First();

        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Name, Is.EqualTo("Yoga"));
        Assert.That(result.Day, Is.EqualTo("Lunes"));
        Assert.That(result.Hour, Is.EqualTo("08:00"));
        Assert.That(result.Capacity, Is.EqualTo(10));
    }

    // ─── FindByIdAsync ────────────────────────────────────────────────────────

    [Test]
    public async Task FindByIdAsync_WhenExists_ReturnsMappedDto()
    {
        var clase = new Classes { id = 1, name = "Yoga", day = "Lunes", hour = "08:00", capacity = 10 };
        _classRepoMock.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(clase);

        var result = await _service.FindByIdAsync(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(1));
        Assert.That(result.Name, Is.EqualTo("Yoga"));
    }

    [Test]
    public async Task FindByIdAsync_WhenNotExists_ReturnsNull()
    {
        _classRepoMock.Setup(r => r.FindByIdAsync(99)).ReturnsAsync((Classes?)null);

        var result = await _service.FindByIdAsync(99);

        Assert.That(result, Is.Null);
    }

    // ─── AddAsync ─────────────────────────────────────────────────────────────

    [Test]
    public async Task AddAsync_ValidDto_ReturnsNewId()
    {
        var dto = new AddClassRequestDto { Name = "Yoga", Day = "Lunes", Hour = "08:00", Capacity = 10 };
        _classRepoMock.Setup(r => r.AddAsync(It.IsAny<Classes>())).ReturnsAsync(3);

        var result = await _service.AddAsync(dto);

        Assert.That(result, Is.EqualTo(3));
    }

    [Test]
    public async Task AddAsync_MapsAllFieldsToEntity()
    {
        var dto = new AddClassRequestDto { Name = "Crossfit", Day = "Miercoles", Hour = "07:00", Capacity = 20 };
        Classes? capturedEntity = null;
        _classRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Classes>()))
            .Callback<Classes>(e => capturedEntity = e)
            .ReturnsAsync(1);

        await _service.AddAsync(dto);

        Assert.That(capturedEntity!.name, Is.EqualTo("Crossfit"));
        Assert.That(capturedEntity.day, Is.EqualTo("Miercoles"));
        Assert.That(capturedEntity.hour, Is.EqualTo("07:00"));
        Assert.That(capturedEntity.capacity, Is.EqualTo(20));
    }

    // ─── DeleteAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task DeleteAsync_ExistingClass_ReturnsTrue()
    {
        var clase = new Classes { id = 1, name = "Yoga", day = "Lunes", hour = "08:00", capacity = 10 };
        _classRepoMock.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(clase);
        _classRepoMock.Setup(r => r.DeleteAsync(clase)).ReturnsAsync(true);

        var result = await _service.DeleteAsync(1);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task DeleteAsync_NotExistingClass_ReturnsFalse()
    {
        _classRepoMock.Setup(r => r.FindByIdAsync(99)).ReturnsAsync((Classes?)null);
        _classRepoMock.Setup(r => r.DeleteAsync(It.IsAny<Classes>())).ReturnsAsync(false);

        var result = await _service.DeleteAsync(99);

        Assert.That(result, Is.False);
    }
}
