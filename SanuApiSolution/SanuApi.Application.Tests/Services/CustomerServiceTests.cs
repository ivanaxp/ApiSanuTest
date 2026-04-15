using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SanuApi.Aplication.Services;
using SanuApi.Application.DTOs.Customer;
using SanuApi.Domain.Entities;
using SanuApi.Domain.Interfaces;
using System.Data;

namespace SanuApi.Application.Tests.Services;

[TestFixture]
public class CustomerServiceTests
{
    private Mock<ICustomerRepository> _customerRepoMock;
    private Mock<IHealthCustomerRepository> _healthRepoMock;
    private Mock<IGoalRepository> _goalRepoMock;
    private Mock<ICustomerMembershipRepository> _membershipRepoMock;
    private Mock<IDbConnection> _dbMock;
    private Mock<IDbTransaction> _transactionMock;
    private CustomerService _service;

    [SetUp]
    public void SetUp()
    {
        _customerRepoMock = new Mock<ICustomerRepository>();
        _healthRepoMock = new Mock<IHealthCustomerRepository>();
        _goalRepoMock = new Mock<IGoalRepository>();
        _membershipRepoMock = new Mock<ICustomerMembershipRepository>();
        _dbMock = new Mock<IDbConnection>();
        _transactionMock = new Mock<IDbTransaction>();

        _dbMock.Setup(d => d.State).Returns(ConnectionState.Closed);
        _dbMock.Setup(d => d.BeginTransaction()).Returns(_transactionMock.Object);

        _service = new CustomerService(
            _customerRepoMock.Object,
            _healthRepoMock.Object,
            _goalRepoMock.Object,
            _membershipRepoMock.Object,
            _dbMock.Object,
            new Mock<ILogger<CustomerService>>().Object
        );
    }

    // ─── GetAllAsync ───────────────────────────────────────────────────────────

