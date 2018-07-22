using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JsonGen
{
    public interface IFilterableDataProvider : IDataProvider
    {
        Task<IEnumerable<dynamic>> GetDataAsync(Func<dynamic, bool> predicate);
    }
}
