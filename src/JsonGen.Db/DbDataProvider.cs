using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JsonGen.Db
{
    public class DbDataProvider : IFilterableDataProvider
    {
        private readonly IDbConnection dbConnection;
        private readonly string query;

        public DbDataProvider(IDbConnection dbConnection, string query)
        {
            this.dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("The query can not be null or white space", nameof(query));
            }
            this.query = query;
        }

        public async Task<IEnumerable<dynamic>> GetDataAsync()
        {
            return await dbConnection.QueryAsync(this.query);
        }

        public async Task<IEnumerable<dynamic>> GetDataAsync(Func<dynamic, bool> predicate)
        {
            var query = await dbConnection.QueryAsync(this.query);
            return query.Where(predicate);
        }
    }
}
