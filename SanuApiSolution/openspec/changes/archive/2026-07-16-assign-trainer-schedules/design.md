## Context

Hoy `trainer_x_classes (idtrainer, idclass)` asocia un trainer a una `class` completa. Una `class` tiene N `class_date` (horarios: día, hora, capacidad). Al asignar un trainer a una clase, `GetClassesWithStudentsAsync` trae **todos** los `class_date` de esa clase, sin distinguir cuáles cubre realmente el trainer. El pedido es que un trainer pueda cubrir un subconjunto de horarios de una clase (ej: lunes sí, viernes no).

La base de datos es PostgreSQL en Render (dev) y Neon (prod), sin migraciones EF Core — el esquema se gestiona a mano.

## Goals / Non-Goals

**Goals:**
- Permitir asignar un trainer a horarios (`class_date`) puntuales, no a la clase completa.
- Permitir asignar horarios en el mismo alta del trainer (`POST /trainer`), además de vía el endpoint de asignación existente.
- Que `GET /trainer/{id}/classes` devuelva, por clase, solo los horarios asignados al trainer.

**Non-Goals:**
- No se resuelve superposición de horarios entre trainers (un mismo horario puede tener 0, 1 o más trainers asignados; no se valida conflicto).
- No se migra automáticamente la data histórica de `trainer_x_classes` a `trainer_x_class_date` (se decide a mano qué horarios corresponden a cada asignación existente, dado que la tabla vieja no tiene esa granularidad).
- No se toca el modelo de `Classes`/`ClassDate` en sí, solo cómo se referencia desde trainers.

## Decisions

**1. Nueva tabla `trainer_x_class_date (idtrainer, idclassdate)` en vez de agregar una columna a `trainer_x_classes`.**
Alternativa considerada: agregar `idclassdate` nullable a `trainer_x_classes` (NULL = todos los horarios de la clase). Se descarta porque mantiene ambigüedad semántica (¿NULL significa "todos" o "sin definir"?) y complica las queries. Una tabla dedicada con PK compuesta (`idtrainer`, `idclassdate`) es más simple: cada fila es "este trainer cubre este horario", sin casos especiales.

**2. `TrainerClasses` (entidad) se reemplaza por `TrainerClassDate` (`idtrainer`, `idclassdate`).**
Mantiene el mismo patrón simple que la entidad actual (sin atributos Dapper.Contrib, solo `[Table]`), mapeado 1:1 a la nueva tabla.

**3. `TrainerAssignClassesRequestDto.ClassIds` se reemplaza por `ClassId` + `ClassDateIds`.**
Es un cambio de contrato (BREAKING) en vez de agregar un campo nuevo, porque mantener `ClassIds` en paralelo obligaría a soportar dos semánticas de asignación (clase completa vs. horario) indefinidamente. Dado que el propio pedido es "arreglar" el POST actual, se reemplaza directamente.
**Corrección post-implementación:** el horario (`class_date`) es un atributo de la clase, no del trainer — pasarlo como un array plano de IDs sin indicar a qué clase pertenecen dejaba esa relación implícita solo en la base (`class_date.idclass`), sin expresarla en el contrato de la API. El body pasa a requerir `ClassId` junto con `ClassDateIds` (`{ "classId": 5, "classDateIds": [12, 15] }`); cada llamada asigna al trainer los horarios de **una** clase. Para asignar varias clases distintas se llama al endpoint una vez por clase.

**4. `TrainerAddRequestDto` suma `ClassId` + `ClassDateIds` opcionales (nullable, default vacío).**
Si `ClassDateIds` viene con valores, `ClassId` es obligatorio (se valida y lanza `ArgumentException` si falta). `TrainerService.AddAsync` inserta el trainer y luego reutiliza la misma lógica de asignación de horarios (`AddClassDatesAsync`) en la misma llamada de servicio. Si un `ClassDateId` no existe o no pertenece a `ClassId`, se lanza `ArgumentException` que el controller ya traduce a 400 — pero el trainer ya fue insertado antes de validar. Para evitar un alta "a medias", la validación se hace **antes** de insertar el trainer.

**5. Validación de existencia y pertenencia de horarios a la clase.**
`ITrainerRepository` suma `Task<IEnumerable<int>> GetExistingClassDateIdsForClassAsync(int classId, IEnumerable<int> classDateIds)` para chequear cuáles de los IDs recibidos existen en `class_date` **y pertenecen a `classId`** (`WHERE idclass = @ClassId AND id = ANY(@Ids)`). Si falta alguno (porque no existe o pertenece a otra clase), se lanza `ArgumentException` con el detalle de los IDs inválidos, sin tocar la base.

**6. Query `GetClassesWithStudentsAsync` cambia el join.**
Pasa de `trainer_x_classes txc INNER JOIN classes cl ON cl.id = txc.idclass LEFT JOIN class_date cd ON cd.idclass = cl.id` a `trainer_x_class_date txcd INNER JOIN class_date cd ON cd.id = txcd.idclassdate INNER JOIN classes cl ON cl.id = cd.idclass`, filtrando por `txcd.idtrainer`. El resultado sigue agrupando por clase, pero `Dates` solo contiene los `class_date` asignados a ese trainer (ya no todos los de la clase).

## Risks / Trade-offs

- [Alta a medias si falla la asignación de horarios después de insertar el trainer] → Se valida que todos los `ClassDateIds` existan antes de insertar el trainer; si la asignación en sí falla después (error de conexión), el trainer queda creado sin horarios — comportamiento aceptable dado que hoy el patrón general del repo no usa transacciones explícitas.
- [Cambio de contrato rompe clientes existentes del frontend] → BREAKING, documentado en la proposal; requiere coordinar el despliegue del frontend que consume `ClassIds` para que pase a mandar `ClassDateIds`.
- [Migración manual de esquema en Render/Neon] → No hay rollback automático. Se documenta el DDL en tasks.md como paso manual explícito antes de desplegar el código.
- [Datos históricos en `trainer_x_classes` se pierden granularidad] → Fuera de alcance (Non-Goal); se documenta que la migración de datos existentes es una decisión manual, no automatizada.

## Migration Plan

1. Ejecutar DDL manual en Render (dev) y luego Neon (prod): crear `trainer_x_class_date`, dejar `trainer_x_classes` en la base sin usar (no se borra en este cambio, para permitir rollback rápido del código si hiciera falta).
2. Desplegar el código con el nuevo contrato.
3. Coordinar con el frontend el cambio de `ClassIds` a `ClassDateIds` en el body de `POST /trainer/{id}/classes`.
4. Una vez validado en producción, en un cambio posterior se puede eliminar `trainer_x_classes` y la entidad `TrainerClasses`.

## Open Questions

- ¿Qué pasa con las asignaciones ya existentes en `trainer_x_classes`? Queda pendiente decidir manualmente (fuera de este cambio) si se migran a horarios específicos o se dan de baja.
