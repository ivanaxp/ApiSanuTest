## Context

No existe hoy ningún concepto de "pago" en la API. Un cliente (`Customer`) puede tener varias membresías activas (`CustomerMembership`, tabla `customer_x_membership`, con `startdate`/`enddate` nullable = activa). El pago es mensual: el dueño carga cuánto pagó un cliente en un período (mes/año), y el sistema necesita saber cuánto *debería* haber pagado ese mes (suma de precios de sus membresías activas en ese momento) para calcular si quedó en deuda o con saldo a favor.

La base de datos es PostgreSQL en Render (dev) y Neon (prod), sin migraciones EF Core — el esquema se gestiona a mano. El resto de la API usa Clean Architecture (Api → Application → Domain ← Infrastructure), Dapper/Dapper.Contrib con SQL crudo, entidades con propiedades en minúscula pegada (sin guiones bajos) que reflejan las columnas de Postgres, y DTOs en PascalCase serializados a camelCase por el `JsonNamingPolicy.CamelCase` por defecto de ASP.NET Core (`Program.cs` solo agrega `JsonStringEnumConverter`, no cambia la política de nombres).

## Goals / Non-Goals

**Goals:**
- Registrar pagos por cliente y período (mes/año), con snapshot de qué membresías cubrió cada pago (para no perder el historial si la membresía cambia después).
- Calcular el saldo de un cliente como `SUM(paidamount - expectedamount)`: positivo = a favor, negativo = deuda.
- Exponer los 7 endpoints pedidos bajo el prefijo literal `/api/pagos` (decisión ya tomada: se prioriza el contrato con Iva por sobre el casing `[controller]` del resto de la API).
- Listado filtrable para el módulo de facturación (período, estado, cliente, rango de fechas) y un resumen mensual agregado.

**Non-Goals:**
- No se genera automáticamente el pago esperado ni se manda ningún recordatorio/notificación de deuda — solo se calcula y se expone.
- No se valida que `paidamount` cubra exactamente el `expectedamount`; el pago parcial o de más es un caso normal (por eso existe el concepto de saldo).
- No se permite recalcular ni editar el detalle de membresías (`PaymentDetail`) de un pago ya creado vía `PUT` — solo se corrigen los campos del pago en sí (monto, período, nota). Si el detalle está mal, se borra y se vuelve a crear el pago.
- No se implementa paginación en `GET /api/pagos` (fuera de alcance de esta historia; se puede agregar después si el volumen lo justifica).

## Decisions

**1. Rutas literales `/api/pagos` en vez de la convención `[Route("api/[controller]")]`.**
El resto de la API (Customer, Membership, Class, Employee) usa `[Route("api/[controller]")]`, que da rutas en singular con mayúscula inicial (`/api/Customer`). Iva ya tiene el contrato `/api/pagos/...` (minúscula, plural) definido. Se prioriza no romper ese contrato: el controller declara `[Route("api/pagos")]` explícito en vez de dejar que `[controller]` resuelva la ruta.
**Corrección post-implementación:** la clase se llamó inicialmente `PagoController` (español), pero Swagger agrupa los endpoints por nombre de clase (el "tag"), no por la ruta — eso hacía que el grupo apareciera como "Pago" mientras el resto de la API se agrupa en inglés ("Customer", "Membership", "Employee", "Class", "Goal"). Se renombra la clase a `PaymentController` (inglés, consistente con el resto), sin tocar la ruta `[Route("api/pagos")]`, que sigue siendo la literal acordada con Iva.

**2. Entidades y tablas en inglés, seleccionado sobre nombrar todo en español como en la historia original.**
El resto del dominio (`Customer`, `Membership`, `Trainer`, `Classes`, `Goal`) está en inglés; mantener esa consistencia interna. Traducción: `Pago` → `Payment` (tabla `payment`), `PagoDetalle` → `PaymentDetail` (tabla `payment_detail`). Los campos del story en español se adaptan al estilo de propiedades ya usado (minúscula pegada, sin guiones bajos, reflejando columnas Postgres):

| Story (ES) | Campo en `Payment` |
|---|---|
| `cliente_id` | `customerid` |
| `periodo_mes` | `periodmonth` |
| `periodo_año` | `periodyear` |
| `monto_esperado` | `expectedamount` |
| `monto_pagado` | `paidamount` |
| `fecha_pago` | `paymentdate` |
| `nota` | `note` |

| Story (ES) | Campo en `PaymentDetail` |
|---|---|
| `pago_id` | `paymentid` |
| `membresia_id` | `membershipid` |
| `nombre_membresia` | `membershipname` |
| `precio_membresia` | `membershipprice` |

**3. `expectedamount` y `PaymentDetail` se calculan del lado del servidor, no los manda el cliente.**
`PaymentAddRequestDto` solo recibe `CustomerId`, `PeriodMonth`, `PeriodYear`, `PaidAmount`, `Note` opcional y `PaymentDate` opcional (default `UtcNow`). Al crear el pago, `PaymentService.AddAsync`:
1. Busca las membresías activas del cliente (`customer_x_membership` donde `enddate IS NULL` o `enddate > paymentdate`, y `startdate <= paymentdate`).
2. Suma sus precios → `expectedamount`.
3. Inserta el `Payment`.
4. Por cada membresía activa, inserta un `PaymentDetail` con el snapshot de `membershipname`/`membershipprice` en ese momento.

