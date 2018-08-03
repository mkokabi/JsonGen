using System.Threading.Tasks;

namespace JsonGen
{
    public interface IScalarDbDataProvider : IScalarDataProvider, IDbDataProvider 
    {
        Task<dynamic> GetScalarDataAsync(Filter[] filters);
    }
}
