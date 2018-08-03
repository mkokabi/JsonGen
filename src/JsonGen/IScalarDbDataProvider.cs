using System.Threading.Tasks;

namespace JsonGen
{
    public interface IScalarDbDataProvider : IScalarFilterableDataProvider, IDbDataProvider
    {
        Task<dynamic> GetScalarDataAsync(Filter[] filters);
    }
}
