using Swashbuckle.AspNetCore.Annotations;

namespace SanuApi.Application.DTOs.Trainer
{
    public class TrainerUpdateRequestDto
    {
        [SwaggerSchema("Nuevo nombre del trainer (opcional)")]
        public string? TrainerName { get; set; }

        [SwaggerSchema("Nuevo apellido del trainer (opcional)")]
        public string? TrainerLastName { get; set; }

        [SwaggerSchema("Nuevo email del trainer (opcional)")]
        public string? Email { get; set; }

        [SwaggerSchema("Nuevo teléfono del trainer (opcional)")]
        public string? Telephone { get; set; }
    }
}
