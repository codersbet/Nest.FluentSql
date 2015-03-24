using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Nest.FluentSql
{
    public class AndOrClause<T> where T : class 
    {
        private readonly ElasticClient _elasticClient;
        private readonly List<QueryExpression<T>> _queries;

        public AndOrClause(FluentSqlType fluentSqlType, Expression<Func<T, object>> objectPath = null, 
            dynamic values = null)
        {
            _queries = new List<QueryExpression<T>>();

            if (objectPath == null || values == null || values.Length <= 0) return;

            var objects = new List<object>();
            foreach (var whereValue in values)
                objects.Add(whereValue);

            _queries.Add(new QueryExpression<T>(objectPath, objects, fluentSqlType));
        }

        public AndOrClause(List<QueryExpression<T>> queries, ElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
            _queries = queries ?? new List<QueryExpression<T>>();
        }

        public ValueOperatorClause<T> And(Expression<Func<T, object>> objectPath)
        {
            return new ValueOperatorClause<T>(_queries, objectPath, Operators.And, _elasticClient);
        }

        public ValueOperatorClause<T> Or(Expression<Func<T, object>> objectPath)
        {
            return new ValueOperatorClause<T>(_queries, objectPath, Operators.Or, _elasticClient);
        }

        public ISearchResponse<T> Execute()
        {
            return _elasticClient.Search<T>(BuildQuery());
        }

        private SearchDescriptor<T> BuildQuery()
        {
            var queryDescriptor = new QueryDescriptor<T>();
            QueryContainer queryContainer = null;
            foreach (var expressionAndValues in _queries)
            {
                if (queryContainer == null)
                {
                    queryContainer = BuildQueryContainer(queryDescriptor, expressionAndValues);
                }
                else
                {
                    switch (expressionAndValues.Operator)
                    {
                        case Operators.And:
                            queryContainer = queryContainer && BuildQueryContainer(queryDescriptor, expressionAndValues);
                            break;
                        case Operators.Or:
                            queryContainer = queryContainer || BuildQueryContainer(queryDescriptor, expressionAndValues);
                            break;
                    }
                }
            }
           return new SearchDescriptor<T>().Query(queryContainer);
        }

        private static QueryContainer BuildQueryContainer(QueryDescriptor<T> queryDescriptor, QueryExpression<T> expressionAndValues)
        {
            QueryContainer queryContainer =  null;
            if (expressionAndValues.ValueOperator == ValueOperators.Between && expressionAndValues.ObjectPathValues.Count == 2)
            {
                if (expressionAndValues.Type == FluentSqlType.DateTime)
                {
                    var dateFormat = "yyyy-MM-dd'T'HH:mm:ss.fff";
                    if (expressionAndValues.MetaData != null
                        && !string.IsNullOrEmpty(Convert.ToString(expressionAndValues.MetaData.DateFormat)))
                    {
                        dateFormat = expressionAndValues.MetaData.DateFormat.ToString();
                    }

                    queryContainer = queryDescriptor.Range(r => r.OnField(expressionAndValues.ObjectPath)
                        .GreaterOrEquals(DateTime.Parse(Convert.ToString(expressionAndValues.ObjectPathValues[0])), dateFormat)
                        .LowerOrEquals(DateTime.Parse(Convert.ToString(expressionAndValues.ObjectPathValues[1])), dateFormat));
                }
                else
                {
                        queryContainer = queryContainer || 
                        queryDescriptor.Range(r => r.OnField(expressionAndValues.ObjectPath)
                            .GreaterOrEquals(DateTime.Parse(Convert.ToString(expressionAndValues.ObjectPathValues[0])))
                            .LowerOrEquals(DateTime.Parse(Convert.ToString(expressionAndValues.ObjectPathValues[1]))));
                }
            }
            else if (expressionAndValues.ValueOperator == ValueOperators.In && expressionAndValues.ObjectPathValues.Count > 0)
            {
                switch (expressionAndValues.Type)
                {
                    case FluentSqlType.Numeric:
                        foreach (var value in expressionAndValues.ObjectPathValues)
                        {
                            queryContainer = queryContainer || queryDescriptor.Term(expressionAndValues.ObjectPath, value);
                        }
                        break;
                    case FluentSqlType.Text:
                        foreach (var value in expressionAndValues.ObjectPathValues)
                        {
                            queryContainer = BuildQueryContainerForText(Convert.ToString(value), queryDescriptor,
                                expressionAndValues, queryContainer);
                        }
                        break;
                }
            }
            return queryContainer;
        }

        private static QueryContainer BuildQueryContainerForText(string value, QueryDescriptor<T> queryDescriptor, QueryExpression<T> expressionAndValues, QueryContainer queryContainer)
        {
            var stringValue = Convert.ToString(value);
            var useFullTextSearch = false;

            if (expressionAndValues.MetaData != null &&
                expressionAndValues.MetaData.UseFullTextSearch != null)
                useFullTextSearch = Convert.ToBoolean(expressionAndValues.MetaData.UseFullTextSearch);

            if (queryContainer == null)
            {
                if (useFullTextSearch)
                {
                    queryContainer =
                        queryDescriptor.Match(m => m.OnField(expressionAndValues.ObjectPath)
                            .Query(Convert.ToString(stringValue)));
                }
                else
                {
                    queryContainer =
                        queryDescriptor.MatchPhrase(m => m.OnField(expressionAndValues.ObjectPath)
                            .Query(Convert.ToString(stringValue)));
                }
            }
            else
            {
                if (useFullTextSearch)
                {
                    queryContainer = queryDescriptor ||
                                     queryDescriptor.MatchPhrase(m => m.OnField(expressionAndValues.ObjectPath)
                                             .Query(Convert.ToString(stringValue)));
                }
                else
                {
                    queryContainer = queryDescriptor ||
                                 queryDescriptor.Match(m => m.OnField(expressionAndValues.ObjectPath)
                                         .Query(Convert.ToString(stringValue)));
                }
            }
            return queryContainer;
        }
    }
}