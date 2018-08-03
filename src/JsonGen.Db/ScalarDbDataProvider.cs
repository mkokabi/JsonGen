using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static JsonGen.Filter;

namespace JsonGen.Db
{
    public class ScalarDbDataProvider : DbDataProvider
    {        
        public async Task<dynamic> GetScalarAsync()
        {
            return (await dbConnection.ExecuteScalarAsync(this.query));
        }

        public async Task<dynamic> GetScalarDataAsync(Func<dynamic, bool> predicate)
        {
            var query = await dbConnection.QueryAsync(this.query);
            if (predicate != null)
            {
                return query.Where(predicate).FirstOrDefault();
            }
            else
            {
                return query.FirstOrDefault();
            }
        }

        public async Task<dynamic> GetScalarDataAsync(Filter[] filters)
        {
            ApplyFilters(filters);
            return (await dbConnection.ExecuteScalarAsync(this.query));
        }
    }
}
