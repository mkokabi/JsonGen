﻿using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static JsonGen.Filter;

namespace JsonGen.Db
{
    public class DbDataProvider : IDbDataProvider
    {
        private IDbConnection dbConnection;
        private string query;
        private Dictionary<Operators, string> dbOperators = new Dictionary<Operators, string>
        {
            { Operators.Eq, "=" },
            { Operators.G, ">" },
            { Operators.L, "<" },
            { Operators.GE, ">=" },
            { Operators.LE, "<=" },
        };

        public IDbConnection DbConnection { get => dbConnection; set => dbConnection = value; }
        public string Query { get => query; set => query = value; }

        public async Task<IEnumerable<dynamic>> GetDataAsync()
        {
            return await dbConnection.QueryAsync(this.query);
        }

        public async Task<IEnumerable<dynamic>> GetDataAsync(Func<dynamic, bool> predicate)
        {
            var query = await dbConnection.QueryAsync(this.query);
            if (predicate != null)
            {
                return query.Where(predicate);
            }
            else
            {
                return query;
            }
        }

        public async Task<IEnumerable<dynamic>> GetDataAsync(Filter[] filters)
        {
            if (filters.Any() && !query.ToUpper().Contains("WHERE"))
            {
                query += " where ";
            }
            foreach (var filter in filters)
            {
                query += $"{filter.FieldName} {dbOperators[filter.Operator]} {filter.Value}";
                if (filter != filters.Last())
                {
                    query += " and ";
                }
            }
            return (await dbConnection.QueryAsync(this.query));
        }
    }
}