    [Test]
    public async Task GetAllAsync_ReturnsAllCustomers()
    {
        var customers = new List<Customer>
        {
            new Customer { id = 1, customername = "Juan", customerlastname = "Perez", ismale = true, fechaalta = DateTime.Now },
            new Customer { id = 2, customername = "Ana", customerlastname = "Lopez", ismale = false, fechaalta = DateTime.Now }
        };
        _customerRepoMock.Setup(r => r.GetAllAsync(null)).ReturnsAsync(customers);

        var result = await _service.GetAllAsync(null);

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetAllAsync_WithActiveFilter_ReturnsFilteredCustomers()
    {
        var customers = new List<Customer>
        {
            new Customer { id = 1, customername = "Juan", customerlastname = "Perez", ismale = true, fechaalta = DateTime.Now, fechabaja = null }
        };
        _customerRepoMock.Setup(r => r.GetAllAsync(true)).ReturnsAsync(customers);

        var result = await _service.GetAllAsync(true);

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllAsync_EmptyList_ReturnsEmpty()
    {
        _customerRepoMock.Setup(r => r.GetAllAsync(null)).ReturnsAsync(new List<Customer>());

        var result = await _service.GetAllAsync(null);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetAllAsync_MapsGenderMasculinoCorrectly()
    {
        var customers = new List<Customer>
        {
            new Customer { id = 1, customername = "Juan", customerlastname = "Perez", ismale = true, fechaalta = DateTime.Now }
        };
        _customerRepoMock.Setup(r => r.GetAllAsync(null)).ReturnsAsync(customers);

        var result = (await _service.GetAllAsync(null)).First();

        Assert.That(result.Gender, Is.EqualTo(Gender.Masculino));
    }

    [Test]
    public async Task GetAllAsync_MapsGenderFemeninoCorrectly()
    {
        var customers = new List<Customer>
        {
            new Customer { id = 1, customername = "Maria", customerlastname = "Lopez", ismale = false, fechaalta = DateTime.Now }
        };
        _customerRepoMock.Setup(r => r.GetAllAsync(null)).ReturnsAsync(customers);

        var result = (await _service.GetAllAsync(null)).First();

        Assert.That(result.Gender, Is.EqualTo(Gender.Femenino));
    }

    // ─── FindByIdAsync ─────────────────────────────────────────────────────────

    [Test]
    public async Task FindByIdAsync_WhenExists_ReturnsDto()
    {
        var customer = new Customer
        {
            id = 1,
            customername = "Juan",
            customerlastname = "Perez",
            ismale = true,
            fechaalta = DateTime.Now,
            customerGoals = new List<CustomerGoal>(),
            customerMembership = new List<CustomerMembership>(),
            healthCustomer = new HealthCustomer { alergics = "Ninguna", heigth = 1.75m, weight = 70m }
        };
        _customerRepoMock.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(customer);

        var result = await _service.FindByIdAsync(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IdCustomer, Is.EqualTo(1));
        Assert.That(result.CustomerName, Is.EqualTo("Juan"));
    }

    [Test]
    public async Task FindByIdAsync_WhenNotExists_ReturnsNull()
    {
        _customerRepoMock.Setup(r => r.FindByIdAsync(99)).ReturnsAsync((Customer?)null);

        var result = await _service.FindByIdAsync(99);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task FindByIdAsync_MapsGoalsCorrectly()
    {
        var customer = new Customer
        {
            id = 1,
            customername = "Juan",
            customerlastname = "Perez",
            ismale = null,
            fechaalta = DateTime.Now,
            customerGoals = new List<CustomerGoal>
            {
                new CustomerGoal { goalid = 1, Goal = new Goal { goalname = "Perder peso" } }
            },
            customerMembership = new List<CustomerMembership>(),
            healthCustomer = new HealthCustomer { heigth = 1.70m, weight = 65m }
        };
        _customerRepoMock.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(customer);

        var result = await _service.FindByIdAsync(1);

        Assert.That(result!.Goals, Has.Count.EqualTo(1));
        Assert.That(result.Goals[0].GoalName, Is.EqualTo("Perder peso"));
    }

    // ─── AddAsync ─────────────────────────────────────────────────────────────

    [Test]
    public async Task AddAsync_SimpleCustomer_ReturnsNewId()
    {
        var dto = new CustomerAddRequestDto
        {
            CustomerName = "Juan",
            CustomerLastName = "Perez",
            DateBirth = new DateTime(1990, 1, 1),
            Dni = 12345678,
            Celphone = "1111111",
            Address = "Calle 1",
            Health = new HealthCustomerDto { Height = 1.75m, Weight = 70m, Alergics = "Ninguna" },
            idGoal = new List<int>(),
            Memberships = new List<CustomerMemberShipRequestDto>(),
            CustomerClasses = new List<int>()
        };

        _customerRepoMock.Setup(r => r.AddAsync(It.IsAny<Customer>())).ReturnsAsync(5);
        _healthRepoMock.Setup(r => r.AddAsync(It.IsAny<HealthCustomer>())).ReturnsAsync(1);

        var result = await _service.AddAsync(dto);

        Assert.That(result, Is.EqualTo(5));
        _transactionMock.Verify(t => t.Commit(), Times.Once);
    }

    [Test]
    public async Task AddAsync_WithGoals_AddsCustomerGoals()
    {
        var dto = new CustomerAddRequestDto
        {
            CustomerName = "Juan",
            CustomerLastName = "Perez",
            DateBirth = new DateTime(1990, 1, 1),
            Dni = 12345678,
            Celphone = "1111111",
            Address = "Calle 1",
            Health = new HealthCustomerDto { Height = 1.75m, Weight = 70m, Alergics = "Ninguna" },
            idGoal = new List<int> { 1, 2 },
            Memberships = new List<CustomerMemberShipRequestDto>(),
            CustomerClasses = new List<int>()
        };

        _customerRepoMock.Setup(r => r.AddAsync(It.IsAny<Customer>())).ReturnsAsync(5);
        _healthRepoMock.Setup(r => r.AddAsync(It.IsAny<HealthCustomer>())).ReturnsAsync(1);
        _goalRepoMock.Setup(r => r.AddCustomerGoalAsync(It.IsAny<CustomerGoal>())).ReturnsAsync(1);

        var result = await _service.AddAsync(dto);

        Assert.That(result, Is.EqualTo(5));
        _goalRepoMock.Verify(r => r.AddCustomerGoalAsync(It.IsAny<CustomerGoal>()), Times.Exactly(2));
    }

    [Test]
    public async Task AddAsync_WhenCustomerInsertFails_RollsBackAndThrows()
    {
        var dto = new CustomerAddRequestDto
        {
            CustomerName = "Juan",
            CustomerLastName = "Perez",
            DateBirth = new DateTime(1990, 1, 1),
            Dni = 12345678,
            Celphone = "1111111",
            Address = "Calle 1",
            Health = new HealthCustomerDto { Height = 1.75m, Weight = 70m },
            idGoal = new List<int>(),
            Memberships = new List<CustomerMemberShipRequestDto>(),
            CustomerClasses = new List<int>()
        };

        _customerRepoMock.Setup(r => r.AddAsync(It.IsAny<Customer>())).ReturnsAsync(0);

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddAsync(dto));
        _transactionMock.Verify(t => t.Rollback(), Times.Once);
    }

    // ─── UpdateAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task UpdateAsync_ExistingCustomer_ReturnsTrue()
    {
        var existingCustomer = new Customer
        {
            id = 1,
            customername = "Juan",
            customerlastname = "Perez",
            fechaalta = DateTime.Now,
            customerGoals = new List<CustomerGoal>(),
            customerMembership = new List<CustomerMembership>(),
            healthCustomer = new HealthCustomer()
        };
        var dto = new CustomerUpdateRequestDto { IdCustomer = 1, CustomerName = "Carlos" };

        _customerRepoMock.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(existingCustomer);
        _customerRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Customer>())).ReturnsAsync(true);

        var result = await _service.UpdateAsync(dto);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task UpdateAsync_NotExistingCustomer_ThrowsException()
    {
        _customerRepoMock.Setup(r => r.FindByIdAsync(99)).ReturnsAsync((Customer?)null);
        var dto = new CustomerUpdateRequestDto { IdCustomer = 99 };

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(dto));
    }

    [Test]
    public async Task UpdateAsync_WithClasses_DeletesAllThenInsertsNew()
    {
        var existing = new Customer
        {
            id = 1, customername = "Juan", customerlastname = "Perez",
            fechaalta = DateTime.Now,
            customerGoals = new List<CustomerGoal>(),
            customerMembership = new List<CustomerMembership>(),
            healthCustomer = new HealthCustomer()
        };
        var dto = new CustomerUpdateRequestDto
        {
            IdCustomer = 1,
            CustomerClasses = new List<int> { 3, 4 }
        };

        _customerRepoMock.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(existing);
        _customerRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Customer>())).ReturnsAsync(true);
        _customerRepoMock.Setup(r => r.DeleteClassesAsync(1)).ReturnsAsync(true);
        _customerRepoMock.Setup(r => r.AddClassesAsync(It.IsAny<ClassCustomer>())).ReturnsAsync(1);

