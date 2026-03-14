
using Swashbuckle.AspNetCore.Annotations;

namespace SanuApi.Application.DTOs.Customer
{
    public class CustomerAddRequestDto
    {
        /// <summary>
        /// Corresponde al nombre del cliente.
        /// </summary>
        [SwaggerSchema("Corresponde al nombre del cliente.")]
        public required string CustomerName { get; set; }
        /// <summary>
        /// Corresponde al Apellido del cliente.
        /// </summary>
        [SwaggerSchema("Corresponde al Apellido del cliente.")]
        public required string CustomerLastName { get; set; }
        /// <summary>
        /// Corresponde a la fecha de nacimiento del cliente. Respeta el formato YYYY-MM-DD.
        /// </summary>
        [SwaggerSchema("Corresponde a la fecha de nacimiento del cliente. Respeta el formato YYYY-MM-DD.")]
        public required DateTime DateBirth { get; set; }
        /// <summary>
        /// Corresponde al documento nacional de identidad del cliente.
        /// </summary>
        [SwaggerSchema("Corresponde al documento nacional de identidad del cliente.")]
        public required int Dni { get; set; }
        /// <summary>
        /// Corresponde al número de celular del cliente.
        /// </summary>
        /// 
        [SwaggerSchema("Corresponde al número de celular del cliente.")]
        public required string Celphone { get; set; }
        /// <summary>
        /// Corresponde a la dirección del cliente.
        /// </summary>
        /// 
        [SwaggerSchema("Corresponde a la dirección del cliente.")]
        public required string Address { get; set; }

        /// <summary>
        /// Indica si es masculino o femenino. En caso de no especificar, se asume que es desconocido.
        /// </summary>
        /// 
        [SwaggerSchema("Indica si es masculino o femenino. En caso de no especificar, se asume que es desconocido.")]
        public bool? IsMale { get; set; }

        /// <summary>
        /// Correponde a las observaciones del cliente al momento del alta del mismo.
        /// </summary>
        /// 
        [SwaggerSchema("Correponde a las observaciones del cliente al momento del alta del mismo.")]
        public string Comentaries { get; set; }

        /// <summary>
        /// Corresponde a los ids de los objetivos seleccionado.
        /// </summary>
        /// 
        [SwaggerSchema("Corresponde a los ids de los bjetivos seleccionado.")]
        public required List<int> idGoal{ get; set; }

        /// <summary>
        /// Corresponde a los datos de salud del cliente.o.
        /// </summary>
        /// 
        [SwaggerSchema("Corresponde a los datos de salud del cliente.")]
        public HealthCustomerDto? Health { get; set; }

        /// <summary>
        /// Corresponde a las membresias del cliente.
        /// </summary>
        ///
        [SwaggerSchema("Corresponde a los ids de los ojetivos seleccionado.")]
        public required List<CustomerMemberShipRequestDto> Memberships { get; set; }

        /// <summary>
        /// Corresponde a las clases del cliente.
        /// </summary>
        ///
        [SwaggerSchema("Corresponde a los ids de las clases seleccionadas.")]
        public required List<int> CustomerClasses { get; set; }
    }
}
