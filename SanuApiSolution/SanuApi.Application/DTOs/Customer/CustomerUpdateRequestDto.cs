using Swashbuckle.AspNetCore.Annotations;

namespace SanuApi.Application.DTOs.Customer
{
    public class CustomerUpdateRequestDto
    {
        /// <summary>
        /// Corresponde al id de cliente.
        /// </summary>
        [SwaggerSchema("Corresponde alid del cliente.")]
        public required int IdCustomer { get; set; }
        /// <summary>
        /// Corresponde al nombre del cliente.
        /// </summary>
        [SwaggerSchema("Corresponde al nombre del cliente.")]
        public string CustomerName { get; set; }
        /// <summary>
        /// Corresponde al Apellido del cliente.
        /// </summary>
        [SwaggerSchema("Corresponde al Apellido del cliente.")]
        public string CustomerLastName { get; set; }
        /// <summary>
        /// Corresponde a la fecha de nacimiento del cliente. Respeta el formato YYYY-MM-DD.
        /// </summary>
        [SwaggerSchema("Corresponde a la fecha de nacimiento del cliente. Respeta el formato YYYY-MM-DD.")]
        public DateTime? DateBirth { get; set; }
        /// <summary>
        /// Corresponde al documento nacional de identidad del cliente.
        /// </summary>
        [SwaggerSchema("Corresponde al documento nacional de identidad del cliente.")]
        public int? Dni { get; set; }
        /// <summary>
        /// Corresponde al número de celular del cliente.
        /// </summary>
        /// 
        [SwaggerSchema("Corresponde al número de celular del cliente.")]
        public string Celphone { get; set; }
        /// <summary>
        /// Corresponde a la dirección del cliente.
        /// </summary>
        /// 
        [SwaggerSchema("Corresponde a la dirección del cliente.")]
        public string Address { get; set; }

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
        public List<int> idGoal { get; set; }

        /// <summary>
        /// Corresponde a los datos de salud del cliente.o.
        /// </summary>
        /// 
        [SwaggerSchema("Corresponde a los datos de salud del cliente.")]
        public  HealthCustomerDto? Health { get; set; }

        /// <summary>
        /// Corresponde a las membresias del cliente.
        /// </summary>
        /// 
        [SwaggerSchema("Corresponde a los ids de los ojetivos seleccionado.")]
        public List<CustomerMemberShipRequestDto> Memberships { get; set; }

        /// <summary>
        /// Corresponde a las clases del cliente.
        /// </summary>
        /// 
        [SwaggerSchema("Corresponde a los ids de las clases seleccionadas.")]
        public List<int> CustomerClasses { get; set; }
    }
}
