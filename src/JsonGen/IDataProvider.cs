using System.Collections.Generic;
using System.Threading.Tasks;

namespace JsonGen
{
    public interface IDataProvider
    {
        Task<IEnumerable<dynamic>> GetDataAsync();
    }
}
