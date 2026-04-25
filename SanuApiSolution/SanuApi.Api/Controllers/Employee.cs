using Microsoft.AspNetCore.Mvc;
using SanuApi.Application.DTOs.Trainer;
using SanuApi.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace SanuApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly ITrainerService _trainerService;

        public EmployeeController(ITrainerService trainerService)
        {
            _trainerService = trainerService;
        }

        [HttpPost("trainer")]
        [SwaggerOperation(Summary = "Registra un nuevo trainer")]
        [SwaggerResponse(201, "Trainer creado correctamente", typeof(int))]
        [SwaggerResponse(400, "Datos invalidos")]
        public async Task<ActionResult<int>> CreateTrainer([FromBody] TrainerAddRequestDto dto)
        {
            try
            {
                var id = await _trainerService.AddAsync(dto);
                return StatusCode(201, id);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("trainer/{trainerId}/classes")]
        [SwaggerOperation(Summary = "Asigna un conjunto de clases a un trainer")]
        [SwaggerResponse(201, "Clases asignadas correctamente. Retorna la cantidad de clases asignadas.", typeof(int))]
        [SwaggerResponse(400, "Datos invalidos")]
        public async Task<ActionResult<int>> AssignClasses(int trainerId, [FromBody] TrainerAssignClassesRequestDto dto)
        {
            try
            {
                var count = await _trainerService.AddClassesAsync(trainerId, dto.ClassIds);
                return StatusCode(201, count);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("trainer/{trainerId}/classes")]
        [SwaggerOperation(Summary = "Lista las clases de un trainer con sus horarios y alumnos")]
        [SwaggerResponse(200, "Clases del trainer", typeof(IEnumerable<TrainerClassWithStudentsResponseDto>))]
        public async Task<ActionResult<IEnumerable<TrainerClassWithStudentsResponseDto>>> GetTrainerClasses(int trainerId)
        {
            var classes = await _trainerService.GetClassesWithStudentsAsync(trainerId);
            return Ok(classes);
        }

        [HttpPut("trainer/{trainerId}")]
        [SwaggerOperation(Summary = "Edita un trainer", Description = "Actualiza nombre y/o apellido de un trainer activo")]
        [SwaggerResponse(204, "Trainer actualizado correctamente")]
        [SwaggerResponse(404, "No se encontro el trainer")]
        public async Task<ActionResult> UpdateTrainer(int trainerId, [FromBody] TrainerUpdateRequestDto dto)
        {
            var updated = await _trainerService.UpdateAsync(trainerId, dto);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("trainer/{trainerId}")]
        [SwaggerOperation(Summary = "Elimina un trainer", Description = "Realiza la baja logica del trainer seteando su fecha de baja")]
        [SwaggerResponse(204, "Trainer dado de baja correctamente")]
        [SwaggerResponse(404, "No se encontro el trainer")]
        public async Task<ActionResult> DeleteTrainer(int trainerId)
        {
            var deleted = await _trainerService.DeleteAsync(trainerId);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
