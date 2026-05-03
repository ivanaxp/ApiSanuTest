using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Logging;
using SanuApi.Domain.Entities;
using SanuApi.Domain.Interfaces;
using System.Data;

namespace SanuApi.Infrastructure.Repositories
{

    public class CustomerRepository : ICustomerRepository
    {
        private readonly IDbConnection _db;
        private readonly ILogger<CustomerRepository> _logger;
        public CustomerRepository(IDbConnection db, ILogger<CustomerRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync(bool? active)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var where = "";
            if (active.HasValue)
            {
                where = active.Value
                    ? " WHERE c.fechabaja IS NULL "
                    : " WHERE c.fechabaja IS NOT NULL ";
            }

            var sql = @"
        SELECT 
            c.id, c.customername, c.customerlastname, c.datebirth,
            c.dni, c.celphone, c.address, c.comentaries, c.ismale, 
            c.fechaalta, c.fechabaja,

            cg.goalid,
            g.id as goal_id, g.goalname,

            hc.id as health_id, hc.height, hc.weight, hc.alergics, hc.medicalcondicion,

            cm.id as cm_id, cm.membershipid, cm.startdate, cm.enddate,
            m.id as membership_id, m.name
        FROM customer c
        LEFT JOIN customer_x_goal cg ON cg.customerid = c.id
        LEFT JOIN goal g ON g.id = cg.goalid
        LEFT JOIN healthcustomer hc ON hc.customerid = c.id
        LEFT JOIN customer_x_membership cm ON cm.customerid = c.id
        LEFT JOIN membership m ON m.id = cm.membershipid
        " + where;

            var customersDictionary = new Dictionary<int, Customer>();

            var result = await _db.QueryAsync<Customer, CustomerGoal, Goal, HealthCustomer, CustomerMembership, Membership, Customer>(
                sql,
                (customer, customerGoal, goal, health, customerMembership, membership) =>
                {
                    // agrupaci�n por customer.id
                    if (!customersDictionary.TryGetValue(customer.id, out var currentCustomer))
                    {
                        currentCustomer = customer;
                        currentCustomer.customerGoals = new List<CustomerGoal>();
                        currentCustomer.customerMembership = new List<CustomerMembership>();
                        currentCustomer.healthCustomer = health ?? new HealthCustomer();

                        customersDictionary.Add(customer.id, currentCustomer);
                    }

                    // Goals
                    if (goal != null && goal.id != 0)
                    {
                        if (!currentCustomer.customerGoals.Any(g => g.goalid == goal.id))
                        {
                            customerGoal.Goal = goal;
                            currentCustomer.customerGoals.Add(customerGoal);
                        }
                    }

                    // Memberships
                    if (membership != null && membership.id != 0)
                    {
                        customerMembership.Membership = membership;
                        currentCustomer.customerMembership.Add(customerMembership);
                    }

                    return currentCustomer;
                },
                splitOn: "goalid,goal_id,health_id,cm_id,membership_id"
            );

            return customersDictionary.Values;
        }
        public async Task<Customer?> FindByIdAsync(int id)
        {
            _logger.LogInformation("[CustomerRepository.FindByIdAsync] Buscando cliente Id={Id}", id);
            // Bug fixes:
            // 1. hc.height AS heigth: "height" en DB no mapea a propiedad "heigth" (typo en entidad)
            // 2. splitOn usaba "name" que disparaba el split en m.name (Membership) antes de cl.name (Classes)
            // 3. Se elimina el JOIN de classes (FindByIdAsync no lo expone en el DTO de respuesta)
            var sql = "SELECT c.id, c.customername, c.customerlastname, c.datebirth," +
                 "c.dni, c.celphone, c.address, c.comentaries, c.ismale, c.fechaalta, c.fechabaja, " +
                 "cg.goalid, g.goalname, g.id, " +
                 "hc.height AS heigth, hc.weight, hc.alergics, hc.medicalcondicion," +
                 "cm.membershipid, cm.startdate, cm.enddate, m.id, m.name " +
                 "FROM customer c " +
                 "LEFT JOIN customer_x_goal cg ON cg.customerid = c.id " +
                 "LEFT JOIN customer_x_membership cm ON cm.customerid = c.id " +
                 "LEFT JOIN goal g ON cg.goalid = g.id " +
                 "LEFT JOIN healthcustomer hc ON hc.customerid = c.id " +
                 "LEFT JOIN membership m ON m.id = cm.membershipid " +
                 "WHERE c.id = @CustomerId " +
                 "ORDER BY g.id, cm.membershipid;";

            if (_db.State != ConnectionState.Open)
                _db.Open();

            _logger.LogInformation("[CustomerRepository.FindByIdAsync] Ejecutando query para Id={Id}", id);
            Customer? resultCustomer = null;

            try
            {
                await _db.QueryAsync<Customer, CustomerGoal, Goal, HealthCustomer, CustomerMembership, Membership, Customer>(
                    sql,
                    (customer, customerGoal, goal, health, customerMembership, membership) =>
                    {
                        if (resultCustomer == null)
                        {
                            resultCustomer = customer;
                            resultCustomer.customerGoals ??= new List<CustomerGoal>();
                            resultCustomer.customerMembership ??= new List<CustomerMembership>();
                            resultCustomer.healthCustomer = health ?? new HealthCustomer();
                        }

                        if (goal != null && goal.id != 0 && !resultCustomer.customerGoals.Any(g => g.goalid == goal.id))
                        {
                            customerGoal.Goal = goal;
                            resultCustomer.customerGoals.Add(customerGoal);
                        }

                        if (membership != null && membership.id != 0 &&
                            !resultCustomer.customerMembership.Any(m => m.membershipid == customerMembership.membershipid))
                        {
                            customerMembership.Membership = membership;
                            resultCustomer.customerMembership.Add(customerMembership);
                        }

                        return resultCustomer;
                    },
                    new { CustomerId = id },
                    splitOn: "goalid,goalname,heigth,membershipid,id"
                );
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[CustomerRepository.FindByIdAsync] ERROR en query. Id={Id}. Mensaje: {Message}. InnerException: {Inner}",
                    id, e.Message, e.InnerException?.Message);
                throw;
            }

