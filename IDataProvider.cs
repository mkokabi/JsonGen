using System.Collections.Generic;

namespace JsonGen
{
    public interface IDataProvider
    {
        IEnumerable<dynamic> GetData();
    }
}
