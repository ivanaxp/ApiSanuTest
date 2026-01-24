using Microsoft.AspNetCore.Mvc;
using SanuApi.Aplication.Services;
using SanuApi.Application.DTOs.Customer;
using SanuApi.Application.DTOs.Goal;
using SanuApi.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace SanuApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GoalController : ControllerBase
    {
        private readonly IGoalService _goalService;

        public GoalController(IGoalService goalAppService)
        {
            _goalService = goalAppService;
        }

        
        [HttpGet]
        [SwaggerOperation(Summary = "Obtiene todos los productos", Description = "Devuelve una lista con todos los productos disponibles en la base de datos")]
        [SwaggerResponse(200, "Lista de objetivos", typeof(IEnumerable<GoalFindResponseDto>))]
        public async Task<ActionResult<IEnumerable<GoalFindResponseDto>>> GetAll()
        {
            var goals = await _goalService.GetAllAsync();
            return Ok(goals);
        }

        
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Obtiene un objetivo por ID", Description = "Devuelve un objetivo específico si existe")]
        [SwaggerResponse(200, "Objetivo encontrado", typeof(GoalFindResponseDto))]
        [SwaggerResponse(404, "No se encontró el objetivo")]
        public async Task<ActionResult<GoalFindResponseDto>> GetById(int id)
        {
            var goal = await _goalService.FindByIdAsync(id);
            if (goal == null) return NotFound();
            return Ok(goal);
        }

        
        [HttpPost]
        [SwaggerOperation(Summary = "Crea un nuevo objetivo", Description = "Inserta un nuevo objetivo en la base de datos")]
        [SwaggerResponse(201, "Objetivo creado correctamente", typeof(int))]
        [SwaggerResponse(400, "Datos inválidos")]
        public async Task<ActionResult<int>> Create([FromBody] AddGoalRequestDto dto)
        {
            var goal = await _goalService.AddAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = goal }, goal);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Elimina un objetivo", Description = "Elimina un objetivo en la base de datos")]
        [SwaggerResponse(201, "Objetivo creado correctamente", typeof(bool))]
        [SwaggerResponse(400, "Datos inválidos")]
        public async Task<ActionResult<int>> Delete(int id)
        {
            var goal = await _goalService.DeleteAsync(id);

            return CreatedAtAction(nameof(GetById), new { id = goal }, goal);
        }


    }
}
