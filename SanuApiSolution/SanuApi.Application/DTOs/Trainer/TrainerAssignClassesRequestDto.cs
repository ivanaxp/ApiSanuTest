using Swashbuckle.AspNetCore.Annotations;

namespace SanuApi.Application.DTOs.Trainer
{
    public class TrainerAssignClassesRequestDto
    {
        [SwaggerSchema("Lista de IDs de clases a asignar al trainer")]
        public required List<int> ClassIds { get; set; }
    }
}