            _logger.LogInformation("[CustomerRepository.FindByIdAsync] Resultado: {Found}", resultCustomer != null ? "Encontrado" : "No encontrado");

            return resultCustomer;
        }
        public async Task<int> AddAsync(Customer entity)
        {
            try
            {
                if (_db.State != ConnectionState.Open)
                    _db.Open();
                var id = await _db.InsertAsync(entity);
                return (int)id;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error al insertar el cliente", e);
            }

        }

        public async Task<int> AddClassesAsync(ClassCustomer entity)
        {
            try
            {
                if (_db.State != ConnectionState.Open)
                    _db.Open();

                var sql = @"INSERT INTO class_x_customer 
                    (classid, customerid)
                    VALUES(@IdClass, @IdCustomer)
                    RETURNING id;";    // <= ID autogenerado

                var newId = await _db.ExecuteScalarAsync<int>(sql, new
                {
                    IdCustomer = entity.customerid,
                    IdClass = entity.classid
                });

                return newId;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error al insertar la clase del cliente", e);
            }
        }
        public async Task<bool> DeleteAsync(int id)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = @"UPDATE customer 
                SET fechabaja = @FechaBaja
                WHERE id = @Id";

            var affectedRows = await _db.ExecuteAsync(sql, new
            {
                FechaBaja = DateTime.UtcNow,
                Id = id
            });

