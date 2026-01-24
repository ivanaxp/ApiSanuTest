using Dapper;
using Dapper.Contrib.Extensions;
using SanuApi.Domain.Entities;
using SanuApi.Domain.Interfaces;
using System.Data;

namespace SanuApi.Infrastructure.Repositories
{
    public class GoalRepository : IGoalRepository
    {
        private readonly IDbConnection _db;

        public GoalRepository(IDbConnection db)
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

        public async Task<int> AddAsync(Goal entity)
        {
            
            using (_db)
            {                
                var sql = @"
            INSERT INTO goal (goalname, fechabaja) 
            VALUES (@GoalName, @FechaBaja)
            RETURNING id;";

                try
                {                 
                    
                    var id = await _db.ExecuteScalarAsync<int>(sql, new
                    {
                        GoalName = entity.goalname,
                        FechaBaja = entity.fechaBaja 
                    });
                    
                    return id;
                }
                catch (Exception e)
                {
                    
                    throw new InvalidOperationException($"Error al insertar el objetivo: {e.Message}", e);
                }
            }
        }

        public async Task<Goal?> FindByIdAsync(int id)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();
            var goal = await _db.GetAsync<Goal>(id);
            return goal;
        }

        public async Task<IEnumerable<Goal>> GetAllAsync()
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = "SELECT id, goalname FROM goal WHERE fechabaja IS NULL"; 
            var goals = await _db.QueryAsync<Goal>(sql);
            return goals;
        }

        public async Task<int> AddCustomerGoalAsync(CustomerGoal entity)
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
                throw new InvalidOperationException("Error al insertar el objetivo del cliente", e);
            }
        }

        public async Task<bool> DeleteAsync(Goal entity)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();
            var goal = await _db.DeleteAsync(entity);
            return goal;
        }
    }
    }

