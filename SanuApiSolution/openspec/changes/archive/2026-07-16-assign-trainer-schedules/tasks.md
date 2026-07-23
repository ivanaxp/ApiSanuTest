## 1. Base de datos (manual, previo al despliegue)

- [ ] 1.1 Ejecutar en Render (dev): `CREATE TABLE trainer_x_class_date (idtrainer integer NOT NULL REFERENCES trainer(id), idclassdate integer NOT NULL REFERENCES class_date(id), PRIMARY KEY (idtrainer, idclassdate));`
- [ ] 1.2 Ejecutar el mismo DDL en Neon (producción)
- [ ] 1.3 Dejar `trainer_x_classes` sin tocar (no se borra en este cambio; ver Open Questions en design.md)

## 2. Dominio

- [x] 2.1 Crear `SanuApi.Domain/Entities/TrainerClassDate.cs` con `[Table("trainer_x_class_date")]`, propiedades `idtrainer` e `idclassdate`
- [x] 2.2 Eliminar `SanuApi.Domain/Entities/TrainerClasses.cs` (reemplazado por `TrainerClassDate`)

## 3. Repositorio

- [x] 3.1 En `ITrainerRepository`: reemplazar `AddClassAsync(TrainerClasses entity)` por `AddClassDateAsync(TrainerClassDate entity)`
- [x] 3.2 En `ITrainerRepository`: agregar `Task<IEnumerable<int>> GetExistingClassDateIdsAsync(IEnumerable<int> classDateIds)`
- [x] 3.3 En `TrainerRepository`: implementar `AddClassDateAsync` con `INSERT INTO trainer_x_class_date (idtrainer, idclassdate) VALUES (@IdTrainer, @IdClassDate)`
- [x] 3.4 En `TrainerRepository`: implementar `GetExistingClassDateIdsAsync` con `SELECT id FROM class_date WHERE id = ANY(@Ids)`
- [x] 3.5 En `TrainerRepository.GetClassesWithStudentsAsync`: cambiar el join de `trainer_x_classes txc INNER JOIN classes cl ON cl.id = txc.idclass LEFT JOIN class_date cd ON cd.idclass = cl.id` a `trainer_x_class_date txcd INNER JOIN class_date cd ON cd.id = txcd.idclassdate INNER JOIN classes cl ON cl.id = cd.idclass`, filtrando por `txcd.idtrainer = @TrainerId`
- [x] 3.6 Ajustar el `studentSql` de `GetClassesWithStudentsAsync` para que use `trainer_x_class_date` (vía subquery de clases del trainer) en vez de `trainer_x_classes`

## 4. Aplicación (DTOs y servicio)

- [x] 4.1 Renombrar/ajustar `TrainerAssignClassesRequestDto.ClassIds` a `ClassDateIds` (**BREAKING**)
- [x] 4.2 Agregar `ClassDateIds` opcional (`List<int>?`) a `TrainerAddRequestDto`
- [x] 4.3 En `ITrainerService`: renombrar `AddClassesAsync(int trainerId, List<int> classIds)` a `AddClassDatesAsync(int trainerId, List<int> classDateIds)`
- [x] 4.4 En `TrainerService.AddClassDatesAsync`: validar que `classDateIds` no esté vacío (`ArgumentException`), luego validar contra `GetExistingClassDateIdsAsync` que todos existan (`ArgumentException` con el detalle de IDs inválidos) antes de insertar ninguna asignación
- [x] 4.5 En `TrainerService.AddAsync`: si `dto.ClassDateIds` viene con valores, validar existencia de todos los `ClassDateIds` **antes** de insertar el trainer (para no dejar un alta a medias); si son válidos, insertar el trainer y luego asignar los horarios reutilizando la lógica de `AddClassDatesAsync`
- [x] 4.6 Actualizar el mapeo en `TrainerService.GetClassesWithStudentsAsync` si cambia la forma de los datos devueltos por el repositorio (debería seguir funcionando igual, ya que `Dates` ahora viene pre-filtrado desde el repo)

## 5. API

- [x] 5.1 En `EmployeeController.AssignClasses`: actualizar para llamar a `_trainerService.AddClassDatesAsync(trainerId, dto.ClassDateIds)` y ajustar el summary/response de Swagger a "horarios" en vez de "clases"
- [x] 5.2 En `EmployeeController.CreateTrainer`: sin cambios de firma (el DTO ya trae el campo opcional), pero verificar que las excepciones de validación de horarios inválidos se traduzcan a `400`

## 6. Tests

- [x] 6.1 Actualizar `SanuApi.Application.Tests/Services/TrainerServiceTests.cs` para el nuevo contrato (`ClassDateIds`, validación de existencia, alta con horarios)
- [x] 6.2 Actualizar `SanuApi.Api.Tests/Controllers/EmployeeControllerTests.cs` para el nuevo contrato del endpoint de asignación
- [x] 6.3 Agregar test de `GetClassesWithStudentsAsync` (o su equivalente a nivel servicio) verificando que solo se devuelven los horarios asignados, no todos los de la clase

## 7. Verificación

- [x] 7.1 Correr `dotnet test` y confirmar que todo pasa (133/133 tests OK)
- [ ] 7.2 Probar manualmente contra la base de dev (Render) con Swagger: alta de trainer con horarios, asignación posterior, y `GET /trainer/{id}/classes` mostrando solo los horarios asignados (pendiente: requiere que la tabla `trainer_x_class_date` exista en la base — ver tarea 1.1)

## 8. Corrección: el horario pertenece a la clase, no al trainer

El request de asignación (tanto en el alta como en el endpoint dedicado) usaba un array plano `ClassDateIds` sin indicar a qué clase pertenecían. Se corrige para que el body agrupe los horarios bajo la clase (`{ "classId": X, "classDateIds": [...] }`), validando además que cada horario pertenezca a esa clase.

- [x] 8.1 En `TrainerAssignClassesRequestDto`: agregar `ClassId` (`required int`) junto a `ClassDateIds`
- [x] 8.2 En `TrainerAddRequestDto`: agregar `ClassId` (`int?`) junto a `ClassDateIds`, requerido cuando `ClassDateIds` no está vacío
- [x] 8.3 En `ITrainerRepository`/`TrainerRepository`: reemplazar `GetExistingClassDateIdsAsync(ids)` por `GetExistingClassDateIdsForClassAsync(classId, ids)` con `SELECT id FROM class_date WHERE idclass = @ClassId AND id = ANY(@Ids)`
- [x] 8.4 En `ITrainerService`/`TrainerService`: `AddClassDatesAsync` pasa a recibir `(int trainerId, int classId, List<int> classDateIds)`; `ValidateClassDateIdsAsync` valida contra la clase indicada
- [x] 8.5 En `TrainerService.AddAsync`: si `ClassDateIds` viene con valores pero `ClassId` es null, lanzar `ArgumentException` antes de crear el trainer
- [x] 8.6 En `EmployeeController.AssignClasses`: pasar `dto.ClassId` al servicio
- [x] 8.7 Actualizar `TrainerServiceTests` y `EmployeeControllerTests` para el nuevo contrato (`ClassId` + `ClassDateIds`, validación de pertenencia a la clase)
- [x] 8.8 Correr `dotnet test` y confirmar que todo pasa (135/135 tests OK)
