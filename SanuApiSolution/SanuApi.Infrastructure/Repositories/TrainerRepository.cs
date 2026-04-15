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

        public async Task<int> AddAsync(Trainer entity)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = @"
                INSERT INTO trainer (name, lastName)
                VALUES (@Name, @LastName)
                RETURNING id;";

            try
            {
                var id = await _db.ExecuteScalarAsync<int>(sql, new
                {
                    Name = entity.name,
                    LastName = entity.lastName
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

            var sql = @"
                SELECT cl.id, cl.name, cl.day, cl.hour, cl.capacity,
                       c.id, c.customername, c.customerlastname
                FROM trainer_x_classes txc
                INNER JOIN classes cl ON cl.id = txc.idclass
                LEFT JOIN class_x_customer cxc ON cl.id = cxc.classid
                LEFT JOIN customer c ON c.id = cxc.customerid AND c.fechabaja IS NULL
                WHERE txc.idtrainer = @TrainerId";

            var classesDictionary = new Dictionary<int, (Classes Class, List<Customer> Students)>();

            await _db.QueryAsync<Classes, Customer, Classes>(
                sql,
                (cls, customer) =>
                {
                    if (!classesDictionary.TryGetValue(cls.id, out var entry))
                    {
                        entry = (cls, new List<Customer>());
                        classesDictionary.Add(cls.id, entry);
                    }

                    if (customer != null && customer.id != 0)
                        entry.Students.Add(customer);

                    return cls;
                },
                new { TrainerId = trainerId },
                splitOn: "id"
            );

            return classesDictionary.Values
                .Select(e => (e.Class, (IEnumerable<Customer>)e.Students));
        }
    }
}
