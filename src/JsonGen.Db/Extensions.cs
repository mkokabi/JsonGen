using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonGen.Db
{
    public static class Extensions
    {
        public static Task<string> GenerateAsync(this Generator generator, string metadataName, List<Filter> filters)
        {
            return generator.GenerateAsync(metadataName);
        }
    }
}
