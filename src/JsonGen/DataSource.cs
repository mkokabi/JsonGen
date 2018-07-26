using System.Data;

namespace JsonGen
{
    public class DataSource
	{
        public string Key { get; set; }

        public string DataProviderFullName { get; set; }

        public IDbConnection DbConnection { get; set; }

        public string Query { get; set; }
    }
}