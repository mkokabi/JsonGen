using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace JsonGen.Db
{
    public class DbDataProvider : IDataProvider
    {
        private readonly IDbConnection dbConnection;
        private readonly string query;

        public DbDataProvider(IDbConnection dbConnection, string query)
        {
            this.dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("The query can not be null or white space", nameof(query));
            }
            this.query = query;
        }

        public IEnumerable<dynamic> GetData()
        {
            return dbConnection.Query(this.query);
        }
    }
}
