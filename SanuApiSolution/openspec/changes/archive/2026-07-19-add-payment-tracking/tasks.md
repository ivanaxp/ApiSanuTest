## 1. Base de datos (manual, previo al despliegue)

- [ ] 1.1 Ejecutar en Render (dev):
  ```sql
  CREATE TABLE payment (
    id SERIAL PRIMARY KEY,
    customerid integer NOT NULL REFERENCES customer(id),
    periodmonth integer NOT NULL,
    periodyear integer NOT NULL,
    expectedamount numeric NOT NULL,
    paidamount numeric NOT NULL,
    paymentdate timestamp NOT NULL,
    note text NULL
  );

  CREATE TABLE payment_detail (
    id SERIAL PRIMARY KEY,
    paymentid integer NOT NULL REFERENCES payment(id) ON DELETE CASCADE,
    membershipid integer NOT NULL REFERENCES membership(id),
    membershipname text NOT NULL,
    membershipprice numeric NOT NULL
  );
  ```
- [ ] 1.2 Ejecutar el mismo DDL en Neon (producción)

## 2. Dominio

- [x] 2.1 Crear `SanuApi.Domain/Entities/Payment.cs` con `[Table("payment")]`: `id`, `customerid`, `periodmonth`, `periodyear`, `expectedamount`, `paidamount`, `paymentdate`, `note` (nullable), y `[Write(false)] List<PaymentDetail> Details`
- [x] 2.2 Crear `SanuApi.Domain/Entities/PaymentDetail.cs` con `[Table("payment_detail")]`: `id`, `paymentid`, `membershipid`, `membershipname`, `membershipprice`

## 3. Repositorio

- [x] 3.1 Crear `SanuApi.Domain/Interfaces/IPaymentRepository.cs` con:
  - `Task<int> AddAsync(Payment entity)`
  - `Task AddDetailsAsync(int paymentId, IEnumerable<PaymentDetail> details)`
  - `Task<Payment?> FindByIdAsync(int id)`
  - `Task<bool> UpdateAsync(Payment entity)`
  - `Task<bool> DeleteAsync(int id)`
  - `Task<IEnumerable<(Payment Payment, IEnumerable<PaymentDetail> Details)>> GetByCustomerAsync(int customerId)`
  - `Task<decimal> GetBalanceAsync(int customerId)`
  - `Task<IEnumerable<(int MembershipId, string Name, decimal Price)>> GetActiveMembershipsAsync(int customerId, DateTime asOfDate)` (usa `customer_x_membership` + `membership`, filtrando `startdate <= @AsOfDate AND (enddate IS NULL OR enddate > @AsOfDate)`)
  - `Task<IEnumerable<(Payment Payment, IEnumerable<PaymentDetail> Details)>> GetAllAsync(int? periodMonth, int? periodYear, string? estado, int? customerId, DateTime? desde, DateTime? hasta)`
  - `Task<MonthlySummaryRow> GetMonthlySummaryAsync(int periodMonth, int periodYear)` (o una tupla equivalente con los agregados)
- [x] 3.2 Implementar `SanuApi.Infrastructure/Repositories/PaymentRepository.cs` con las queries SQL correspondientes (Dapper crudo, siguiendo el patrón de `TrainerRepository`/`CustomerRepository`: abrir conexión si no está abierta, `try/catch` envolviendo `InvalidOperationException` en inserts)
- [x] 3.3 En `GetAllAsync`: construir el `WHERE` dinámicamente según qué filtros vienen presentes; calcular `estado` en SQL con `CASE WHEN paidamount < expectedamount THEN 'deuda' WHEN paidamount > expectedamount THEN 'favor' ELSE 'completo' END` y filtrar por ese valor cuando se pase `estado`

## 4. Aplicación (DTOs y servicio)

- [x] 4.1 Crear `SanuApi.Application/DTOs/Payment/PaymentAddRequestDto.cs`: `CustomerId` (required int), `PeriodMonth` (required int), `PeriodYear` (required int), `PaidAmount` (required decimal), `Note` (string?), `PaymentDate` (DateTime?, default `UtcNow` si no viene)
- [x] 4.2 Crear `SanuApi.Application/DTOs/Payment/PaymentUpdateRequestDto.cs`: `PeriodMonth` (int?), `PeriodYear` (int?), `PaidAmount` (decimal?), `Note` (string?) — todos opcionales, patrón parcial como `TrainerUpdateRequestDto`
- [x] 4.3 Crear `SanuApi.Application/DTOs/Payment/PaymentDetailResponseDto.cs`: `MembershipId`, `MembershipName`, `MembershipPrice`
- [x] 4.4 Crear `SanuApi.Application/DTOs/Payment/PaymentResponseDto.cs`: `Id`, `CustomerId`, `PeriodMonth`, `PeriodYear`, `ExpectedAmount`, `PaidAmount`, `PaymentDate`, `Note`, `Estado` (`"deuda"`/`"favor"`/`"completo"`, derivado), `Details: List<PaymentDetailResponseDto>`
- [x] 4.5 Crear `SanuApi.Application/DTOs/Payment/CustomerPaymentHistoryResponseDto.cs`: `CustomerId`, `Balance`, `Payments: List<PaymentResponseDto>`
- [x] 4.6 Crear `SanuApi.Application/DTOs/Payment/CustomerBalanceResponseDto.cs`: `CustomerId`, `Balance`
- [x] 4.7 Crear `SanuApi.Application/DTOs/Payment/MonthlySummaryResponseDto.cs`: `PeriodMonth`, `PeriodYear`, `TotalExpected`, `TotalPaid`, `TotalDebt`, `TotalCredit`, `PaymentCount`, `CustomersInDebtCount`, `CustomersWithCreditCount`
- [x] 4.8 Crear `SanuApi.Application/Interfaces/IPaymentService.cs`:
  - `Task<int> AddAsync(PaymentAddRequestDto dto)`
  - `Task<bool> UpdateAsync(int paymentId, PaymentUpdateRequestDto dto)`
  - `Task<bool> DeleteAsync(int paymentId)`
  - `Task<CustomerPaymentHistoryResponseDto> GetCustomerHistoryAsync(int customerId)`
  - `Task<CustomerBalanceResponseDto> GetCustomerBalanceAsync(int customerId)`
  - `Task<IEnumerable<PaymentResponseDto>> GetAllAsync(int? periodMonth, int? periodYear, string? estado, int? customerId, DateTime? desde, DateTime? hasta)`
  - `Task<MonthlySummaryResponseDto> GetMonthlySummaryAsync(int? periodMonth, int? periodYear)`
