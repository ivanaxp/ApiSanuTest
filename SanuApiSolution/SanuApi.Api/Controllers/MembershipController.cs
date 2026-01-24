using Microsoft.AspNetCore.Mvc;
using SanuApi.Application.DTOs.Membership;
using SanuApi.Application.Interfaces;
using SanuApi.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace SanuApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembershipController : ControllerBase
    {
        private readonly IMembershipService _membershipService;

        public MembershipController(IMembershipService membershipAppService)
        {
            _membershipService = membershipAppService;
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Obtiene una membresía por ID", Description = "Devuelve una membresía específico si existe")]
        [SwaggerResponse(200, "Membresía encontrada", typeof(MembershipFindResponseDto))]
        [SwaggerResponse(404, "No se encontró la membresía")]
        public async Task<ActionResult<MembershipFindResponseDto>> GetById( int id)
        {
            var goal = await _membershipService.FindByIdAsync(id);
            if (goal == null) return NotFound();
            return Ok(goal);
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Obtiene todos las membresias", Description = "Devuelve una lista con todos las membresías disponibles en la base de datos")]
        [SwaggerResponse(200, "Lista de productos", typeof(IEnumerable<MembershipFindResponseDto>))]
        public async Task<ActionResult<IEnumerable<MembershipFindResponseDto>>> GetAll()
        {
            var goals = await _membershipService.GetAllAsync();
            return Ok(goals);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Crea un nuevo objetivo", Description = "Inserta una nueva nueva membresía en la base de datos")]
        [SwaggerResponse(201, "membresía creada correctamente", typeof(int))]
        [SwaggerResponse(400, "Datos inválidos")]
        public async Task<ActionResult<int>> Create([FromBody] MembershipAddRequestDto dto)
        {
            var membership = await _membershipService.AddAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = membership }, membership);
        }

        [HttpPut]
        [SwaggerOperation(Summary = "Da de baja una membresía", Description = "Inserta una nueva nueva membresía en la base de datos")]
        [SwaggerResponse(201, "membresía creada correctamente", typeof(bool))]
        [SwaggerResponse(400, "Datos inválidos")]
        public async Task<ActionResult<int>> Update([FromBody] MembershipUpdateRequestDto request)
        {
            var membership = await _membershipService.UpdateAsync(request);

            return Ok(membership);
        }

        [HttpDelete("{idMembership}")]
        [SwaggerOperation(Summary = "Da de baja una membresía", Description = "Elimina una membresía en la base de datos")]
        [SwaggerResponse(201, "membresía eliminada correctamente", typeof(bool))]
        [SwaggerResponse(400, "Datos inválidos")]
        public async Task<ActionResult<int>> Delete(int idMembership)
        {
            var membership = await _membershipService.DeleteAsync(idMembership);

            return Ok(membership);
        }

    }
}
