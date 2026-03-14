
using Dapper;
using SanuApi.Domain.Entities;
using SanuApi.Domain.Interfaces;
using System.Data;

namespace SanuApi.Infrastructure.Repositories
{
    public class HealthCustomerRepository : IHealthCustomerRepository
    {
        private readonly IDbConnection _db;

        public HealthCustomerRepository(IDbConnection db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }
        public void TestConnection()
        {
            Console.WriteLine($"👉 Estado antes de abrir: {_db.State}");
            if (_db.State != ConnectionState.Open)
                _db.Open();
            Console.WriteLine($"✅ Estado después de abrir: {_db.State}");
        }
        public async Task<int> AddAsync(HealthCustomer entity)
        {

            var sql = @"
        INSERT INTO healthcustomer (customerid, height, weight, alergics, medicalCondicion)
        VALUES (@CustomerId, @Height, @Weight, @Alergics, @MedicalCondicion)
        RETURNING id;";

            try
            {

                var id = await _db.ExecuteScalarAsync<int>(sql, new
                {
                    CustomerId = entity.customerid,
                    Height = entity.heigth,
                    Weight = entity.weight,
                    Alergics = entity.alergics,
                    MedicalCondicion = entity.medicalCondicion

                });

                return id;
            }
            catch (Exception e)
            {

                throw new InvalidOperationException($"Error al insertar la condición médica: {e.Message}", e);
            }
        }

        public async Task<bool> UpsertAsync(HealthCustomer entity)
        {
            var sql = @"
        INSERT INTO healthcustomer (customerid, height, weight, alergics, medicalCondicion)
        VALUES (@CustomerId, @Height, @Weight, @Alergics, @MedicalCondicion)
        ON CONFLICT (customerid) DO UPDATE SET
            height          = EXCLUDED.height,
            weight          = EXCLUDED.weight,
            alergics        = EXCLUDED.alergics,
            medicalCondicion = EXCLUDED.medicalCondicion;";

            try
            {
                if (_db.State != ConnectionState.Open)
                    _db.Open();

                var rows = await _db.ExecuteAsync(sql, new
                {
                    CustomerId = entity.customerid,
                    Height = entity.heigth,
                    Weight = entity.weight,
                    Alergics = entity.alergics,
                    MedicalCondicion = entity.medicalCondicion
                });

                return rows > 0;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error al actualizar la condición médica: {e.Message}", e);
            }
        }
    }
}