- [x] 4.9 Implementar `SanuApi.Application/Services/PaymentService.cs`:
  - `AddAsync`: valida `PeriodMonth` en rango 1-12 (`ArgumentException` si no); busca membresías activas del cliente a la fecha del pago vía `GetActiveMembershipsAsync`; suma precios → `expectedamount`; inserta `Payment`; inserta un `PaymentDetail` por cada membresía activa (snapshot de nombre/precio)
  - `UpdateAsync`: patrón parcial (solo pisa campos presentes en el dto), igual que `TrainerService.UpdateAsync`
  - `DeleteAsync`: `404`/`false` si no existe; si existe, elimina (el `ON DELETE CASCADE` de `payment_detail` se encarga del detalle)
  - `GetMonthlySummaryAsync`: si `periodMonth`/`periodYear` no vienen, usa el mes/año actual (`DateTime.UtcNow`)
  - El campo `Estado` de `PaymentResponseDto` se deriva comparando `PaidAmount` vs `ExpectedAmount` al mapear (o se toma directamente del valor ya calculado en SQL si el repositorio lo expone)

## 5. API

- [x] 5.1 Crear `SanuApi.Api/Controllers/PaymentController.cs` con `[ApiController]` y `[Route("api/pagos")]` explícito (no usar `[Route("api/[controller]")]`; nombre de clase en inglés para que el tag de Swagger sea consistente con el resto de la API — "Payment", no "Pago")
- [x] 5.2 `[HttpGet("cliente/{id}")]` → `GetCustomerHistory(int id)` → `_paymentService.GetCustomerHistoryAsync(id)`
- [x] 5.3 `[HttpGet("cliente/{id}/saldo")]` → `GetCustomerBalance(int id)` → `_paymentService.GetCustomerBalanceAsync(id)`
- [x] 5.4 `[HttpPost]` → `Create([FromBody] PaymentAddRequestDto dto)` → `_paymentService.AddAsync(dto)`, captura `ArgumentException` → `400`
- [x] 5.5 `[HttpPut("{id}")]` → `Update(int id, [FromBody] PaymentUpdateRequestDto dto)` → `_paymentService.UpdateAsync(id, dto)`, `404` si no existe
- [x] 5.6 `[HttpDelete("{id}")]` → `Delete(int id)` → `_paymentService.DeleteAsync(id)`, `404` si no existe
- [x] 5.7 `[HttpGet]` → `GetAll([FromQuery(Name = "periodo_mes")] int? periodoMes, [FromQuery(Name = "periodo_año")] int? periodoAnio, [FromQuery] string? estado, [FromQuery(Name = "cliente_id")] int? clienteId, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)` → `_paymentService.GetAllAsync(...)`
- [x] 5.8 `[HttpGet("resumen-mes")]` → `GetMonthlySummary([FromQuery(Name = "periodo_mes")] int? periodoMes, [FromQuery(Name = "periodo_año")] int? periodoAnio)` → `_paymentService.GetMonthlySummaryAsync(...)`

## 6. DI (Program.cs)

- [x] 6.1 Registrar `builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();` junto a los demás repositorios
- [x] 6.2 Registrar `builder.Services.AddScoped<IPaymentService, PaymentService>();` junto a los demás servicios

## 7. Tests

- [x] 7.1 Crear `SanuApi.Application.Tests/Services/PaymentServiceTests.cs`: alta con/sin membresías activas, validación de `PeriodMonth` fuera de rango, corrección parcial, eliminación (existente/inexistente), cálculo de saldo (a favor/deuda/cero), listado con filtros combinados, resumen mensual (con/sin pagos)
- [x] 7.2 Crear `SanuApi.Api.Tests/Controllers/PaymentControllerTests.cs`: casos felices de los 7 endpoints + `400`/`404` en los casos de error correspondientes
- [x] 7.3 Correr `dotnet test` y confirmar que todo pasa (169/169 tests OK)

## 8. Verificación

- [ ] 8.1 Probar manualmente contra la base de dev (Render) con Swagger: registrar un pago, corregirlo, consultar historial y saldo, listar con filtros, y ver el resumen del mes (pendiente: requiere que las tablas `payment`/`payment_detail` existan — ver tarea 1.1)
- [ ] 8.2 Confirmar con Iva que las rutas y nombres de query params (`periodo_mes`, `periodo_año`, `estado`, `cliente_id`, `desde`, `hasta`) coinciden con lo que espera el frontend
