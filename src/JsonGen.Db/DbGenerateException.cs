using System;

namespace JsonGen.Db
{
    public class DbGenerateException : GenerateException
    {
        public string Query { get; set; }

        public DbGenerateException(string query, Exception innerExcpetion) :
            base(innerExcpetion.Message, innerExcpetion)
        {
            this.Query = query;
        }
    }
}
