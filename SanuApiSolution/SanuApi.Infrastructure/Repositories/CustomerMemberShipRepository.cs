using Dapper;
using Dapper.Contrib.Extensions;
using Npgsql;
using SanuApi.Domain.Entities;
using SanuApi.Domain.Interfaces;
using System.Data;

namespace SanuApi.Infrastructure.Repositories
{
    public class CustomerMemberShipRepository : ICustomerMembershipRepository
    {
        private readonly IDbConnection _db;

        public CustomerMemberShipRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<int> AddAsync(CustomerMembership entity)
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

        public async Task<bool> DeleteByCustomerIdAsync(int customerId)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();
            var sql = "DELETE FROM customer_x_membership WHERE customerid = @CustomerId";
            var rows = await _db.ExecuteAsync(sql, new { CustomerId = customerId });
            return rows >= 0;
        }
    }
}
