using Dapper;
using SanuApi.Domain.Entities;
using SanuApi.Domain.Interfaces;
using System.Data;

namespace SanuApi.Infrastructure.Repositories
{
    public class TrainerRepository : ITrainerRepository
    {
        private readonly IDbConnection _db;

        public TrainerRepository(IDbConnection db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<IEnumerable<Trainer>> GetAllAsync()
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = "SELECT id, name, lastName, email, telephone FROM trainer WHERE endDate IS NULL ORDER BY lastName, name";
            return await _db.QueryAsync<Trainer>(sql);
        }

        public async Task<int> AddAsync(Trainer entity)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = @"
                INSERT INTO trainer (name, lastName, email, telephone)
                VALUES (@Name, @LastName, @Email, @Telephone)
                RETURNING id;";

            try
            {
                var id = await _db.ExecuteScalarAsync<int>(sql, new
                {
                    Name = entity.name,
                    LastName = entity.lastName,
                    Email = entity.email,
                    Telephone = entity.telephone
                });
                return id;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error al insertar el trainer: {e.Message}", e);
            }
        }

        public async Task<bool> AddClassAsync(TrainerClasses entity)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = @"
                INSERT INTO trainer_x_classes (idtrainer, idclass)
                VALUES (@IdTrainer, @IdClass);";

            try
            {
                var rows = await _db.ExecuteAsync(sql, new
                {
                    IdTrainer = entity.idtrainer,
                    IdClass = entity.idclass
                });
                return rows > 0;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error al asignar la clase al trainer: {e.Message}", e);
            }
        }

        public async Task<IEnumerable<(Classes Class, IEnumerable<Customer> Students)>> GetClassesWithStudentsAsync(int trainerId)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            // Load classes with their dates
            var classSql = @"
                SELECT cl.id, cl.name, cl.idmembership,
                       cd.idclass, cd.day, cd.hour, cd.capacity
                FROM trainer_x_classes txc
                INNER JOIN classes cl ON cl.id = txc.idclass
                LEFT JOIN class_date cd ON cd.idclass = cl.id
                WHERE txc.idtrainer = @TrainerId";

            var classDict = new Dictionary<int, Classes>();
            await _db.QueryAsync<Classes, ClassDate, Classes>(
                classSql,
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
                new { TrainerId = trainerId },
                splitOn: "idclass"
            );

            // Load students per class
            var studentSql = @"
                SELECT cxc.classid, c.id, c.customername, c.customerlastname
                FROM trainer_x_classes txc
                INNER JOIN class_x_customer cxc ON cxc.classid = txc.idclass
                INNER JOIN customer c ON c.id = cxc.customerid AND c.fechabaja IS NULL
                WHERE txc.idtrainer = @TrainerId";

            var studentRows = await _db.QueryAsync<StudentClassRow>(studentSql, new { TrainerId = trainerId });
            var studentsByClass = studentRows
                .GroupBy(r => r.classid)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => new Customer { id = r.id, customername = r.customername, customerlastname = r.customerlastname }).ToList()
                );

            return classDict.Values.Select(cls => (
                cls,
                (IEnumerable<Customer>)(studentsByClass.TryGetValue(cls.id, out var students) ? students : new List<Customer>())
            ));
        }

        private class StudentClassRow
        {
            public int classid { get; set; }
            public int id { get; set; }
            public string customername { get; set; }
            public string customerlastname { get; set; }
        }

        public async Task<Trainer?> FindByIdAsync(int id)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = "SELECT id, name, lastName, endDate FROM trainer WHERE id = @Id";
            return await _db.QuerySingleOrDefaultAsync<Trainer>(sql, new { Id = id });
        }

        public async Task<bool> UpdateAsync(Trainer entity)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = @"UPDATE trainer
                        SET name      = COALESCE(@Name, name),
                            lastName  = COALESCE(@LastName, lastName),
                            email     = COALESCE(@Email, email),
                            telephone = COALESCE(@Telephone, telephone)
                        WHERE id = @Id AND endDate IS NULL";

            var rows = await _db.ExecuteAsync(sql, new
            {
                Name = entity.name,
                LastName = entity.lastName,
                Email = entity.email,
                Telephone = entity.telephone,
                Id = entity.id
            });
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = "UPDATE trainer SET endDate = @EndDate WHERE id = @Id AND endDate IS NULL";
            var rows = await _db.ExecuteAsync(sql, new { EndDate = DateTime.UtcNow, Id = id });
            return rows > 0;
        }
    }
}
