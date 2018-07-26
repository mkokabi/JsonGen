using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JsonGen.Db
{
    public class DbDataProvider : IDbDataProvider
    {
        private IDbConnection dbConnection;
        private string query;

        public IDbConnection DbConnection { get => dbConnection; set => dbConnection = value; }
        public string Query { get => query; set => query = value; }

        public async Task<IEnumerable<dynamic>> GetDataAsync()
        {
            return await dbConnection.QueryAsync(this.query);
        }

        public async Task<IEnumerable<dynamic>> GetDataAsync(Func<dynamic, bool> predicate)
        {
            var query = await dbConnection.QueryAsync(this.query);
            if (predicate != null)
            {
                return query.Where(predicate);
            }
            else
            {
                return query;
            }
        }
    }
}
