using Swashbuckle.AspNetCore.Annotations;

namespace SanuApi.Application.DTOs.Payment
{
    public class PaymentAddRequestDto
    {
        [SwaggerSchema("ID del cliente que realiza el pago")]
        public required int CustomerId { get; set; }

        [SwaggerSchema("Mes del período al que corresponde el pago (1-12)")]
        public required int PeriodMonth { get; set; }

        [SwaggerSchema("Año del período al que corresponde el pago")]
        public required int PeriodYear { get; set; }

        [SwaggerSchema("Monto efectivamente pagado")]
        public required decimal PaidAmount { get; set; }

        [SwaggerSchema("Observación opcional")]
        public string? Note { get; set; }

        [SwaggerSchema("Fecha del pago (opcional, por defecto la fecha/hora actual)")]
        public DateTime? PaymentDate { get; set; }
    }
}
