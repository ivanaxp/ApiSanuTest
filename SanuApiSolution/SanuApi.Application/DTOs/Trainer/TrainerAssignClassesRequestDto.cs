using Swashbuckle.AspNetCore.Annotations;

namespace SanuApi.Application.DTOs.Trainer
{
    public class TrainerAssignClassesRequestDto
    {
        [SwaggerSchema("ID de la clase a la que pertenecen los horarios")]
        public required int ClassId { get; set; }

        [SwaggerSchema("Lista de IDs de horarios (class_date) de esa clase a asignar al trainer")]
        public required List<int> ClassDateIds { get; set; }
    }
}
