using System;
using System.Collections.Generic;

namespace JsonGen
{
    public interface IFilterableDataProvider : IDataProvider
    {
        IEnumerable<dynamic> GetData(Func<dynamic, bool> predicate);
    }
}
