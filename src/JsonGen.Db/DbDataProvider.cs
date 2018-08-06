using Dapper;
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
        protected IDbConnection dbConnection;
        protected string query;
        internal Dictionary<Operators, string> dbOperators = new Dictionary<Operators, string>
        {
            { Operators.Eq, "=" },
            { Operators.G, ">" },
            { Operators.L, "<" },
            { Operators.GE, ">=" },
            { Operators.LE, "<=" },
            { Operators.In, "in" },
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

        protected void ApplyFilters(Filter[] filters)
        {
            if (filters.Any())
            {
                query += query.ToUpper().Contains("WHERE") ? " and " : " where ";

                foreach (var filter in filters)
                {
                    string quotedValue = string.Empty;

                    if (filter.Operator == Operators.In)
                    {
                        if (filter.Value.GetType().IsArray)
                        {
                            foreach (var item in filter.Value)
                            {
                                quotedValue += Quote(item) + ",";
                            }
                            quotedValue = $"({quotedValue.TrimEnd(',')})";
                        }
                        else
                        {
                            throw new GenerateException("The type of filter values should be an array.");
                        }
                    }
                    else
                    {
                        quotedValue = Quote(filter.Value);
                    }
                    query += $"{filter.FieldName} {dbOperators[filter.Operator]} {quotedValue}";
                    if (filter != filters.Last())
                    {
                        query += " and ";
                    }
                }
            }
        }

        public async Task<IEnumerable<dynamic>> GetDataAsync(Filter[] filters)
        {
            ApplyFilters(filters);
            return (await dbConnection.QueryAsync(this.query));
        }

        private string Quote(dynamic value)
        {
            if (value.GetType() == typeof(string))
            {
                return $"'{value}'";
            }
            return value.ToString();
        }
    }
}
