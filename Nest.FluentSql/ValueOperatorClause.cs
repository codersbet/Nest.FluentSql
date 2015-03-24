using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;

namespace Nest.FluentSql
{
    public class ValueOperatorClause<T> where T : class 
    {
        private readonly List<QueryExpression<T>> _queries;
        private readonly Expression<Func<T, object>> _objectPath;
        private readonly Operators _operator;
        private readonly ElasticClient _elasticClient;

        public ValueOperatorClause(List<QueryExpression<T>> queries, Expression<Func<T, object>> objectPath, Operators @operator, ElasticClient elasticClient)
        {
            _queries = queries;
            _objectPath = objectPath;
            _operator = @operator;
            _elasticClient = elasticClient;
        }

        public AndOrClause<T> Equals(int value)
        {
            return In(value);
        }

        public AndOrClause<T> Like(string value, bool useFullTextSearch = false)
        {
            return Like(useFullTextSearch, value);
        }

        public AndOrClause<T> Equals(string value)
        {
            return Like(value);
        }

        public AndOrClause<T> Like(bool useFullTextSearch = false, params string[] values)
        {
            if (_objectPath == null || (values == null || values.Length <= 0))
                return new AndOrClause<T>(_queries, _elasticClient);

            dynamic metaData = new ExpandoObject();
            metaData.UseFullTextSearch = useFullTextSearch;

            return Create(values, FluentSqlType.Text, ValueOperators.In, metaData);
        }

        public AndOrClause<T> In(params int[] values)
        {
            if (_objectPath == null || (values == null || values.Length <= 0))
                return new AndOrClause<T>(_queries, _elasticClient);

            return Create(values, FluentSqlType.Numeric, ValueOperators.In );
        }

        public AndOrClause<T> Between(DateTime fromDateTime, DateTime toDateTime, string format = "yyyy-MM-dd'T'HH:mm:ss.fff")
        {
            if (_objectPath == null)
                return new AndOrClause<T>(_queries, _elasticClient);

            var values = new[] {fromDateTime, toDateTime};

            dynamic metaData = new ExpandoObject();
            metaData.DateFormat = format;

            return Create(values, FluentSqlType.DateTime, ValueOperators.Between);
        }

        #region Private Methods
        private AndOrClause<T> Create(dynamic dynamicValues, FluentSqlType type, 
            ValueOperators valueOperator, dynamic metaData = null)
        {
            var objects = new List<object>();
            foreach (var dynamicValue in dynamicValues)
                objects.Add(dynamicValue);

            var query = new QueryExpression<T>(_objectPath, objects, type)
            {
                Operator = _operator,
                ValueOperator = valueOperator,
                MetaData = metaData
            };

            _queries.Add(query);
            return new AndOrClause<T>(_queries, _elasticClient);
        }
        #endregion
    }
}