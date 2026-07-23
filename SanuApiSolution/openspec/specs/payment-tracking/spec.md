## Purpose

Registro de pagos de clientes, cálculo de saldo (a favor/deuda), historial por cliente, listado filtrable para el módulo de facturación, y resumen mensual. El monto esperado y el detalle de qué membresías cubrió cada pago se calculan del lado del servidor a partir de las membresías activas del cliente al momento del pago, con snapshot de nombre y precio para preservar el historial si la membresía cambia después.

## Requirements

### Requirement: Registrar un pago
El sistema SHALL permitir registrar un pago de un cliente mediante `POST /api/pagos`, recibiendo `customerId`, `periodMonth` (1-12), `periodYear`, `paidAmount`, `note` opcional y `paymentDate` opcional (default: momento actual). El `expectedAmount` (monto esperado) y el detalle de qué membresías cubrió el pago SHALL calcularse del lado del servidor a partir de las membresías activas del cliente al momento del pago, no recibirse del cliente.

#### Scenario: Registrar un pago con membresías activas
- **WHEN** se envía `POST /api/pagos` con un `customerId` que tiene membresías activas en el momento del pago
- **THEN** el sistema crea el `Payment` con `expectedAmount` igual a la suma de los precios de esas membresías, crea un `PaymentDetail` por cada una (con snapshot de nombre y precio), y responde `201` con el pago creado

#### Scenario: Registrar un pago sin membresías activas
- **WHEN** se envía `POST /api/pagos` con un `customerId` que no tiene ninguna membresía activa en el momento del pago
- **THEN** el sistema crea el `Payment` con `expectedAmount` igual a `0` y no crea ningún `PaymentDetail`

#### Scenario: Datos inválidos
- **WHEN** se envía `POST /api/pagos` con `periodMonth` fuera del rango 1-12, o `customerId` que no corresponde a un cliente existente
- **THEN** el sistema responde `400` sin crear ningún registro

### Requirement: Corregir un pago existente
El sistema SHALL permitir corregir un pago mediante `PUT /api/pagos/{id}`, actualizando únicamente `periodMonth`, `periodYear`, `paidAmount` y/o `note` cuando vienen presentes en el request. El detalle de membresías (`PaymentDetail`) del pago NO SHALL ser recalculado ni modificable por esta operación.

#### Scenario: Corrección parcial exitosa
- **WHEN** se envía `PUT /api/pagos/{id}` con un pago existente y solo `paidAmount`
- **THEN** el sistema actualiza únicamente `paidAmount`, deja el resto de los campos y el detalle de membresías sin cambios, y responde `204`

#### Scenario: Pago inexistente
- **WHEN** se envía `PUT /api/pagos/{id}` con un `id` que no corresponde a ningún pago
- **THEN** el sistema responde `404`

### Requirement: Eliminar un pago
El sistema SHALL permitir eliminar físicamente un pago mediante `DELETE /api/pagos/{id}`, junto con todos sus `PaymentDetail` asociados.

#### Scenario: Eliminación exitosa
- **WHEN** se envía `DELETE /api/pagos/{id}` con un `id` de un pago existente
- **THEN** el sistema elimina el `Payment` y sus `PaymentDetail`, y responde `204`

#### Scenario: Pago inexistente
- **WHEN** se envía `DELETE /api/pagos/{id}` con un `id` que no corresponde a ningún pago
- **THEN** el sistema responde `404`

### Requirement: Calcular el saldo de un cliente
El sistema SHALL calcular el saldo de un cliente como la suma de `(paidAmount - expectedAmount)` de todos sus pagos. Un saldo positivo indica plata a favor; un saldo negativo indica deuda; cero indica que está al día.

#### Scenario: Cliente con saldo a favor
- **WHEN** la suma de `paidAmount - expectedAmount` de todos los pagos de un cliente es positiva
- **THEN** el saldo devuelto es ese valor positivo

#### Scenario: Cliente con deuda
- **WHEN** la suma de `paidAmount - expectedAmount` de todos los pagos de un cliente es negativa
- **THEN** el saldo devuelto es ese valor negativo

#### Scenario: Cliente sin pagos registrados
- **WHEN** un cliente no tiene ningún pago registrado
- **THEN** el saldo devuelto es `0`

### Requirement: Consultar historial de pagos de un cliente
`GET /api/pagos/cliente/{id}` SHALL devolver el historial completo de pagos de un cliente (incluyendo el detalle de membresías cubiertas por cada pago) junto con su saldo acumulado.

#### Scenario: Cliente con historial de pagos
- **WHEN** se consulta `GET /api/pagos/cliente/{id}` para un cliente con pagos registrados
- **THEN** la respuesta incluye la lista de pagos (con su detalle de membresías) ordenada y el saldo acumulado del cliente

#### Scenario: Cliente sin pagos
- **WHEN** se consulta `GET /api/pagos/cliente/{id}` para un cliente sin pagos registrados
- **THEN** la respuesta incluye una lista vacía y saldo `0`

### Requirement: Consultar solo el saldo actual de un cliente
`GET /api/pagos/cliente/{id}/saldo` SHALL devolver únicamente el saldo actual del cliente, sin el historial completo.

#### Scenario: Consulta de saldo
- **WHEN** se consulta `GET /api/pagos/cliente/{id}/saldo`
- **THEN** la respuesta incluye solo el valor del saldo del cliente

### Requirement: Listar todos los pagos con filtros
`GET /api/pagos` SHALL devolver todos los pagos del sistema, permitiendo filtrar opcionalmente por `periodo_mes`, `periodo_año`, `estado` (`deuda`, `favor` o `completo`, evaluado por registro comparando `paidAmount` contra `expectedAmount`), `cliente_id`, y rango de fechas (`desde`/`hasta` sobre `paymentDate`). Los filtros son combinables.

#### Scenario: Filtrar por período
- **WHEN** se consulta `GET /api/pagos?periodo_mes=7&periodo_año=2026`
- **THEN** la respuesta incluye únicamente los pagos de julio de 2026

#### Scenario: Filtrar por estado de deuda
- **WHEN** se consulta `GET /api/pagos?estado=deuda`
- **THEN** la respuesta incluye únicamente los pagos donde `paidAmount < expectedAmount`

#### Scenario: Filtrar por cliente y rango de fechas
- **WHEN** se consulta `GET /api/pagos?cliente_id=123&desde=2026-01-01&hasta=2026-12-31`
- **THEN** la respuesta incluye únicamente los pagos de ese cliente con `paymentDate` dentro del rango indicado

#### Scenario: Sin filtros
- **WHEN** se consulta `GET /api/pagos` sin ningún parámetro de filtro
- **THEN** la respuesta incluye todos los pagos del sistema

### Requirement: Resumen mensual de facturación
`GET /api/pagos/resumen-mes` SHALL devolver estadísticas agregadas de un mes: total esperado, total cobrado, total en deuda, total a favor, cantidad de pagos, cantidad de clientes en deuda y cantidad de clientes con saldo a favor. El mes/año SHALL tomarse de `periodo_mes`/`periodo_año` en la query string, u opcionalmente el mes/año actual si no se especifican.

#### Scenario: Resumen de un mes con pagos
- **WHEN** se consulta `GET /api/pagos/resumen-mes?periodo_mes=7&periodo_año=2026` y existen pagos en ese período
- **THEN** la respuesta incluye los totales agregados calculados sobre los pagos de ese período

#### Scenario: Resumen de un mes sin pagos
- **WHEN** se consulta `GET /api/pagos/resumen-mes` para un período sin pagos registrados
- **THEN** la respuesta incluye todos los totales en `0`