            return affectedRows > 0;
        }

        public async Task UpsertGoalAsync(CustomerGoal entity)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();
            var sql = @"INSERT INTO customer_x_goal (customerid, goalid)
                        SELECT @CustomerId, @GoalId
                        WHERE NOT EXISTS (
                            SELECT 1 FROM customer_x_goal
                            WHERE customerid = @CustomerId AND goalid = @GoalId
                        )";
            await _db.ExecuteAsync(sql, new { CustomerId = entity.customerid, GoalId = entity.goalid });
        }

        public async Task UpsertClassAsync(ClassCustomer entity)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();
            var sql = @"INSERT INTO class_x_customer (customerid, classid)
                        SELECT @CustomerId, @ClassId
                        WHERE NOT EXISTS (
                            SELECT 1 FROM class_x_customer
                            WHERE customerid = @CustomerId AND classid = @ClassId
                        )";
            await _db.ExecuteAsync(sql, new { CustomerId = entity.customerid, ClassId = entity.classid });
        }

        public async Task<bool> DeleteClassesAsync(int customerId)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();
            var sql = "DELETE FROM class_x_customer WHERE customerid = @CustomerId";
            var rows = await _db.ExecuteAsync(sql, new { CustomerId = customerId });
            return rows >= 0;
        }

        public async Task<bool> DeleteGoalsAsync(int customerId)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();
            var sql = "DELETE FROM customer_x_goal WHERE customerid = @CustomerId";
            var rows = await _db.ExecuteAsync(sql, new { CustomerId = customerId });
            return rows >= 0;
        }

        public async Task<IEnumerable<Classes>> GetClassesAsync(int id)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();
            var sql = @"SELECT cl.id, cl.name, cl.day, cl.hour, cl.capacity
                        FROM classes cl
                        INNER JOIN class_x_customer cxc ON cl.id = cxc.classid
                        WHERE cxc.customerid = @CustomerId";
            var classes = await _db.QueryAsync<Classes>(sql, new { CustomerId = id });
            return classes;
        }

        public async Task<bool> UpdateAsync(Customer entity)
        {
            _logger.LogInformation("[CustomerRepository.UpdateAsync] Iniciando UPDATE para Id={Id}", entity.id);
            try
            {
                if (_db.State != ConnectionState.Open)
                    _db.Open();

                _logger.LogInformation("[CustomerRepository.UpdateAsync] Conexion abierta. Estado={State}", _db.State);

                var sql = @"
                        UPDATE public.customer
                        SET
                            customername     = COALESCE(@CustomerName, customername),
                            customerlastname = COALESCE(@CustomerLastName, customerlastname),
                            datebirth        = COALESCE(@DateBirth, datebirth),
                            dni              = COALESCE(@Dni, dni),
                            celphone         = COALESCE(@CelPhone, celphone),
                            address          = COALESCE(@Address, address),
                            comentaries      = COALESCE(@Comentaries, comentaries),
                            ismale           = COALESCE(@IsMale, ismale),
                            fechabaja        = COALESCE(@FechaBaja, fechabaja),
                            fechaalta        = COALESCE(@FechaAlta, fechaalta)
                        WHERE id = @Id;
                        ";

                _logger.LogInformation("[CustomerRepository.UpdateAsync] Ejecutando SQL UPDATE. Params: Id={Id}, Name={Name}, LastName={LastName}, DateBirth={DateBirth}, Dni={Dni}",
                    entity.id, entity.customername, entity.customerlastname, entity.datebirth, entity.dni);

                var rowsAffected = await _db.ExecuteAsync(sql, new
                {
                    Id = entity.id,
                    CustomerName = entity.customername,
                    CustomerLastName = entity.customerlastname,
                    DateBirth = entity.datebirth,
                    Dni = entity.dni,
                    CelPhone = entity.celphone,
                    Address = entity.address,
                    Comentaries = entity.comentaries,
                    IsMale = entity.ismale,
                    FechaBaja = entity.fechabaja,
                    FechaAlta = entity.fechaalta
                });

                _logger.LogInformation("[CustomerRepository.UpdateAsync] Filas afectadas={RowsAffected}", rowsAffected);
                return rowsAffected > 0;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[CustomerRepository.UpdateAsync] ERROR al ejecutar UPDATE. Id={Id}. Mensaje: {Message}. InnerException: {Inner}",
                    entity.id, e.Message, e.InnerException?.Message);
                throw new InvalidOperationException("Error al actualizar el cliente", e);
            }
        }

        public async Task<int> AddAbsenceAsync(Absences entity)
        {
            try
            {
                if (_db.State != ConnectionState.Open)
                    _db.Open();
                var sql = @"INSERT INTO public.absences
                            (classid, customerid, dateabsence, status)
                            VALUES(@classid, @customerid, @dateabsence, @status)
                            RETURNING id;";
                var newId = await _db.ExecuteScalarAsync<int>(sql, new
                {
                    classid = entity.classid,
                    customerid = entity.customerid,
                    dateabsence = entity.dateabsence,
                    status = entity.status
                });
                return newId;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error al insertar la asistencia del cliente", e);
            }
        }

        public async Task<IEnumerable<(Absences Absence, Classes Class)>> GetAbsencesAsync(int customerId)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = @"SELECT a.id, a.customerid, a.classid, a.dateabsence, a.status,
                               cl.id, cl.name, cl.day, cl.hour, cl.capacity
                        FROM public.absences a
                        LEFT JOIN classes cl ON cl.id = a.classid
                        WHERE a.customerid = @CustomerId
                        ORDER BY a.dateabsence DESC";

            var result = await _db.QueryAsync<Absences, Classes, (Absences, Classes)>(
                sql,
                (absence, cls) => (absence, cls),
                new { CustomerId = customerId },
                splitOn: "id"
            );

            return result;
        }

    }
}