using Swashbuckle.AspNetCore.Annotations;

namespace SanuApi.Application.DTOs.Trainer
{
    public class TrainerAddRequestDto
    {
        [SwaggerSchema("Nombre del trainer")]
        public required string TrainerName { get; set; }

        [SwaggerSchema("Apellido del trainer")]
        public required string TrainerLastName { get; set; }
    }
}
