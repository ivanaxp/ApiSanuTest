
using Dapper;
using Dapper.Contrib.Extensions;
using SanuApi.Domain.Entities;
using SanuApi.Domain.Interfaces;
using System.Data;
using System.Data.Common;
using static Dapper.SqlMapper;

namespace SanuApi.Infrastructure.Repositories
{
    public class MembershipRepository : IMembershipRepository
    {
        private readonly IDbConnection _db;

        public MembershipRepository(IDbConnection db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<Membership?> FindByIdAsync(int id)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();
            var membership = await _db.GetAsync<Membership>(id);
            return membership;
        }
        public async Task<int> AddAsync(Membership entity)
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
                throw new InvalidOperationException(e + "Error al insertar la membresía.");
            }
        }

        public async Task<IEnumerable<Membership>> GetAllAsync()
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            var sql = "SELECT id, name, price, frecuency FROM membership ";
            var memberships = await _db.QueryAsync<Membership>(sql);
            return memberships;
        }

        public async Task<bool> UpdateAsync(Membership entity)
        {
            try
            {
                if (_db.State != ConnectionState.Open)
                    _db.Open();

                var update = await _db.UpdateAsync(entity);
                return update;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(e + "Error al actualizar la membresía.");
            }
        }

        public async Task<bool> DeleteAsync(int idMembership)
        {

            const string sqlQuery = "DELETE FROM membership m " +
                "WHERE m.id = @IdMembership " +
                "AND NOT EXISTS (SELECT 1 FROM customer_x_membership cxm WHERE cxm.membershipid = m.id " +
                " AND cxm.enddate < NOW()) ";

            using (var connection = _db)
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sqlQuery;


                if (connection.State != ConnectionState.Open)
                {
                    await ((DbConnection)connection).OpenAsync();

                }


                var parameter = command.CreateParameter();
                parameter.ParameterName = "@IdMembership";
                parameter.Value = idMembership;
                command.Parameters.Add(parameter);

                try
                {

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al eliminar Membership: {ex.Message}");
                    return false;
                }
            }
        }
    }
}