Si el cliente no tiene ninguna membresía activa en ese momento, `expectedamount = 0` y no se crea ningún `PaymentDetail` (no es un error: puede pasar si se le perdonó el mes o la membresía se dio de baja antes del pago).

**4. `PUT /api/pagos/{id}` no permite recalcular membresías.**
Sigue el patrón parcial de `TrainerUpdateRequestDto`/`TrainerService.UpdateAsync` (campos opcionales, solo se actualiza lo que viene). `PaymentUpdateRequestDto` expone `PeriodMonth?`, `PeriodYear?`, `PaidAmount?`, `Note?`. No expone `ClassId`/membresías: el detalle queda fijo desde la creación. Si `PaidAmount` cambia, el saldo se recalcula automáticamente porque el saldo siempre se computa on-the-fly (`SUM(paidamount - expectedamount)`), no se guarda como columna.

**5. `DELETE /api/pagos/{id}` es borrado físico.**
A diferencia de `Customer`/`Trainer` (baja lógica con `fechabaja`/`endDate`), un pago no tiene noción de "activo/inactivo": es un registro puntual que se puede haber cargado mal. Se borra físicamente el `Payment` y en cascada su(s) `PaymentDetail`. Si en el futuro se necesita auditoría de borrados, se puede agregar después (Non-Goal de esta historia).

**6. Clasificación de `estado` (`deuda`/`favor`/`completo`) es por registro de pago, no acumulada por cliente.**
`GET /api/pagos?estado=deuda` filtra pagos individuales donde `paidamount < expectedamount` (`deuda`), `paidamount > expectedamount` (`favor`), o `paidamount = expectedamount` (`completo`). Es un filtro a nivel de fila, calculado en SQL (`CASE WHEN paidamount < expectedamount THEN 'deuda' WHEN paidamount > expectedamount THEN 'favor' ELSE 'completo' END`), no en la aplicación, para que funcione junto con los demás filtros sin traer todo a memoria.

**7. Filtros de `GET /api/pagos` como parámetros escalares individuales, no un DTO wrapper.**
Sigue el único patrón existente de `[FromQuery]` (`CustomerController.GetAll([FromQuery] bool? active)`, `ClassController.GetAttendanceByDate(int id, [FromQuery] DateTime date)`), que usa parámetros escalares directos, no un objeto de filtro. Los nombres de query string se mantienen literales al story (`periodo_mes`, `periodo_año`, `estado`, `cliente_id`, `desde`, `hasta`) vía `[FromQuery(Name = "...")]`, mapeados a parámetros C# con nombres válidos (`periodoMes`, `periodoAnio`, `estado`, `clienteId`, `desde`, `hasta`) — igual que se decidió para las rutas, se prioriza el contrato con Iva.

**8. `GET /api/pagos/resumen-mes` requiere período por query string.**
No tiene sentido un "resumen del mes" sin especificar cuál mes. Toma `periodo_mes`/`periodo_año` opcionales (default: mes/año actual del servidor si no se especifican) y devuelve: `totalEsperado`, `totalCobrado`, `totalDeuda` (suma de `expectedamount - paidamount` de los pagos en deuda), `totalFavor` (suma de `paidamount - expectedamount` de los pagos a favor), `cantidadPagos`, `cantidadClientesEnDeuda`, `cantidadClientesConFavor`.

**9. Se registra `IPaymentRepository`/`PaymentRepository` e `IPaymentService`/`PaymentService` en `Program.cs`**, siguiendo exactamente el mismo patrón scoped que el resto (`AddScoped<ITrainerRepository, TrainerRepository>()`, etc.).

## Risks / Trade-offs

- [Pago sin membresías activas da `expectedamount = 0`] → Es un caso válido (Decision 3), documentado como comportamiento esperado, no como error.
- [Borrado físico pierde auditoría] → Aceptado como Non-Goal; se puede revisar si el negocio lo requiere más adelante.
- [Migración manual de esquema en Render/Neon] → Sin rollback automático; DDL documentado en `tasks.md` como paso manual explícito, igual que en cambios anteriores de este repo.
- [Ruta `/api/pagos` inconsistente con el resto de controllers] → Decisión consciente (Decision 1) para no romper el contrato ya acordado con Iva; queda documentado para que futuros desarrolladores no lo "corrijan" por error.
- [Cálculo de saldo on-the-fly en cada consulta, sin columna materializada] → Simplicidad sobre performance; el volumen esperado (pagos por cliente por mes) es bajo, no justifica mantener una columna de saldo desnormalizada con el riesgo de que se desincronice.

## Migration Plan

1. Ejecutar DDL manual en Render (dev) y luego Neon (prod): crear tablas `payment` y `payment_detail` (ver `tasks.md` para el DDL exacto).
2. Desplegar el código con los nuevos endpoints (no reemplaza ni modifica endpoints existentes, es aditivo — no hay breaking changes).
3. Confirmar con Iva que el contrato de rutas/query params coincide con lo que espera el frontend.

## Open Questions

- ¿`GET /api/pagos/resumen-mes` necesita filtrar también por algún otro criterio (ej. solo clientes activos)? Se asume que no por ahora; se puede ajustar cuando Iva empiece a integrar el módulo de facturación.
