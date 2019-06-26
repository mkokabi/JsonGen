using Dapper;
using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JsonGen.Db
{
    public class DbSingleRowDataProvider : DbDataProvider, ISingleRowDataProvider
    {
        public async Task<dynamic> GetSingleRowDataAsync(Filter[] filters)
        {
            try
            {
                if (CommandType == CommandType.StoredProcedure)
                {
                    DynamicParameters @params = AddFiltersAsParameters(filters);
                    this.query = Regex.Replace(this.Query, ExecRegex, string.Empty, RegexOptions.IgnoreCase);
                    return (await dbConnection.QueryAsync(this.query, param: @params, commandType: this.CommandType));
                }
                else
                {
                    ApplyFilters(filters);
                    return (await dbConnection.QueryAsync(this.query, commandType: this.CommandType));
                }
            }
            catch (Exception exc)
            {
                throw new DbGenerateException(this.query, exc);
            }
        }
    }
}
