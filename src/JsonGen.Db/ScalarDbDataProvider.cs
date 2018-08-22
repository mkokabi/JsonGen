using Dapper;
using System;
using System.Threading.Tasks;

namespace JsonGen.Db
{
    public class ScalarDbDataProvider : DbDataProvider, IScalarDbDataProvider

    {        
        public async Task<dynamic> GetScalarAsync()
        {
            try
            {
                return (await dbConnection.ExecuteScalarAsync(this.query));
            }
            catch (Exception exc)
            {
                throw new DbGenerateException(this.query, exc);
            }
        }


        public async Task<dynamic> GetScalarDataAsync(Filter[] filters)
        {
            try
            {
                ApplyFilters(filters);
                return (await dbConnection.ExecuteScalarAsync(this.query));
            }
            catch (Exception exc)
            {
                throw new DbGenerateException(this.query, exc);
            }
        }

        public async Task<dynamic> GetScalarDataAsync()
        {
            try
            {
                return (await dbConnection.ExecuteScalarAsync(this.query));
            }
            catch (Exception exc)
            {
                throw new DbGenerateException(this.query, exc);
            }
        }
    }
}
