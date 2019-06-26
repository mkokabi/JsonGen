using System.Threading.Tasks;

namespace JsonGen
{
    public interface ISingleRowDataProvider
    {
        Task<dynamic> GetSingleRowDataAsync(Filter[] filters);
    }
}
