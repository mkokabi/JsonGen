using System;

namespace JsonGen
{
    public interface IGenerator
    {
        string Generate(string reportName, Func<dynamic, bool> predicate = null);
    }
}