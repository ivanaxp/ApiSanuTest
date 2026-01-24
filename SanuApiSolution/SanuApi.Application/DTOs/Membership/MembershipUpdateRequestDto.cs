using Swashbuckle.AspNetCore.Annotations;

namespace SanuApi.Application.DTOs.Membership
{
    public class MembershipUpdateRequestDto
    {
        /// <summary>
        /// Id de la membresía.
        /// </summary>
        [SwaggerSchema("Id de la membresía.")]
        public int Id { get; set; }

        /// <summary>
        /// Nombre de la membresía.
        /// </summary>
        [SwaggerSchema("Nombre de la membresía.")]
        public string Name { get; set; }

        /// <summary>
        /// Precio de la membresía.
        /// </summary>
        [SwaggerSchema("Precio de la membresia")]
        public decimal Price { get; set; }

        /// <summary>
        /// Frecuencia de concurrencia de la membresía.
        /// </summary>
        [SwaggerSchema("Frecuencia de concurrencia de la membresía.")]
        public int Frecuency { get; set; }
    }
}
