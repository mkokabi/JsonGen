using System.Data;

namespace JsonGen
{
    public interface IDbDataProvider : IFilterableDataProvider
    {
        IDbConnection DbConnection { get; set; }
        string Query { get; set; }
    }
}
