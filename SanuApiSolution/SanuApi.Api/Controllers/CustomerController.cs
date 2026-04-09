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
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ICustomerService productAppService, ILogger<CustomerController> logger)
        {
            _customerService = productAppService;
            _logger = logger;
        }


        [HttpGet]
        [SwaggerOperation(Summary = "Obtiene todos los clientes", Description = "Devuelve una lista con todos los clientes disponibles en la base de datos")]
        [SwaggerResponse(200, "Lista de clientes", typeof(IEnumerable<CustomerFindResponseDto>))]
        public async Task<ActionResult<IEnumerable<CustomerFindResponseDto>>> GetAll([FromQuery] bool? active)
        {
            var customers = await _customerService.GetAllAsync(active);
            return Ok(customers);
        }


        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Obtiene un customer por ID", Description = "Devuelve un customer especifico si existe")]
        [SwaggerResponse(200, "Customer encontrado", typeof(CutsomerFindByIdResponseDto))]
        [SwaggerResponse(404, "No se encontro el cliente")]
        public async Task<ActionResult<CutsomerFindByIdResponseDto>> GetById(int id)
        {
            var coustomer = await _customerService.FindByIdAsync(id);
            if (coustomer == null) return NotFound();
            return Ok(coustomer);
        }


        [HttpPost]
        [SwaggerOperation(Summary = "Crea un nuevo cliente", Description = "Inserta un nuevo cliente en la base de datos")]
        [SwaggerResponse(201, "Cliente creado correctamente", typeof(int))]
        [SwaggerResponse(400, "Datos invalidos")]
        public async Task<ActionResult<int>> Create([FromBody] CustomerAddRequestDto dto)
        {
            try
            {
                var coustomerId = await _customerService.AddAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = coustomerId }, coustomerId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Actualiza un Cliente existente", Description = "Modifica los datos de un cliente por su ID")]
        [SwaggerResponse(200, "Cliente actualizado correctamente", typeof(bool))]
        [SwaggerResponse(404, "No se encontro el cliente")]
        public async Task<ActionResult<bool>> Update([FromBody] CustomerUpdateRequestDto dto)
        {
            _logger.LogInformation("[CustomerController.Update] Iniciando actualizacion para IdCustomer={IdCustomer}", dto?.IdCustomer);
            try
            {
                var customerUpdate = await _customerService.UpdateAsync(dto);
                if (!customerUpdate)
                {
                    _logger.LogWarning("[CustomerController.Update] Cliente no encontrado. IdCustomer={IdCustomer}", dto?.IdCustomer);
                    return NotFound();
                }
                _logger.LogInformation("[CustomerController.Update] Actualizacion exitosa. IdCustomer={IdCustomer}", dto?.IdCustomer);
                return Ok(customerUpdate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CustomerController.Update] ERROR al actualizar cliente. IdCustomer={IdCustomer}. Mensaje: {Message}. InnerException: {Inner}",
                    dto?.IdCustomer, ex.Message, ex.InnerException?.Message);
                return StatusCode(500, new { error = ex.Message, innerError = ex.InnerException?.Message });
            }
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Elimina un Cliente", Description = "Borra un Cliente de la base de datos por su ID")]
        [SwaggerResponse(204, "Cliente eliminado correctamente")]
        [SwaggerResponse(404, "No se encontro el cliente")]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _customerService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}/classes")]
        [SwaggerOperation(Summary = "Obtiene un listado de clases por ID", Description = "Devuelve un listado de clases")]
        [SwaggerResponse(200, "Customer encontrado", typeof(IEnumerable<ClassCustomerResponseDto>))]
        [SwaggerResponse(404, "No se encontr� las clases")]
        public async Task<ActionResult<CutsomerFindByIdResponseDto>> GetClasess(int id)
        {
            var coustomerClasses = await _customerService.GetClasses(id);
            if (coustomerClasses == null) return NotFound();
            return Ok(coustomerClasses);
        }

        [HttpPost("{customerId}/classes")]
        [SwaggerOperation(Summary = "Agrega clases a un cliente", Description = "Asocia una o m�s clases a un cliente existente")]
        [SwaggerResponse(StatusCodes.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddClassesToCustomer( int customerId, [FromBody] AddCustomerClassRequestDto request)
        {
            if (request == null || !request.ClassIds.Any())
                return BadRequest("Debe especificar al menos una clase.");

            await _customerService.AddClassesAsync(customerId, request);

            return Ok(true);
        }

        [HttpPost("{customerId}/membership")]
        [SwaggerOperation(Summary = "Agrega membres�a a un cliente", Description = "Asocia una o m�s membresia a un cliente existente")]
        [SwaggerResponse(StatusCodes.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddMembershipToCustomer(int customerId, [FromBody] AddCustomerMembershipRequestDto request)
        {
            if (request == null || !request.MembershipIds.Any())
                return BadRequest("Debe especificar al menos una membres�a.");

            await _customerService.AddMembershipAsync(customerId, request);

            return Ok(true);
        }

        [HttpPost("{customerId}/absence")]
        [SwaggerOperation(Summary = "Agrega asistencia a un cliente", Description = "Atoma asistencia al cliente")]
        [SwaggerResponse(StatusCodes.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddAbsenceToCustomer(int customerId, [FromBody] AddCustomerAbsenceRequestDto request)
        {
            if (request == null )
                return BadRequest("Debe especificar los datos de la asistencia");

            await _customerService.AddAbsenceAsync(customerId,request);

            return Ok(true);
        }
    }
}
