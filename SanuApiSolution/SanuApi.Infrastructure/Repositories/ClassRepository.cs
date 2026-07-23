using Dapper;
using Dapper.Contrib.Extensions;
using SanuApi.Domain.Entities;
using SanuApi.Domain.Interfaces;
using System.Data;

namespace SanuApi.Infrastructure.Repositories
{
    public class ClassRepository : IClassRepository
    {
        private readonly IDbConnection _db;

        public ClassRepository(IDbConnection db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        private void EnsureOpen()
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();
        }

        public async Task<int> AddAsync(Classes entity)
        {
            EnsureOpen();
            try
            {
                var id = await _db.InsertAsync(entity);
                return (int)id;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error al insertar la clase", e);
            }
        }

        public async Task AddDatesAsync(int classId, IEnumerable<ClassDate> dates)
        {
            EnsureOpen();
            const string sql = "INSERT INTO class_date (idclass, day, hour, capacity) VALUES (@idclass, @day::day_of_week, @hour, @capacity)";
            await _db.ExecuteAsync(sql, dates);
        }

        public async Task ReplaceDatesAsync(int classId, IEnumerable<ClassDate> dates)
        {
            EnsureOpen();
            var newDates = dates.ToList();

            var existing = (await _db.QueryAsync<ClassDate>(
                "SELECT id, idclass, day, hour, capacity FROM class_date WHERE idclass = @ClassId",
                new { ClassId = classId })).ToList();

            foreach (var ex in existing)
            {
                var match = newDates.FirstOrDefault(d => d.day == ex.day && d.hour == ex.hour);
                if (match != null)
                {
                    if (match.capacity != ex.capacity)
                    {
                        await _db.ExecuteAsync(
                            "UPDATE class_date SET capacity = @Capacity WHERE id = @Id",
                            new { Capacity = match.capacity, Id = ex.id });
                    }
                    newDates.Remove(match);
                }
                else
                {
                    var inUse = await _db.ExecuteScalarAsync<int>(
                        "SELECT COUNT(1) FROM class_x_customer WHERE idclassdate = @Id",
                        new { Id = ex.id });
                    if (inUse == 0)
                    {
                        await _db.ExecuteAsync("DELETE FROM class_date WHERE id = @Id", new { Id = ex.id });
                    }
                }
            }

            if (newDates.Count > 0)
            {
                const string sql = "INSERT INTO class_date (idclass, day, hour, capacity) VALUES (@idclass, @day::day_of_week, @hour, @capacity)";
                await _db.ExecuteAsync(sql, newDates);
            }
        }

        public async Task<bool> UpdateAsync(Classes entity)
        {
            EnsureOpen();
            return await _db.UpdateAsync(entity);
        }

        public async Task AddMembershipsAsync(int classId, IEnumerable<int> membershipIds)
        {
            EnsureOpen();
            var rows = membershipIds.Select(membershipId => new { ClassId = classId, MembershipId = membershipId }).ToList();
            if (rows.Count > 0)
            {
                const string sql = "INSERT INTO class_x_membership (classid, membershipid) VALUES (@ClassId, @MembershipId)";
                await _db.ExecuteAsync(sql, rows);
            }
        }

        public async Task ReplaceMembershipsAsync(int classId, IEnumerable<int> membershipIds)
        {
            EnsureOpen();
            await _db.ExecuteAsync("DELETE FROM class_x_membership WHERE classid = @ClassId", new { ClassId = classId });
            await AddMembershipsAsync(classId, membershipIds);
        }

