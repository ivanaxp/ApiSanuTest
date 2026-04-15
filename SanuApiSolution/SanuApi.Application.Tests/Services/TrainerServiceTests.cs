using Moq;
using NUnit.Framework;
using SanuApi.Application.DTOs.Trainer;
using SanuApi.Application.Services;
using SanuApi.Domain.Entities;
using SanuApi.Domain.Interfaces;

namespace SanuApi.Application.Tests.Services;

[TestFixture]
public class TrainerServiceTests
{
    private Mock<ITrainerRepository> _repoMock;
    private TrainerService _service;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<ITrainerRepository>();
        _service = new TrainerService(_repoMock.Object);
    }

    // ─── AddAsync ─────────────────────────────────────────────────────────────

    [Test]
    public async Task AddAsync_ValidDto_ReturnsInsertedId()
    {
        var dto = new TrainerAddRequestDto { TrainerName = "Carlos", TrainerLastName = "Gomez" };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Trainer>())).ReturnsAsync(5);

        var result = await _service.AddAsync(dto);

        Assert.That(result, Is.EqualTo(5));
    }

    [Test]
    public async Task AddAsync_MapsNameCorrectly()
    {
        var dto = new TrainerAddRequestDto { TrainerName = "Juan", TrainerLastName = "Perez" };
        Trainer? captured = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Trainer>()))
            .Callback<Trainer>(t => captured = t)
            .ReturnsAsync(1);

        await _service.AddAsync(dto);

        Assert.That(captured!.name, Is.EqualTo("Juan"));
        Assert.That(captured.lastName, Is.EqualTo("Perez"));
    }

    [Test]
    public async Task AddAsync_CallsRepositoryOnce()
    {
        var dto = new TrainerAddRequestDto { TrainerName = "Maria", TrainerLastName = "Diaz" };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Trainer>())).ReturnsAsync(2);

        await _service.AddAsync(dto);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Trainer>()), Times.Once);
    }

    [Test]
    public async Task AddAsync_WhenRepositoryThrows_PropagatesException()
    {
        var dto = new TrainerAddRequestDto { TrainerName = "Carlos", TrainerLastName = "Gomez" };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Trainer>()))
            .ThrowsAsync(new InvalidOperationException("Error al insertar el trainer"));

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddAsync(dto));
    }

    // ─── AddClassesAsync ──────────────────────────────────────────────────────

    [Test]
    public async Task AddClassesAsync_ValidList_ReturnsCountOfInserted()
    {
        var classIds = new List<int> { 1, 2, 3 };
        _repoMock.Setup(r => r.AddClassAsync(It.IsAny<TrainerClasses>())).ReturnsAsync(true);

        var result = await _service.AddClassesAsync(1, classIds);

        Assert.That(result, Is.EqualTo(3));
    }

    [Test]
    public async Task AddClassesAsync_CallsRepositoryOncePerClassId()
    {
        var classIds = new List<int> { 5, 6, 7 };
        _repoMock.Setup(r => r.AddClassAsync(It.IsAny<TrainerClasses>())).ReturnsAsync(true);

        await _service.AddClassesAsync(1, classIds);

        _repoMock.Verify(r => r.AddClassAsync(It.IsAny<TrainerClasses>()), Times.Exactly(3));
    }

    [Test]
    public async Task AddClassesAsync_MapsIdsCorrectly()
    {
        var captured = new List<TrainerClasses>();
        _repoMock.Setup(r => r.AddClassAsync(It.IsAny<TrainerClasses>()))
            .Callback<TrainerClasses>(tc => captured.Add(tc))
            .ReturnsAsync(true);

        await _service.AddClassesAsync(4, new List<int> { 7, 8 });

        Assert.That(captured, Has.Count.EqualTo(2));
        Assert.That(captured[0].idtrainer, Is.EqualTo(4));
        Assert.That(captured[0].idclass, Is.EqualTo(7));
        Assert.That(captured[1].idtrainer, Is.EqualTo(4));
        Assert.That(captured[1].idclass, Is.EqualTo(8));
    }

    [Test]
    public async Task AddClassesAsync_EmptyList_ThrowsArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(() => _service.AddClassesAsync(1, new List<int>()));
    }

    [Test]
    public async Task AddClassesAsync_EmptyList_DoesNotCallRepository()
    {
        try { await _service.AddClassesAsync(1, new List<int>()); } catch { }

        _repoMock.Verify(r => r.AddClassAsync(It.IsAny<TrainerClasses>()), Times.Never);
    }

    [Test]
    public async Task AddClassesAsync_WhenRepositoryThrows_PropagatesException()
    {
        _repoMock.Setup(r => r.AddClassAsync(It.IsAny<TrainerClasses>()))
            .ThrowsAsync(new InvalidOperationException("Error al asignar la clase al trainer"));

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddClassesAsync(1, new List<int> { 5 }));
    }

    // ─── GetClassesWithStudentsAsync ──────────────────────────────────────────

    [Test]
    public async Task GetClassesWithStudentsAsync_ReturnsMappedDtos()
    {
        var repoResult = new List<(Classes, IEnumerable<Customer>)>
        {
            (
                new Classes { id = 1, name = "Yoga", day = "Lunes", hour = "08:00", capacity = 10 },
                new List<Customer>
                {
                    new Customer { id = 1, customername = "Juan", customerlastname = "Perez" }
                }
            )
        };
        _repoMock.Setup(r => r.GetClassesWithStudentsAsync(1)).ReturnsAsync(repoResult);

        var result = (await _service.GetClassesWithStudentsAsync(1)).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].ClassId, Is.EqualTo(1));
        Assert.That(result[0].ClassName, Is.EqualTo("Yoga"));
        Assert.That(result[0].Day, Is.EqualTo("Lunes"));
        Assert.That(result[0].Hour, Is.EqualTo("08:00"));
        Assert.That(result[0].Capacity, Is.EqualTo(10));
        Assert.That(result[0].Students, Has.Count.EqualTo(1));
        Assert.That(result[0].Students[0].CustomerName, Is.EqualTo("Juan"));
        Assert.That(result[0].Students[0].CustomerLastName, Is.EqualTo("Perez"));
    }

    [Test]
    public async Task GetClassesWithStudentsAsync_MultipleClasses_ReturnsMappedDtos()
    {
        var repoResult = new List<(Classes, IEnumerable<Customer>)>
        {
            (
                new Classes { id = 1, name = "Yoga", day = "Lunes", hour = "08:00", capacity = 10 },
                new List<Customer> { new Customer { id = 1, customername = "Juan", customerlastname = "Perez" } }
            ),
            (
                new Classes { id = 2, name = "Pilates", day = "Miercoles", hour = "11:00", capacity = 8 },
                new List<Customer>()
            )
        };
        _repoMock.Setup(r => r.GetClassesWithStudentsAsync(1)).ReturnsAsync(repoResult);

        var result = (await _service.GetClassesWithStudentsAsync(1)).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[1].ClassName, Is.EqualTo("Pilates"));
        Assert.That(result[1].Students, Is.Empty);
    }

    [Test]
    public async Task GetClassesWithStudentsAsync_ClassWithNoStudents_ReturnsEmptyStudentsList()
    {
        var repoResult = new List<(Classes, IEnumerable<Customer>)>
        {
            (
                new Classes { id = 2, name = "Pilates", day = "Martes", hour = "10:00", capacity = 5 },
                new List<Customer>()
            )
        };
        _repoMock.Setup(r => r.GetClassesWithStudentsAsync(2)).ReturnsAsync(repoResult);

        var result = (await _service.GetClassesWithStudentsAsync(2)).ToList();

        Assert.That(result[0].Students, Is.Empty);
    }

    [Test]
    public async Task GetClassesWithStudentsAsync_WhenNoClasses_ReturnsEmpty()
    {
        _repoMock.Setup(r => r.GetClassesWithStudentsAsync(99))
            .ReturnsAsync(new List<(Classes, IEnumerable<Customer>)>());

        var result = await _service.GetClassesWithStudentsAsync(99);

        Assert.That(result, Is.Empty);
    }
}
