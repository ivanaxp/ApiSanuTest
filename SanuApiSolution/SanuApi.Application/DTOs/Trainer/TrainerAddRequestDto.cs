using Swashbuckle.AspNetCore.Annotations;

namespace SanuApi.Application.DTOs.Trainer
{
    public class TrainerAddRequestDto
    {
        [SwaggerSchema("Nombre del trainer")]
        public required string TrainerName { get; set; }

        [SwaggerSchema("Apellido del trainer")]
        public required string TrainerLastName { get; set; }

        [SwaggerSchema("Email del trainer (opcional)")]
        public string? Email { get; set; }

        [SwaggerSchema("Teléfono del trainer (opcional)")]
        public string? Telephone { get; set; }
    }
}
