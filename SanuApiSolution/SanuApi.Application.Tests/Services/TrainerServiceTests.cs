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

    [Test]
    public async Task AddAsync_WithClassDateIds_ValidatesThenAssignsSchedules()
    {
        var dto = new TrainerAddRequestDto
        {
            TrainerName = "Carlos",
            TrainerLastName = "Gomez",
            ClassId = 3,
            ClassDateIds = new List<int> { 7, 8 }
        };
        _repoMock.Setup(r => r.GetExistingClassDateIdsForClassAsync(3, It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<int> { 7, 8 });
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Trainer>())).ReturnsAsync(9);
        _repoMock.Setup(r => r.AddClassDateAsync(It.IsAny<TrainerClassDate>())).ReturnsAsync(true);

        var id = await _service.AddAsync(dto);

        Assert.That(id, Is.EqualTo(9));
        _repoMock.Verify(r => r.AddClassDateAsync(It.Is<TrainerClassDate>(tc => tc.idtrainer == 9 && tc.idclassdate == 7)), Times.Once);
        _repoMock.Verify(r => r.AddClassDateAsync(It.Is<TrainerClassDate>(tc => tc.idtrainer == 9 && tc.idclassdate == 8)), Times.Once);
    }

    [Test]
    public async Task AddAsync_WithInvalidClassDateId_DoesNotCreateTrainer()
    {
        var dto = new TrainerAddRequestDto
        {
            TrainerName = "Carlos",
            TrainerLastName = "Gomez",
            ClassId = 3,
            ClassDateIds = new List<int> { 7, 99 }
        };
        _repoMock.Setup(r => r.GetExistingClassDateIdsForClassAsync(3, It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<int> { 7 });

        Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(dto));
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Trainer>()), Times.Never);
    }

    [Test]
    public async Task AddAsync_WithClassDateIdsButNoClassId_ThrowsArgumentException()
    {
        var dto = new TrainerAddRequestDto
        {
            TrainerName = "Carlos",
            TrainerLastName = "Gomez",
            ClassDateIds = new List<int> { 7, 8 }
        };

        Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(dto));
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Trainer>()), Times.Never);
    }

    // ─── AddClassDatesAsync ───────────────────────────────────────────────────

    [Test]
    public async Task AddClassDatesAsync_ValidList_ReturnsCountOfInserted()
    {
        var classDateIds = new List<int> { 1, 2, 3 };
        _repoMock.Setup(r => r.GetExistingClassDateIdsForClassAsync(5, It.IsAny<IEnumerable<int>>())).ReturnsAsync(classDateIds);
        _repoMock.Setup(r => r.AddClassDateAsync(It.IsAny<TrainerClassDate>())).ReturnsAsync(true);

        var result = await _service.AddClassDatesAsync(1, 5, classDateIds);

        Assert.That(result, Is.EqualTo(3));
    }

    [Test]
    public async Task AddClassDatesAsync_CallsRepositoryOncePerClassDateId()
    {
        var classDateIds = new List<int> { 5, 6, 7 };
        _repoMock.Setup(r => r.GetExistingClassDateIdsForClassAsync(5, It.IsAny<IEnumerable<int>>())).ReturnsAsync(classDateIds);
        _repoMock.Setup(r => r.AddClassDateAsync(It.IsAny<TrainerClassDate>())).ReturnsAsync(true);

        await _service.AddClassDatesAsync(1, 5, classDateIds);

        _repoMock.Verify(r => r.AddClassDateAsync(It.IsAny<TrainerClassDate>()), Times.Exactly(3));
    }

    [Test]
    public async Task AddClassDatesAsync_MapsIdsCorrectly()
    {
        var captured = new List<TrainerClassDate>();
        _repoMock.Setup(r => r.GetExistingClassDateIdsForClassAsync(5, It.IsAny<IEnumerable<int>>())).ReturnsAsync(new List<int> { 7, 8 });
        _repoMock.Setup(r => r.AddClassDateAsync(It.IsAny<TrainerClassDate>()))
            .Callback<TrainerClassDate>(tc => captured.Add(tc))
            .ReturnsAsync(true);

        await _service.AddClassDatesAsync(4, 5, new List<int> { 7, 8 });

        Assert.That(captured, Has.Count.EqualTo(2));
        Assert.That(captured[0].idtrainer, Is.EqualTo(4));
        Assert.That(captured[0].idclassdate, Is.EqualTo(7));
        Assert.That(captured[1].idtrainer, Is.EqualTo(4));
        Assert.That(captured[1].idclassdate, Is.EqualTo(8));
    }

    [Test]
    public async Task AddClassDatesAsync_EmptyList_ThrowsArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(() => _service.AddClassDatesAsync(1, 5, new List<int>()));
    }

    [Test]
    public async Task AddClassDatesAsync_EmptyList_DoesNotCallRepository()
    {
        try { await _service.AddClassDatesAsync(1, 5, new List<int>()); } catch { }

        _repoMock.Verify(r => r.AddClassDateAsync(It.IsAny<TrainerClassDate>()), Times.Never);
    }

    [Test]
    public async Task AddClassDatesAsync_NonExistentClassDateId_ThrowsArgumentException()
    {
        _repoMock.Setup(r => r.GetExistingClassDateIdsForClassAsync(5, It.IsAny<IEnumerable<int>>())).ReturnsAsync(new List<int> { 5 });

        Assert.ThrowsAsync<ArgumentException>(() => _service.AddClassDatesAsync(1, 5, new List<int> { 5, 99 }));
    }

    [Test]
    public async Task AddClassDatesAsync_NonExistentClassDateId_DoesNotCallRepository()
    {
        _repoMock.Setup(r => r.GetExistingClassDateIdsForClassAsync(5, It.IsAny<IEnumerable<int>>())).ReturnsAsync(new List<int> { 5 });

        try { await _service.AddClassDatesAsync(1, 5, new List<int> { 5, 99 }); } catch { }

        _repoMock.Verify(r => r.AddClassDateAsync(It.IsAny<TrainerClassDate>()), Times.Never);
    }

    [Test]
    public async Task AddClassDatesAsync_ClassDateBelongingToOtherClass_ThrowsArgumentException()
    {
        // classDateId 6 exists but belongs to a different class than the one requested
        _repoMock.Setup(r => r.GetExistingClassDateIdsForClassAsync(5, It.IsAny<IEnumerable<int>>())).ReturnsAsync(new List<int>());

        Assert.ThrowsAsync<ArgumentException>(() => _service.AddClassDatesAsync(1, 5, new List<int> { 6 }));
        _repoMock.Verify(r => r.AddClassDateAsync(It.IsAny<TrainerClassDate>()), Times.Never);
    }

    [Test]
    public async Task AddClassDatesAsync_WhenRepositoryThrows_PropagatesException()
    {
        _repoMock.Setup(r => r.GetExistingClassDateIdsForClassAsync(5, It.IsAny<IEnumerable<int>>())).ReturnsAsync(new List<int> { 5 });
        _repoMock.Setup(r => r.AddClassDateAsync(It.IsAny<TrainerClassDate>()))
            .ThrowsAsync(new InvalidOperationException("Error al asignar el horario al trainer"));

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddClassDatesAsync(1, 5, new List<int> { 5 }));
    }

    // ─── GetClassesWithStudentsAsync ──────────────────────────────────────────

    [Test]
    public async Task GetClassesWithStudentsAsync_ReturnsMappedDtos()
    {
        var repoResult = new List<(Classes, IEnumerable<Customer>)>
        {
            (
                new Classes
                {
                    id = 1, name = "Yoga",
                    Dates = new List<ClassDate> { new ClassDate { id = 1, idclass = 1, day = "Lunes", hour = "08:00", capacity = 10 } }
                },
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
        Assert.That(result[0].Dates.First().Day, Is.EqualTo("Lunes"));
        Assert.That(result[0].Dates.First().Hour, Is.EqualTo("08:00"));
        Assert.That(result[0].Dates.First().Capacity, Is.EqualTo(10));
        Assert.That(result[0].Students, Has.Count.EqualTo(1));
        Assert.That(result[0].Students[0].CustomerName, Is.EqualTo("Juan"));
        Assert.That(result[0].Students[0].CustomerLastName, Is.EqualTo("Perez"));
    }

    [Test]
    public async Task GetClassesWithStudentsAsync_OnlyReturnsAssignedScheduleDates()
    {
        // The class has 3 horarios in total, but the repository only returns the 2
        // assigned to this trainer (trainer_x_class_date), not the whole class.
        var repoResult = new List<(Classes, IEnumerable<Customer>)>
        {
            (
                new Classes
                {
                    id = 1, name = "Yoga",
                    Dates = new List<ClassDate>
                    {
                        new ClassDate { id = 1, idclass = 1, day = "Lunes", hour = "08:00", capacity = 10 },
                        new ClassDate { id = 2, idclass = 1, day = "Miercoles", hour = "08:00", capacity = 10 }
                    }
                },
                new List<Customer>()
            )
        };
        _repoMock.Setup(r => r.GetClassesWithStudentsAsync(1)).ReturnsAsync(repoResult);

        var result = (await _service.GetClassesWithStudentsAsync(1)).ToList();

        Assert.That(result[0].Dates, Has.Count.EqualTo(2));
        Assert.That(result[0].Dates.Select(d => d.Day), Is.EquivalentTo(new[] { "Lunes", "Miercoles" }));
        Assert.That(result[0].Dates.Any(d => d.Day == "Viernes"), Is.False);
    }

    [Test]
    public async Task GetClassesWithStudentsAsync_MultipleClasses_ReturnsMappedDtos()
    {
        var repoResult = new List<(Classes, IEnumerable<Customer>)>
        {
            (
                new Classes { id = 1, name = "Yoga" },
                new List<Customer> { new Customer { id = 1, customername = "Juan", customerlastname = "Perez" } }
            ),
            (
                new Classes { id = 2, name = "Pilates" },
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
                new Classes { id = 2, name = "Pilates" },
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
