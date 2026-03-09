using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SanuApi.Aplication.Services;
using SanuApi.Api.Controllers;
using SanuApi.Application.DTOs.Customer;

namespace SanuApi.Api.Tests.Controllers;

[TestFixture]
public class CustomerControllerTests
{
    private Mock<ICustomerService> _serviceMock;
    private CustomerController _controller;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<ICustomerService>();
        _controller = new CustomerController(_serviceMock.Object);
    }

    // ─── GetAll ────────────────────────────────────────────────────────────────

    [Test]
    public async Task GetAll_ReturnsOkWithList()
    {
        var customers = new List<CustomerFindResponseDto>
        {
            new CustomerFindResponseDto { IdCoustomer = 1, CoustomerName = "Juan" },
            new CustomerFindResponseDto { IdCoustomer = 2, CoustomerName = "Ana" }
        };
        _serviceMock.Setup(s => s.GetAllAsync(null)).ReturnsAsync(customers);

        var actionResult = await _controller.GetAll(null);

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.StatusCode, Is.EqualTo(200));
        var data = ok.Value as IEnumerable<CustomerFindResponseDto>;
        Assert.That(data!.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetAll_WithActiveFilter_PassesFilterToService()
    {
        _serviceMock.Setup(s => s.GetAllAsync(true)).ReturnsAsync(new List<CustomerFindResponseDto>());

        await _controller.GetAll(true);

        _serviceMock.Verify(s => s.GetAllAsync(true), Times.Once);
    }

    // ─── GetById ───────────────────────────────────────────────────────────────

    [Test]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        var customer = new CutsomerFindByIdResponseDto { IdCustomer = 1, CustomerName = "Juan" };
        _serviceMock.Setup(s => s.FindByIdAsync(1)).ReturnsAsync(customer);

        var actionResult = await _controller.GetById(1);

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.FindByIdAsync(99)).ReturnsAsync((CutsomerFindByIdResponseDto?)null);

        var actionResult = await _controller.GetById(99);

        Assert.That(actionResult.Result, Is.InstanceOf<NotFoundResult>());
    }

    // ─── Create ────────────────────────────────────────────────────────────────

    [Test]
    public async Task Create_ValidDto_ReturnsCreatedAtAction()
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
        _serviceMock.Setup(s => s.AddAsync(dto)).ReturnsAsync(5);

        var actionResult = await _controller.Create(dto);

        var created = actionResult.Result as CreatedAtActionResult;
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.StatusCode, Is.EqualTo(201));
        Assert.That(created.Value, Is.EqualTo(5));
    }

    // ─── Update ────────────────────────────────────────────────────────────────

    [Test]
    public async Task Update_WhenSuccess_ReturnsOk()
    {
        var dto = new CustomerUpdateRequestDto { IdCustomer = 1, CustomerName = "Carlos" };
        _serviceMock.Setup(s => s.UpdateAsync(dto)).ReturnsAsync(true);

        var actionResult = await _controller.Update(dto);

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task Update_WhenNotFound_ReturnsNotFound()
    {
        var dto = new CustomerUpdateRequestDto { IdCustomer = 99 };
        _serviceMock.Setup(s => s.UpdateAsync(dto)).ReturnsAsync(false);

        var actionResult = await _controller.Update(dto);

        Assert.That(actionResult.Result, Is.InstanceOf<NotFoundResult>());
    }

    // ─── Delete ────────────────────────────────────────────────────────────────

    [Test]
    public async Task Delete_WhenSuccess_ReturnsNoContent()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        var actionResult = await _controller.Delete(1);

        Assert.That(actionResult, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task Delete_WhenNotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.DeleteAsync(99)).ReturnsAsync(false);

        var actionResult = await _controller.Delete(99);

        Assert.That(actionResult, Is.InstanceOf<NotFoundResult>());
    }

    // ─── GetClasess ────────────────────────────────────────────────────────────

    [Test]
    public async Task GetClasess_WhenFound_ReturnsOk()
    {
        var classes = new List<ClassCustomerResponseDto>
        {
            new ClassCustomerResponseDto { ClassId = 1, Name = "Yoga" }
        };
        _serviceMock.Setup(s => s.GetClasses(1)).ReturnsAsync(classes);

        var actionResult = await _controller.GetClasess(1);

        var ok = actionResult.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetClasess_WhenNull_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetClasses(99)).ReturnsAsync((IEnumerable<ClassCustomerResponseDto>?)null);

        var actionResult = await _controller.GetClasess(99);

        Assert.That(actionResult.Result, Is.InstanceOf<NotFoundResult>());
    }

    // ─── AddClassesToCustomer ──────────────────────────────────────────────────

    [Test]
    public async Task AddClassesToCustomer_ValidRequest_ReturnsOk()
    {
        var request = new AddCustomerClassRequestDto { ClassIds = new List<int> { 1, 2 } };
        _serviceMock.Setup(s => s.AddClassesAsync(1, request)).ReturnsAsync(true);

        var actionResult = await _controller.AddClassesToCustomer(1, request);

        var ok = actionResult as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task AddClassesToCustomer_NullRequest_ReturnsBadRequest()
    {
        var actionResult = await _controller.AddClassesToCustomer(1, null!);

        Assert.That(actionResult, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task AddClassesToCustomer_EmptyClassIds_ReturnsBadRequest()
    {
        var request = new AddCustomerClassRequestDto { ClassIds = new List<int>() };

        var actionResult = await _controller.AddClassesToCustomer(1, request);

        Assert.That(actionResult, Is.InstanceOf<BadRequestObjectResult>());
    }

    // ─── AddMembershipToCustomer ───────────────────────────────────────────────

    [Test]
    public async Task AddMembershipToCustomer_ValidRequest_ReturnsOk()
    {
        var request = new AddCustomerMembershipRequestDto
        {
            MembershipIds = new List<CustomerMemberShipRequestDto>
            {
                new CustomerMemberShipRequestDto { IdMembership = 1, StartDate = DateTime.Now }
            }
        };
        _serviceMock.Setup(s => s.AddMembershipAsync(1, request)).ReturnsAsync(true);

        var actionResult = await _controller.AddMembershipToCustomer(1, request);

        var ok = actionResult as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
    }

    [Test]
    public async Task AddMembershipToCustomer_NullRequest_ReturnsBadRequest()
    {
        var actionResult = await _controller.AddMembershipToCustomer(1, null!);

        Assert.That(actionResult, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task AddMembershipToCustomer_EmptyMembershipIds_ReturnsBadRequest()
    {
        var request = new AddCustomerMembershipRequestDto { MembershipIds = new List<CustomerMemberShipRequestDto>() };

        var actionResult = await _controller.AddMembershipToCustomer(1, request);

        Assert.That(actionResult, Is.InstanceOf<BadRequestObjectResult>());
    }

    // ─── AddAbsenceToCustomer ──────────────────────────────────────────────────

    [Test]
    public async Task AddAbsenceToCustomer_ValidRequest_ReturnsOk()
    {
        var request = new AddCustomerAbsenceRequestDto { IdClass = 1, IdCustomer = 1, DateAbsence = DateTime.Now };
        _serviceMock.Setup(s => s.AddAbsenceAsync(1, request)).ReturnsAsync(true);

        var actionResult = await _controller.AddAbsenceToCustomer(1, request);

        var ok = actionResult as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
    }

    [Test]
    public async Task AddAbsenceToCustomer_NullRequest_ReturnsBadRequest()
    {
        var actionResult = await _controller.AddAbsenceToCustomer(1, null!);

        Assert.That(actionResult, Is.InstanceOf<BadRequestObjectResult>());
    }
}
