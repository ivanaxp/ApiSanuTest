using Swashbuckle.AspNetCore.Annotations;

namespace SanuApi.Application.DTOs.Payment
{
    public class PaymentUpdateRequestDto
    {
        [SwaggerSchema("Mes del período (opcional)")]
        public int? PeriodMonth { get; set; }

        [SwaggerSchema("Año del período (opcional)")]
        public int? PeriodYear { get; set; }

        [SwaggerSchema("Monto pagado (opcional)")]
        public decimal? PaidAmount { get; set; }

        [SwaggerSchema("Observación (opcional)")]
        public string? Note { get; set; }
    }
}
