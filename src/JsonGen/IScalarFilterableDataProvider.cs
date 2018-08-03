using System;
using System.Threading.Tasks;

namespace JsonGen
{
    public interface IScalarFilterableDataProvider : IScalarDataProvider
    {
        Task<dynamic> GetScalarDataAsync(Func<dynamic, bool> predicate);
    }
}
