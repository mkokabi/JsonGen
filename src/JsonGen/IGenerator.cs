using System;
using System.Threading.Tasks;

namespace JsonGen
{
    public interface IGenerator
    {
        string Generate(string reportName, Func<dynamic, bool> predicate = null);
        Task<string> GenerateAsync(string reportName, Func<dynamic, bool> predicate = null, Filter[] filters = null);
    }
}