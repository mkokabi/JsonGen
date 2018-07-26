using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace JsonGen
{
    public interface IDbDataProvider : IFilterableDataProvider
    {
        IDbConnection DbConnection { get; set; }
        string Query { get; set; }
        Task<IEnumerable<dynamic>> GetDataAsync(Filter[] filters);
    }
}