        private async Task<Dictionary<int, List<Membership>>> LoadMembershipsForClassesAsync(IEnumerable<int> classIds)
        {
            var ids = classIds.Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<int, List<Membership>>();

            const string sql = @"
                SELECT cxm.classid, m.id, m.name, m.price, m.frecuency
                FROM class_x_membership cxm
                INNER JOIN membership m ON m.id = cxm.membershipid
                WHERE cxm.classid = ANY(@ClassIds)";

            var rows = await _db.QueryAsync<ClassMembershipRow>(sql, new { ClassIds = ids });
            return rows
                .GroupBy(r => r.classid)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => new Membership { id = r.id, name = r.name, price = r.price, frecuency = r.frecuency }).ToList()
                );
        }

        private class ClassMembershipRow
        {
            public int classid { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public decimal price { get; set; }
            public int frecuency { get; set; }
        }

        public async Task<int> AddCustomerClassesAsync(ClassCustomer entity)
        {
            EnsureOpen();
            try
            {
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
            EnsureOpen();
            return await _db.DeleteAsync(entity);
        }

        public async Task<Classes?> FindByIdAsync(int id)
        {
            EnsureOpen();
            const string sql = @"
                SELECT c.id, c.name,
                       cd.idclass, cd.id, cd.day, cd.hour, cd.capacity
                FROM classes c
                LEFT JOIN class_date cd ON cd.idclass = c.id
                WHERE c.id = @Id";

            Classes? result = null;
            await _db.QueryAsync<Classes, ClassDate, Classes>(
                sql,
                (cls, date) =>
                {
                    result ??= cls;
                    if (date != null && date.idclass != 0)
                        result.Dates.Add(date);
                    return cls;
                },
                new { Id = id },
                splitOn: "idclass"
            );

            if (result != null)
            {
                var membershipMap = await LoadMembershipsForClassesAsync(new[] { id });
                result.Memberships = membershipMap.TryGetValue(id, out var mems) ? mems : new();
            }
            return result;
        }

        public async Task<IEnumerable<Classes>> GetAllAsync()
        {
            EnsureOpen();
            const string sql = @"
                SELECT c.id, c.name,
                       cd.idclass, cd.id, cd.day, cd.hour, cd.capacity
                FROM classes c
                LEFT JOIN class_date cd ON cd.idclass = c.id
                ORDER BY c.id";

            var classDict = new Dictionary<int, Classes>();
            await _db.QueryAsync<Classes, ClassDate, Classes>(
                sql,
                (cls, date) =>
                {
                    if (!classDict.TryGetValue(cls.id, out var existing))
                    {
                        existing = cls;
                        classDict[cls.id] = existing;
                    }
                    if (date != null && date.idclass != 0)
                        existing.Dates.Add(date);
                    return existing;
                },
                splitOn: "idclass"
            );

            var membershipMap = await LoadMembershipsForClassesAsync(classDict.Keys);
            foreach (var cls in classDict.Values)
                cls.Memberships = membershipMap.TryGetValue(cls.id, out var mems) ? mems : new();

            return classDict.Values;
        }

        public async Task<IEnumerable<Classes>> GetByMembershipIdAsync(int membershipId)
        {
            EnsureOpen();
            const string sql = @"
                SELECT c.id, c.name,
                       cd.idclass, cd.id, cd.day, cd.hour, cd.capacity
                FROM classes c
                INNER JOIN class_x_membership cxm ON cxm.classid = c.id
                LEFT JOIN class_date cd ON cd.idclass = c.id
                WHERE cxm.membershipid = @MembershipId
                ORDER BY c.id";

            var classDict = new Dictionary<int, Classes>();
            await _db.QueryAsync<Classes, ClassDate, Classes>(
                sql,
                (cls, date) =>
                {
                    if (!classDict.TryGetValue(cls.id, out var existing))
                    {
                        existing = cls;
                        classDict[cls.id] = existing;
                    }
                    if (date != null && date.idclass != 0)
                        existing.Dates.Add(date);
                    return existing;
                },
                new { MembershipId = membershipId },
                splitOn: "idclass"
            );

            var membershipMap = await LoadMembershipsForClassesAsync(classDict.Keys);
            foreach (var cls in classDict.Values)
                cls.Memberships = membershipMap.TryGetValue(cls.id, out var mems) ? mems : new();

            return classDict.Values;
        }

        public async Task<(Classes? Class, IEnumerable<(Customer Customer, int? ClassDateId, string? Day, string? Hour)> Customers)> GetWithCustomersAsync(int classId)
        {
            EnsureOpen();
            var cls = await FindByIdAsync(classId);
            if (cls == null) return (null, Enumerable.Empty<(Customer, int?, string?, string?)>());

            const string sql = @"
                SELECT c.id, c.customername, c.customerlastname, c.celphone,
                       cd.id AS classdateid, cd.day, cd.hour
                FROM class_x_customer cxc
                INNER JOIN customer c ON c.id = cxc.customerid
                LEFT JOIN class_date cd ON cd.id = cxc.idclassdate
                WHERE cxc.classid = @ClassId
                AND c.fechabaja IS NULL";

            var rows = await _db.QueryAsync<CustomerWithDateRow>(sql, new { ClassId = classId });
            var customers = rows.Select(r => (
                new Customer { id = r.id, customername = r.customername, customerlastname = r.customerlastname, celphone = r.celphone },
                (int?)r.classdateid,
                (string?)r.day,
                (string?)r.hour
            ));
            return (cls, customers);
        }

        private class CustomerWithDateRow
        {
            public int id { get; set; }
            public string customername { get; set; }
            public string customerlastname { get; set; }
            public string celphone { get; set; }
            public int? classdateid { get; set; }
            public string? day { get; set; }
            public string? hour { get; set; }
        }

        public async Task<(Classes? Class, IEnumerable<(Customer Customer, string? Status)> Students)> GetAttendanceByDateAsync(int classId, DateTime date)
        {
            EnsureOpen();
            var cls = await FindByIdAsync(classId);
            if (cls == null) return (null, Enumerable.Empty<(Customer, string?)>());

            const string sql = @"
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

        public async Task<IEnumerable<(int AttendanceId, DateTime Date, string Status, Customer Customer)>> GetAttendanceRecordsAsync(int classId)
        {
            EnsureOpen();
            const string sql = @"
                SELECT a.id AS attendanceid, a.dateabsence, a.status,
                       c.id, c.customername, c.customerlastname, c.celphone
                FROM absences a
                INNER JOIN customer c ON c.id = a.customerid AND c.fechabaja IS NULL
                WHERE a.classid = @ClassId
                ORDER BY a.dateabsence DESC, c.customerlastname, c.customername";

            var rows = await _db.QueryAsync<AttendanceRecordRow>(sql, new { ClassId = classId });
            return rows.Select(r => (
                r.attendanceid,
                r.dateabsence,
                r.status,
                new Customer { id = r.id, customername = r.customername, customerlastname = r.customerlastname, celphone = r.celphone }
            ));
        }

        private class AttendanceRow
        {
            public int id { get; set; }
            public string customername { get; set; }
            public string customerlastname { get; set; }
            public string? status { get; set; }
        }

        private class AttendanceRecordRow
        {
            public int attendanceid { get; set; }
            public DateTime dateabsence { get; set; }
            public string status { get; set; }
            public int id { get; set; }
            public string customername { get; set; }
            public string customerlastname { get; set; }
            public string celphone { get; set; }
        }
    }
}
