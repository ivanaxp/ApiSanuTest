using Microsoft.AspNetCore.Mvc;
using SanuApi.Application.DTOs.Payment;
using SanuApi.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace SanuApi.Api.Controllers
{
    [ApiController]
    [Route("api/pagos")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("cliente/{id}")]
        [SwaggerOperation(Summary = "Historial de pagos de un cliente", Description = "Devuelve el historial de pagos de un cliente junto con su saldo acumulado")]
        [SwaggerResponse(200, "Historial de pagos", typeof(CustomerPaymentHistoryResponseDto))]
        public async Task<ActionResult<CustomerPaymentHistoryResponseDto>> GetCustomerHistory(int id)
        {
            var history = await _paymentService.GetCustomerHistoryAsync(id);
            return Ok(history);
        }

        [HttpGet("cliente/{id}/saldo")]
        [SwaggerOperation(Summary = "Saldo actual de un cliente")]
        [SwaggerResponse(200, "Saldo del cliente", typeof(CustomerBalanceResponseDto))]
        public async Task<ActionResult<CustomerBalanceResponseDto>> GetCustomerBalance(int id)
        {
            var balance = await _paymentService.GetCustomerBalanceAsync(id);
            return Ok(balance);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Registra un pago", Description = "El monto esperado y el detalle de membresías cubiertas se calculan del lado del servidor a partir de las membresías activas del cliente")]
        [SwaggerResponse(201, "Pago registrado correctamente", typeof(int))]
        [SwaggerResponse(400, "Datos invalidos")]
        public async Task<ActionResult<int>> Create([FromBody] PaymentAddRequestDto dto)
        {
            try
            {
                var id = await _paymentService.AddAsync(dto);
                return StatusCode(201, id);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Corrige un pago", Description = "Actualiza monto pagado, período y/o nota. No permite recalcular el detalle de membresías")]
        [SwaggerResponse(204, "Pago actualizado correctamente")]
        [SwaggerResponse(400, "Datos invalidos")]
        [SwaggerResponse(404, "No se encontro el pago")]
        public async Task<ActionResult> Update(int id, [FromBody] PaymentUpdateRequestDto dto)
        {
            try
            {
                var updated = await _paymentService.UpdateAsync(id, dto);
                if (!updated) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Elimina un pago", Description = "Borrado fisico del pago y su detalle de membresías")]
        [SwaggerResponse(204, "Pago eliminado correctamente")]
        [SwaggerResponse(404, "No se encontro el pago")]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _paymentService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Lista todos los pagos", Description = "Modulo de facturacion: permite filtrar por periodo, estado (deuda/favor/completo), cliente y rango de fechas")]
        [SwaggerResponse(200, "Lista de pagos", typeof(IEnumerable<PaymentResponseDto>))]
        [SwaggerResponse(400, "Filtro invalido")]
        public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetAll(
            [FromQuery(Name = "periodo_mes")] int? periodoMes,
            [FromQuery(Name = "periodo_año")] int? periodoAnio,
            [FromQuery] string? estado,
            [FromQuery(Name = "cliente_id")] int? clienteId,
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta)
        {
            try
            {
                var payments = await _paymentService.GetAllAsync(periodoMes, periodoAnio, estado, clienteId, desde, hasta);
                return Ok(payments);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("resumen-mes")]
        [SwaggerOperation(Summary = "Resumen mensual de facturacion", Description = "Totales del mes: esperado, cobrado, en deuda, a favor, y cantidades de pagos/clientes")]
        [SwaggerResponse(200, "Resumen del mes", typeof(MonthlySummaryResponseDto))]
        public async Task<ActionResult<MonthlySummaryResponseDto>> GetMonthlySummary(
            [FromQuery(Name = "periodo_mes")] int? periodoMes,
            [FromQuery(Name = "periodo_año")] int? periodoAnio)
        {
            var summary = await _paymentService.GetMonthlySummaryAsync(periodoMes, periodoAnio);
            return Ok(summary);
        }
    }
}
