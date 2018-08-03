using Dapper;

using System.Threading.Tasks;

namespace JsonGen.Db
{
    public class ScalarDbDataProvider : DbDataProvider, IScalarDbDataProvider

    {        
        public async Task<dynamic> GetScalarAsync()
        {
            return (await dbConnection.ExecuteScalarAsync(this.query));
        }


        public async Task<dynamic> GetScalarDataAsync(Filter[] filters)
        {
            ApplyFilters(filters);
            return (await dbConnection.ExecuteScalarAsync(this.query));
        }

        public async Task<dynamic> GetScalarDataAsync()
        {
            return (await dbConnection.ExecuteScalarAsync(this.query));
        }
    }
}
