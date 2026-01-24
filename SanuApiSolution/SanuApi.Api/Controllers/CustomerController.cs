using Microsoft.AspNetCore.Mvc;
using SanuApi.Aplication.Services;
using SanuApi.Application.DTOs.Customer;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections;

namespace SanuApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService productAppService)
        {
            _customerService = productAppService;
        }


        [HttpGet]
        [SwaggerOperation(Summary = "Obtiene todos los productos", Description = "Devuelve una lista con todos los productos disponibles en la base de datos")]
        [SwaggerResponse(200, "Lista de productos", typeof(IEnumerable<CustomerFindResponseDto>))]
        public async Task<ActionResult<IEnumerable<CustomerFindResponseDto>>> GetAll([FromQuery] bool? active)
        {
            var customers = await _customerService.GetAllAsync(active);
            return Ok(customers);
        }


        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Obtiene un customer por ID", Description = "Devuelve un customer específico si existe")]
        [SwaggerResponse(200, "Customer encontrado", typeof(CutsomerFindByIdResponseDto))]
        [SwaggerResponse(404, "No se encontró el producto")]
        public async Task<ActionResult<CutsomerFindByIdResponseDto>> GetById(int id)
        {
            var coustomer = await _customerService.FindByIdAsync(id);
            if (coustomer == null) return NotFound();
            return Ok(coustomer);
        }


        [HttpPost]
        [SwaggerOperation(Summary = "Crea un nuevo producto", Description = "Inserta un nuevo producto en la base de datos")]
        [SwaggerResponse(201, "Producto creado correctamente", typeof(int))]
        [SwaggerResponse(400, "Datos inválidos")]
        public async Task<ActionResult<int>> Create([FromBody] CustomerAddRequestDto dto)
        {
            var coustomerId = await _customerService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = coustomerId }, coustomerId);
        }


        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Actualiza un producto existente", Description = "Modifica los datos de un producto por su ID")]
        [SwaggerResponse(200, "Producto actualizado correctamente", typeof(bool))]
        [SwaggerResponse(404, "No se encontró el producto")]
        public async Task<ActionResult<bool>> Update([FromBody] CustomerUpdateRequestDto dto)
        {
            var customerUpdate = await _customerService.UpdateAsync(dto);
            if (!customerUpdate) return NotFound();
            return Ok(customerUpdate);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Elimina un producto", Description = "Borra un producto de la base de datos por su ID")]
        [SwaggerResponse(204, "Producto eliminado correctamente")]
        [SwaggerResponse(404, "No se encontró el producto")]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _customerService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}/classes")]
        [SwaggerOperation(Summary = "Obtiene un listado de clases por ID", Description = "Devuelve un listado de clases")]
        [SwaggerResponse(200, "Customer encontrado", typeof(IEnumerable<ClassCustomerResponseDto>))]
        [SwaggerResponse(404, "No se encontró las clases")]
        public async Task<ActionResult<CutsomerFindByIdResponseDto>> GetClasess(int id)
        {
            var coustomerClasses = await _customerService.GetClasses(id);
            if (coustomerClasses == null) return NotFound();
            return Ok(coustomerClasses);
        }

        [HttpPost("{customerId}/classes")]
        [SwaggerOperation(Summary = "Agrega clases a un cliente", Description = "Asocia una o más clases a un cliente existente")]
        [SwaggerResponse(StatusCodes.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddClassesToCustomer( int customerId, [FromBody] AddCustomerClassRequestDto request)
        {
            if (request == null || !request.ClassIds.Any())
                return BadRequest("Debe especificar al menos una clase.");

            await _customerService.AddClassesAsync(customerId, request);

            return NoContent();
        }

        [HttpPost("{customerId}/membership")]
        [SwaggerOperation(Summary = "Agrega membresía a un cliente", Description = "Asocia una o más membresia a un cliente existente")]
        [SwaggerResponse(StatusCodes.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddMembershipToCustomer(int customerId, [FromBody] AddCustomerMembershipRequestDto request)
        {
            if (request == null || !request.MembershipIds.Any())
                return BadRequest("Debe especificar al menos una membresía.");

            await _customerService.AddMembershipAsync(customerId, request);

            return NoContent();
        }

        [HttpPost("{customerId}/absence")]
        [SwaggerOperation(Summary = "Agrega asistencia a un cliente", Description = "Atoma asistencia al cliente")]
        [SwaggerResponse(StatusCodes.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddAbsenceToCustomer(int customerId, [FromBody] AddCustomerMembershipRequestDto request)
        {
            if (request == null || !request.MembershipIds.Any())
                return BadRequest("Debe especificar al menos una asistencia.");

            await _customerService.AddMembershipAsync(customerId, request);

            return NoContent();
        }
    }
}
