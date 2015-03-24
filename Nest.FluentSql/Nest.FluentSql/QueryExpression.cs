using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Nest.FluentSql
{
    public class QueryExpression<T>
    {
        public Expression<Func<T, object>> ObjectPath { get; private set; }
        public IList<object> ObjectPathValues { get; private set; }
        public FluentSqlType Type { get; private set; }
        public Operators Operator { get; set; }
        public ValueOperators ValueOperator { get; set; }
        public dynamic MetaData { get; set; }

        public QueryExpression(Expression<Func<T, object>> objectPath, 
            IList<object> objectPathValues, 
            FluentSqlType type = FluentSqlType.Text)
        {
            ObjectPath = objectPath;
            ObjectPathValues = objectPathValues;
            Type = type;
        }
    }
}