        await _service.UpdateAsync(dto);

        _customerRepoMock.Verify(r => r.DeleteClassesAsync(1), Times.Once);
        _customerRepoMock.Verify(r => r.AddClassesAsync(It.IsAny<ClassCustomer>()), Times.Exactly(2));
    }

    [Test]
    public async Task UpdateAsync_WithEmptyClassList_DeletesAllAndInsertsNothing()
    {
        var existing = new Customer
        {
            id = 1, customername = "Juan", customerlastname = "Perez",
            fechaalta = DateTime.Now,
            customerGoals = new List<CustomerGoal>(),
            customerMembership = new List<CustomerMembership>(),
            healthCustomer = new HealthCustomer()
        };
        var dto = new CustomerUpdateRequestDto
        {
            IdCustomer = 1,
            CustomerClasses = new List<int>()
        };

        _customerRepoMock.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(existing);
        _customerRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Customer>())).ReturnsAsync(true);
        _customerRepoMock.Setup(r => r.DeleteClassesAsync(1)).ReturnsAsync(true);

        await _service.UpdateAsync(dto);

        _customerRepoMock.Verify(r => r.DeleteClassesAsync(1), Times.Once);
        _customerRepoMock.Verify(r => r.AddClassesAsync(It.IsAny<ClassCustomer>()), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_WithNullClasses_DoesNotTouchClasses()
    {
        var existing = new Customer
        {
            id = 1, customername = "Juan", customerlastname = "Perez",
            fechaalta = DateTime.Now,
            customerGoals = new List<CustomerGoal>(),
            customerMembership = new List<CustomerMembership>(),
            healthCustomer = new HealthCustomer()
        };
        var dto = new CustomerUpdateRequestDto
        {
            IdCustomer = 1,
            CustomerName = "Carlos",
            CustomerClasses = null
        };

        _customerRepoMock.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(existing);
        _customerRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Customer>())).ReturnsAsync(true);

        await _service.UpdateAsync(dto);

        _customerRepoMock.Verify(r => r.DeleteClassesAsync(It.IsAny<int>()), Times.Never);
        _customerRepoMock.Verify(r => r.AddClassesAsync(It.IsAny<ClassCustomer>()), Times.Never);
    }

    // ─── DeleteAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task DeleteAsync_ExistingCustomer_ReturnsTrue()
    {
        _customerRepoMock.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(new Customer { id = 1 });
        _customerRepoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _service.DeleteAsync(1);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task DeleteAsync_NotFound_ReturnsFalse()
    {
        _customerRepoMock.Setup(r => r.FindByIdAsync(99)).ReturnsAsync((Customer?)null);
        _customerRepoMock.Setup(r => r.DeleteAsync(99)).ReturnsAsync(false);

        var result = await _service.DeleteAsync(99);

        Assert.That(result, Is.False);
    }

    // ─── GetClasses ───────────────────────────────────────────────────────────

    [Test]
    public async Task GetClasses_ReturnsClassesMapped()
    {
        var classes = new List<Classes>
        {
            new Classes { id = 1, name = "Yoga", day = "Lunes", hour = "08:00", capacity = 10 }
        };
        _customerRepoMock.Setup(r => r.GetClassesAsync(1)).ReturnsAsync(classes);

        var result = await _service.GetClasses(1);

        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo("Yoga"));
    }

    // ─── AddClassesAsync ──────────────────────────────────────────────────────

    [Test]
    public async Task AddClassesAsync_WithValidIds_ReturnsTrue()
    {
        var dto = new AddCustomerClassRequestDto { ClassIds = new List<int> { 1, 2 } };
        _customerRepoMock.Setup(r => r.AddClassesAsync(It.IsAny<ClassCustomer>())).ReturnsAsync(1);

        var result = await _service.AddClassesAsync(1, dto);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task AddClassesAsync_EmptyList_ReturnsFalse()
    {
        var dto = new AddCustomerClassRequestDto { ClassIds = new List<int>() };

        var result = await _service.AddClassesAsync(1, dto);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task AddClassesAsync_WhenInsertFails_ReturnsFalse()
    {
        var dto = new AddCustomerClassRequestDto { ClassIds = new List<int> { 1 } };
        _customerRepoMock.Setup(r => r.AddClassesAsync(It.IsAny<ClassCustomer>())).ReturnsAsync(0);

        var result = await _service.AddClassesAsync(1, dto);

        Assert.That(result, Is.False);
    }

    // ─── AddMembershipAsync ───────────────────────────────────────────────────

    [Test]
    public async Task AddMembershipAsync_WithValidData_ReturnsTrue()
    {
        var dto = new AddCustomerMembershipRequestDto
        {
            MembershipIds = new List<CustomerMemberShipRequestDto>
            {
                new CustomerMemberShipRequestDto { IdMembership = 1, StartDate = DateTime.Now }
            }
        };
        _membershipRepoMock.Setup(r => r.AddAsync(It.IsAny<CustomerMembership>())).ReturnsAsync(1);

        var result = await _service.AddMembershipAsync(1, dto);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task AddMembershipAsync_EmptyList_ReturnsFalse()
    {
        var dto = new AddCustomerMembershipRequestDto { MembershipIds = new List<CustomerMemberShipRequestDto>() };

        var result = await _service.AddMembershipAsync(1, dto);

        Assert.That(result, Is.False);
    }

    // ─── AddAbsenceAsync ──────────────────────────────────────────────────────

    [Test]
    public async Task AddAbsenceAsync_ReturnsTrue()
    {
        var dto = new AddCustomerAbsenceRequestDto { IdClass = 1, IdCustomer = 1, DateAbsence = DateTime.Now };
        _customerRepoMock.Setup(r => r.AddAbsenceAsync(It.IsAny<Absences>())).ReturnsAsync(1);

        var result = await _service.AddAbsenceAsync(1, dto);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task AddAbsenceAsync_WhenInsertFails_ReturnsFalse()
    {
        var dto = new AddCustomerAbsenceRequestDto { IdClass = 1, IdCustomer = 1, DateAbsence = DateTime.Now };
        _customerRepoMock.Setup(r => r.AddAbsenceAsync(It.IsAny<Absences>())).ReturnsAsync(0);

        var result = await _service.AddAbsenceAsync(1, dto);

        Assert.That(result, Is.False);
    }
}
