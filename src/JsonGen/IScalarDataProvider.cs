using System.Threading.Tasks;

namespace JsonGen
{
    public interface IScalarDataProvider: IDataProvider
    {
        Task<dynamic> GetScalarDataAsync();
    }
}
