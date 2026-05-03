using Microsoft.AspNetCore.Mvc;
using SanuApi.Application.DTOs.Class;
using SanuApi.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace SanuApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classlService;

        public ClassController(IClassService classAppService)
        {
            _classlService = classAppService;
        }

        
        [HttpGet]
        [SwaggerOperation(Summary = "Obtiene todas las clases", Description = "Devuelve una lista con todos las clases disponibles en la base de datos")]
        [SwaggerResponse(200, "Lista de productos", typeof(IEnumerable<ClassFindResponseDto>))]
        public async Task<ActionResult<IEnumerable<ClassFindResponseDto>>> GetAll()
        {
            var classes = await _classlService.GetAllAsync();
            return Ok(classes);
        }


        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Obtiene un producto por ID", Description = "Devuelve una clase espec�fico si existe")]
        [SwaggerResponse(200, "Clase encontrado", typeof(ClassFindByIdResponseDto))]
        [SwaggerResponse(404, "No se encontr� la clase")]
        public async Task<ActionResult<ClassFindResponseDto>> GetById(int id)
        {
            var clase = await _classlService.FindByIdAsync(id);
            if (clase == null) return NotFound();
            return Ok(clase);
        }

        [HttpGet("{id}/customers")]
        [SwaggerOperation(Summary = "Obtiene los clientes de una clase", Description = "Devuelve la clase con el listado de clientes activos inscriptos")]
        [SwaggerResponse(200, "Clase con clientes", typeof(ClassWithCustomersResponseDto))]
        [SwaggerResponse(404, "No se encontró la clase")]
        public async Task<ActionResult<ClassWithCustomersResponseDto>> GetWithCustomers(int id)
        {
            var result = await _classlService.GetWithCustomersAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Crea un nuevo objetivo", Description = "Inserta un nuevo objetivo en la base de datos")]
        [SwaggerResponse(201, "Objetivo creado correctamente", typeof(int))]
        [SwaggerResponse(400, "Datos inv�lidos")]
        public async Task<ActionResult<int>> Create([FromBody] AddClassRequestDto dto)
        {
            var clase = await _classlService.AddAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = clase }, clase);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Elimina un objetivo", Description = "Elimina un objetivo en la base de datos")]
        [SwaggerResponse(201, "Objetivo creado correctamente", typeof(bool))]
        [SwaggerResponse(400, "Datos inv�lidos")]
        public async Task<ActionResult<int>> Delete(int id)
        {
            var clase = await _classlService.DeleteAsync(id);

            return CreatedAtAction(nameof(GetById), new { id = clase }, clase);
        }

        [HttpGet("{id}/attendance")]
        [SwaggerOperation(
            Summary = "Obtiene la asistencia de una clase en una fecha",
            Description = "Devuelve todos los alumnos inscriptos con su estado ('presente', 'ausente', 'ausente_justificado'). Los alumnos sin registro aparecen con status null. FreeSpotsToday indica cuántos lugares quedaron libres por ausencias justificadas.")]
        [SwaggerResponse(200, "Asistencia de la clase", typeof(ClassAttendanceResponseDto))]
        [SwaggerResponse(404, "No se encontró la clase")]
        public async Task<ActionResult<ClassAttendanceResponseDto>> GetAttendanceByDate(int id, [FromQuery] DateTime date)
        {
            var result = await _classlService.GetAttendanceByDateAsync(id, date);
            if (result == null) return NotFound();
            return Ok(result);
        }

    }
}
