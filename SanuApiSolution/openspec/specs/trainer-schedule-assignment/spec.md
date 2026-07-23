## Purpose

Asignación de trainers a horarios (`class_date`) específicos de una clase — tanto en el alta del trainer como mediante el endpoint dedicado de asignación — y su reflejo en la consulta de clases del trainer. El horario es un atributo de la clase, no del trainer: las asignaciones siempre se expresan agrupadas bajo la clase a la que pertenecen.

## Requirements

### Requirement: Asignar horarios de una clase a un trainer existente
El sistema SHALL permitir asignar a un trainer un conjunto de horarios (`class_date`) de una clase específica mediante `POST /api/employee/trainer/{trainerId}/classes`, recibiendo `ClassId` (la clase) y `ClassDateIds` (los horarios de esa clase). El trainer queda asociado únicamente a esos horarios, no a la clase completa. El horario es un atributo de la clase, no del trainer: por eso el request agrupa los `ClassDateIds` bajo el `ClassId` al que pertenecen, en vez de un array plano desconectado de su clase.

#### Scenario: Asignación exitosa de horarios válidos de la clase indicada
- **WHEN** se envía `POST /api/employee/trainer/{trainerId}/classes` con `{ "classId": X, "classDateIds": [...] }` donde todos los `classDateIds` existen en `class_date` y pertenecen a la clase `X`
- **THEN** el sistema crea un registro de asignación por cada horario y responde `201` con la cantidad de horarios asignados

#### Scenario: ClassDateIds vacío
- **WHEN** se envía `POST /api/employee/trainer/{trainerId}/classes` con `classDateIds` vacío o ausente
- **THEN** el sistema responde `400` indicando que debe especificarse al menos un horario

#### Scenario: ClassDateId inexistente o de otra clase
- **WHEN** se envía `POST /api/employee/trainer/{trainerId}/classes` con al menos un `classDateId` que no existe en `class_date`, o que existe pero pertenece a una clase distinta de `classId`
- **THEN** el sistema responde `400` sin crear ninguna asignación, indicando cuáles IDs son inválidos para esa clase

### Requirement: Asignar horarios de una clase al dar de alta un trainer
El sistema SHALL permitir que `POST /api/employee/trainer` reciba los campos opcionales `ClassId` y `ClassDateIds`. Cuando `ClassDateIds` está presente y no vacío, `ClassId` es obligatorio y el sistema SHALL crear el trainer y asignarle esos horarios de esa clase como parte de la misma operación.

#### Scenario: Alta de trainer sin horarios
- **WHEN** se envía `POST /api/employee/trainer` sin `ClassId` ni `ClassDateIds`
- **THEN** el sistema crea el trainer sin ninguna asignación de horarios, igual que el comportamiento actual

#### Scenario: Alta de trainer con horarios válidos de una clase
- **WHEN** se envía `POST /api/employee/trainer` con `classId` y `classDateIds` que existen en `class_date` y pertenecen a esa clase
- **THEN** el sistema crea el trainer y lo asigna a esos horarios, respondiendo `201` con el ID del trainer creado

#### Scenario: Alta de trainer con ClassDateIds pero sin ClassId
- **WHEN** se envía `POST /api/employee/trainer` con `classDateIds` no vacío pero sin `classId`
- **THEN** el sistema responde `400` y **no** crea el trainer

#### Scenario: Alta de trainer con un horario inválido o de otra clase
- **WHEN** se envía `POST /api/employee/trainer` con `classId` y al menos un `classDateId` que no existe en `class_date` o pertenece a otra clase
- **THEN** el sistema responde `400` indicando los IDs inválidos y **no** crea el trainer

### Requirement: Consulta de clases de un trainer refleja solo sus horarios asignados
`GET /api/employee/trainer/{trainerId}/classes` SHALL devolver, para cada clase asociada al trainer, únicamente los horarios (`Dates`) que le fueron asignados a ese trainer, no todos los horarios de la clase.

#### Scenario: Trainer con horarios parciales de una clase
- **WHEN** un trainer tiene asignados 2 de los 3 horarios de una clase y se consulta `GET /api/employee/trainer/{trainerId}/classes`
- **THEN** la respuesta incluye esa clase con exactamente los 2 horarios asignados en `Dates`, sin incluir el tercero
