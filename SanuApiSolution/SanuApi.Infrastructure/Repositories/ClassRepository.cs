using Dapper;
using Dapper.Contrib.Extensions;
using SanuApi.Domain.Entities;
using SanuApi.Domain.Interfaces;
using System.Data;

namespace SanuApi.Infrastructure.Repositories
{
    public class ClassRepository: IClassRepository
    {
        private readonly IDbConnection _db;

        public ClassRepository(IDbConnection db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<int> AddAsync(Classes entity)
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
                throw new InvalidOperationException("Error al insertar la clase", e);
            }
        }

        public async Task<int> AddCustomerClassesAsync(ClassCustomer entity)
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
                throw new InvalidOperationException("Error al insertar la clase del cliente", e);
            }
        }

        public async Task<bool> DeleteAsync(Classes entity)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();
            var classes = await _db.DeleteAsync(entity);
            return classes;
        }

        public async Task<Classes?> FindByIdAsync(int id)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();
            var classes = await _db.GetAsync<Classes>(id);
            return classes;
        }

        public async Task<(Classes? Class, IEnumerable<Customer> Customers)> GetWithCustomersAsync(int classId)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = @"
                SELECT cl.id, cl.name, cl.day, cl.hour, cl.capacity,
                       c.id, c.customername, c.customerlastname, c.celphone
                FROM classes cl
                INNER JOIN class_x_customer cxc ON cl.id = cxc.classid
                INNER JOIN customer c ON c.id = cxc.customerid
                WHERE cl.id = @ClassId
                AND c.fechabaja IS NULL";

            Classes? resultClass = null;
            var customers = new List<Customer>();

            await _db.QueryAsync<Classes, Customer, Classes>(
                sql,
                (clase, customer) =>
                {
                    resultClass ??= clase;
                    customers.Add(customer);
                    return clase;
                },
                new { ClassId = classId },
                splitOn: "id"
            );

            return (resultClass, customers);
        }

        public async Task<IEnumerable<Classes>> GetAllAsync()
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = "SELECT id, name, day, hour, capacity FROM public.classes ";
            var classes = await _db.QueryAsync<Classes>(sql);
            return classes;
        }

        public async Task<(Classes? Class, IEnumerable<(Customer Customer, string? Status)> Students)> GetAttendanceByDateAsync(int classId, DateTime date)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var cls = await _db.QuerySingleOrDefaultAsync<Classes>(
                "SELECT id, name, capacity, day, hour FROM classes WHERE id = @ClassId",
                new { ClassId = classId });

            if (cls == null) return (null, Enumerable.Empty<(Customer, string?)>());

            var sql = @"
                SELECT c.id, c.customername, c.customerlastname, a.status
                FROM class_x_customer cxc
                INNER JOIN customer c ON c.id = cxc.customerid AND c.fechabaja IS NULL
                LEFT JOIN absences a ON a.customerid = c.id
                                    AND a.classid = @ClassId
                                    AND a.dateabsence::date = @Date::date
                WHERE cxc.classid = @ClassId
                ORDER BY c.customerlastname, c.customername";

            var rows = await _db.QueryAsync<AttendanceRow>(sql, new { ClassId = classId, Date = date.Date });

            var students = rows.Select(r => (
                new Customer { id = r.id, customername = r.customername, customerlastname = r.customerlastname },
                r.status
            ));

            return (cls, students);
        }

        private class AttendanceRow
        {
            public int id { get; set; }
            public string customername { get; set; }
            public string customerlastname { get; set; }
            public string? status { get; set; }
        }
    }
}
