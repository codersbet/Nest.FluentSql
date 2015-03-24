using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Nest.FluentSql
{
    public class FluentSelect<T> where T:class 
    {
        private readonly ElasticClient _elasticClient;
        public FluentSelect(ElasticClient elasticClient)
        {
            if (elasticClient == null)
                throw new ArgumentNullException("elasticClient", "Make sure ElasticClient is configured correctly.");

            _elasticClient = elasticClient;
        }

        public ValueOperatorClause<T> Where(Expression<Func<T, object>> objectPath)
        {
            return new ValueOperatorClause<T>(new List<QueryExpression<T>>(), objectPath, Operators.None, _elasticClient);
        }
    }
}