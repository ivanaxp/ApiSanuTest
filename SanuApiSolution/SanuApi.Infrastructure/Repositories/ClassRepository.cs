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

        public async Task<IEnumerable<Classes>> GetAllAsync()
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = "SELECT id, name, day, hour, capacity FROM public.classes ";
            var classes = await _db.QueryAsync<Classes>(sql);
            return classes;
        }
    }
}
