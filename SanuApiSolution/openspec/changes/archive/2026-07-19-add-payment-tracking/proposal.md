## Why

Hoy la API no tiene forma de registrar pagos de clientes ni de saber si un cliente está al día, en deuda o con saldo a favor. Iva (facturación) necesita esta funcionalidad para llevar el historial de pagos por cliente, calcular saldos, y tener una vista mensual de cobranza. Es un módulo nuevo, sin dependencias de features existentes más allá de `Customer` y `Membership`.

## What Changes

- Se agregan dos tablas nuevas: `payment` (un registro por cada pago cargado) y `payment_detail` (snapshot de qué membresías cubrió ese pago, con nombre y precio al momento del pago — para no perder el historial si la membresía cambia después). Requiere DDL manual en Render (dev) y Neon (producción), ya que el esquema no se gestiona con EF Core migrations.
- Nuevo endpoint `GET /api/pagos/cliente/{id}`: historial de pagos de un cliente junto con su saldo acumulado.
- Nuevo endpoint `GET /api/pagos/cliente/{id}/saldo`: solo el saldo actual del cliente.
- Nuevo endpoint `POST /api/pagos`: registra un pago. El `monto_esperado` y el detalle de membresías cubiertas (`PaymentDetail`) se calculan del lado del servidor a partir de las membresías activas del cliente al momento del pago — no los manda el cliente.
- Nuevo endpoint `PUT /api/pagos/{id}`: corrige un pago existente (monto pagado, período, nota). No permite recalcular el detalle de membresías (ese snapshot queda fijo).
- Nuevo endpoint `DELETE /api/pagos/{id}`: elimina un pago (borrado físico, con su detalle).
- Nuevo endpoint `GET /api/pagos`: lista todos los pagos con filtros opcionales por período, estado (`deuda`/`favor`/`completo`), cliente y rango de fechas — para el módulo de facturación.
- Nuevo endpoint `GET /api/pagos/resumen-mes`: estadísticas de un mes (total esperado, total cobrado, total en deuda, total a favor, cantidad de pagos).
- Las rutas usan el prefijo literal `/api/pagos` (minúscula, plural) en vez de la convención `[Route("api/[controller]")]` que usa el resto de la API (que da rutas en singular con mayúscula, ej. `/api/Customer`) — se prioriza el contrato ya acordado con Iva por sobre la consistencia de casing con otros controllers.
- Las entidades, tablas y DTOs se nombran en inglés (`Payment`, `PaymentDetail`, tabla `payment`/`payment_detail`, propiedades `customerid`, `periodmonth`, etc.) siguiendo el mismo estilo que `Customer`/`Membership` (Dapper.Contrib, propiedades en minúscula pegada sin guiones bajos en las entidades de dominio, PascalCase en DTOs).

## Capabilities

### New Capabilities
- `payment-tracking`: registro de pagos de clientes, cálculo de saldo (a favor/deuda), historial por cliente, listado filtrable para facturación, y resumen mensual.

### Modified Capabilities
(ninguna: no se modifica el comportamiento de `trainer-schedule-assignment` ni de ninguna otra capability existente)

## Impact

- **Dominio**: nuevas entidades `Payment` (tabla `payment`) y `PaymentDetail` (tabla `payment_detail`).
- **Aplicación**: nuevos DTOs (`PaymentAddRequestDto`, `PaymentUpdateRequestDto`, `PaymentResponseDto`, `PaymentDetailResponseDto`, `CustomerPaymentHistoryResponseDto`, `CustomerBalanceResponseDto`, `MonthlySummaryResponseDto`), nueva interfaz `IPaymentService` y su implementación `PaymentService`.
- **Infraestructura**: nueva interfaz `IPaymentRepository` y su implementación `PaymentRepository`, con queries que calculan el saldo (`SUM(paidamount - expectedamount)`) y el monto esperado (suma de precios de membresías activas del cliente).
- **API**: nuevo `PaymentController` con ruta explícita `[Route("api/pagos")]` (nombre de clase en inglés, consistente con el resto de los controllers; la ruta se mantiene en español/plural por el contrato con Iva).
- **DI**: registrar `IPaymentRepository`/`PaymentRepository` e `IPaymentService`/`PaymentService` como scoped en `Program.cs`, siguiendo el patrón existente.
- **Base de datos**: nuevas tablas `payment` y `payment_detail` con FKs a `customer` y `membership`. Requiere DDL manual en Render (dev) y Neon (prod).
- **Tests**: nuevos `PaymentServiceTests` (en `SanuApi.Application.Tests`) y `PaymentControllerTests` (en `SanuApi.Api.Tests`), siguiendo el patrón NUnit + Moq existente.
