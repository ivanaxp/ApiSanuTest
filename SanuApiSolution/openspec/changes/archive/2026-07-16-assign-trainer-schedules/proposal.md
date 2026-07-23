## Why

Un trainer hoy se asigna a una `class` completa (`trainer_x_classes: idtrainer, idclass`), lo que implica automáticamente todos sus horarios (`class_date`). En la práctica un trainer puede cubrir solo algunos horarios de una clase (por ejemplo, la clase de lunes/miércoles pero no la de viernes), y el modelo actual no permite expresar eso. Además, para asignar horarios a un trainer nuevo hoy hacen falta dos llamadas (alta del trainer y luego alta de clases), cuando debería poder hacerse en una sola operación.

## What Changes

- **BREAKING**: La asignación de trainer a clases pasa de nivel "clase" a nivel "horario" (`class_date`). El endpoint `POST /api/employee/trainer/{trainerId}/classes` deja de recibir `ClassIds` y pasa a recibir `{ ClassId, ClassDateIds }`: el horario es un atributo de la clase, no del trainer, así que el request agrupa los horarios bajo la clase a la que pertenecen en vez de mandarlos como un array plano y desconectado.
- **BREAKING**: La tabla `trainer_x_classes (idtrainer, idclass)` se reemplaza por `trainer_x_class_date (idtrainer, idclassdate)`, con FK a `class_date.id`. Requiere migración manual en las bases de Render (dev) y Neon (producción), ya que el esquema no se gestiona con EF Core migrations.
- `TrainerAddRequestDto` (`POST /api/employee/trainer`) suma campos opcionales `ClassId` y `ClassDateIds`: si vienen, el alta del trainer asigna esos horarios de esa clase en la misma operación.
- `GET /api/employee/trainer/{trainerId}/classes` (listado de clases con horarios y alumnos) se ajusta para devolver, por clase, únicamente los horarios (`Dates`) efectivamente asignados al trainer, no todos los de la clase.
- Se valida que cada `ClassDateId` recibido exista en `class_date` **y pertenezca al `ClassId` indicado**; de lo contrario se responde `400`.

## Capabilities

### New Capabilities
- `trainer-schedule-assignment`: asignación de trainers a horarios (`class_date`) específicos de una clase, tanto en el alta del trainer como mediante el endpoint dedicado de asignación, y su reflejo en la consulta de clases del trainer.

### Modified Capabilities
(ninguna: no existen specs previos en `openspec/specs/`)

## Impact

- **Dominio**: `TrainerClasses` se reemplaza por una entidad `TrainerClassDate` (`idtrainer`, `idclassdate`) mapeada a la nueva tabla `trainer_x_class_date`.
- **Aplicación**: `TrainerAddRequestDto`, `TrainerAssignClassesRequestDto`, `ITrainerService`, `TrainerService`.
- **Infraestructura**: `ITrainerRepository`, `TrainerRepository` (insert de asignación y query de `GetClassesWithStudentsAsync`, que hoy hace `JOIN` por `idclass` y debe filtrar por `idclassdate`).
- **API**: `EmployeeController` (`POST /trainer`, `POST /trainer/{trainerId}/classes`, `GET /trainer/{trainerId}/classes`).
- **Base de datos**: nueva tabla `trainer_x_class_date`; baja o migración de datos de `trainer_x_classes` en Render (dev) y Neon (prod). Es un paso manual fuera del control de este repo.
- **Tests**: `TrainerServiceTests` y `EmployeeControllerTests` deben actualizarse para el nuevo contrato (horarios en vez de clases).
