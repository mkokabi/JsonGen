using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JsonGen
{
    public interface IGenerator
    {
        string Generate(string reportName, Func<dynamic, bool> predicate = null, Dictionary<string, dynamic> parameters = null);
        Task<string> GenerateAsync(string reportName, Func<dynamic, bool> predicate = null, Filter[] filters = null, Dictionary<string, dynamic> parameters = null);
    }
}