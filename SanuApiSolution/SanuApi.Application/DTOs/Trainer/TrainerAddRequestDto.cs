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

        [SwaggerSchema("ID de la clase cuyos horarios se asignan al trainer al momento del alta (opcional, requerido si se especifica ClassDateIds)")]
        public int? ClassId { get; set; }

        [SwaggerSchema("Lista de IDs de horarios (class_date) de esa clase a asignar al trainer al momento del alta (opcional)")]
        public List<int>? ClassDateIds { get; set; }
    }
}
