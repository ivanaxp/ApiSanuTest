using Moq;
using NUnit.Framework;
using SanuApi.Application.DTOs.Membership;
using SanuApi.Application.Services;
using SanuApi.Domain.Entities;
using SanuApi.Domain.Interfaces;

namespace SanuApi.Application.Tests.Services;

[TestFixture]
public class MembershipServiceTests
{
    private Mock<IMembershipRepository> _membershipRepoMock;
    private MembershipService _service;

    [SetUp]
    public void SetUp()
    {
        _membershipRepoMock = new Mock<IMembershipRepository>();
        _service = new MembershipService(_membershipRepoMock.Object);
    }

    // ─── GetAllAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task GetAllAsync_ReturnsMappedList()
    {
        var memberships = new List<Membership>
        {
            new Membership { id = 1, name = "Mensual", price = 5000m, frecuency = 1 },
            new Membership { id = 2, name = "Anual",   price = 50000m, frecuency = 12 }
        };
        _membershipRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(memberships);

        var result = await _service.GetAllAsync();

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetAllAsync_EmptyRepository_ReturnsEmpty()
    {
        _membershipRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Membership>());

        var result = await _service.GetAllAsync();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetAllAsync_MapsFieldsCorrectly()
    {
        var memberships = new List<Membership>
        {
            new Membership { id = 1, name = "Mensual", price = 5000m, frecuency = 1 }
        };
        _membershipRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(memberships);

        var result = (await _service.GetAllAsync()).First();

        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Name, Is.EqualTo("Mensual"));
        Assert.That(result.Price, Is.EqualTo(5000m));
        Assert.That(result.Frecuency, Is.EqualTo(1));
    }

    // ─── FindByIdAsync ────────────────────────────────────────────────────────

    [Test]
    public async Task FindByIdAsync_WhenExists_ReturnsMappedDto()
    {
        var membership = new Membership { id = 1, name = "Mensual", price = 5000m, frecuency = 1 };
        _membershipRepoMock.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(membership);

        var result = await _service.FindByIdAsync(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(1));
        Assert.That(result.Name, Is.EqualTo("Mensual"));
    }

    [Test]
    public async Task FindByIdAsync_WhenNotExists_ReturnsNull()
    {
        _membershipRepoMock.Setup(r => r.FindByIdAsync(99)).ReturnsAsync((Membership?)null);

        var result = await _service.FindByIdAsync(99);

        Assert.That(result, Is.Null);
    }

    // ─── AddAsync ─────────────────────────────────────────────────────────────

    [Test]
    public async Task AddAsync_ValidDto_ReturnsNewId()
    {
        var dto = new MembershipAddRequestDto { Name = "Trimestral", Price = 12000m, Frecuency = 3 };
        _membershipRepoMock.Setup(r => r.AddAsync(It.IsAny<Membership>())).ReturnsAsync(4);

        var result = await _service.AddAsync(dto);

        Assert.That(result, Is.EqualTo(4));
    }

    [Test]
    public async Task AddAsync_MapsAllFieldsToEntity()
    {
        var dto = new MembershipAddRequestDto { Name = "Semestral", Price = 25000m, Frecuency = 6 };
        Membership? capturedEntity = null;
        _membershipRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Membership>()))
            .Callback<Membership>(m => capturedEntity = m)
            .ReturnsAsync(1);

        await _service.AddAsync(dto);

        Assert.That(capturedEntity!.name, Is.EqualTo("Semestral"));
        Assert.That(capturedEntity.price, Is.EqualTo(25000m));
        Assert.That(capturedEntity.frecuency, Is.EqualTo(6));
    }

    // ─── UpdateAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task UpdateAsync_ValidDto_ReturnsTrue()
    {
        var dto = new MembershipUpdateRequestDto { Id = 1, Name = "Mensual Plus", Price = 6000m, Frecuency = 1 };
        _membershipRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Membership>())).ReturnsAsync(true);

        var result = await _service.UpdateAsync(dto);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task UpdateAsync_MapsAllFieldsToEntity()
    {
        var dto = new MembershipUpdateRequestDto { Id = 2, Name = "Anual Pro", Price = 60000m, Frecuency = 12 };
        Membership? capturedEntity = null;
        _membershipRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Membership>()))
            .Callback<Membership>(m => capturedEntity = m)
            .ReturnsAsync(true);

        await _service.UpdateAsync(dto);

        Assert.That(capturedEntity!.id, Is.EqualTo(2));
        Assert.That(capturedEntity.name, Is.EqualTo("Anual Pro"));
        Assert.That(capturedEntity.price, Is.EqualTo(60000m));
    }

    [Test]
    public async Task UpdateAsync_WhenFails_ReturnsFalse()
    {
        var dto = new MembershipUpdateRequestDto { Id = 99, Name = "X" };
        _membershipRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Membership>())).ReturnsAsync(false);

        var result = await _service.UpdateAsync(dto);

        Assert.That(result, Is.False);
    }

    // ─── DeleteAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task DeleteAsync_ExistingMembership_ReturnsTrue()
    {
        _membershipRepoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _service.DeleteAsync(1);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task DeleteAsync_NotExistingMembership_ReturnsFalse()
    {
        _membershipRepoMock.Setup(r => r.DeleteAsync(99)).ReturnsAsync(false);

        var result = await _service.DeleteAsync(99);

        Assert.That(result, Is.False);
    }
}
