using System;
using System.Linq.Expressions;

namespace Nest.FluentSql
{
    public class WhereClause<T> where T:class 
    {
        public Expression<Func<T, object>> ObjectPath { get; private set; }
        public WhereClause(Expression<Func<T, object>> objectPath)
        {
            ObjectPath = objectPath;
        }

        public AndOrClause<T> In(params int[] values)
        {
            return new AndOrClause<T>(FluentSqlType.Numeric, ObjectPath, values);
        }

        public AndOrClause<T> Equals(int value)
        {
            return In(value);
        }

        public AndOrClause<T> In(params decimal[] values)
        {
            return new AndOrClause<T>(FluentSqlType.Numeric, ObjectPath, values);
        }

        public AndOrClause<T> Equals(decimal value)
        {
            return In(value);
        }

        public AndOrClause<T> In(params object[] values)
        {
            return new AndOrClause<T>(FluentSqlType.Text, ObjectPath, values);
        }

        public new AndOrClause<T> Equals(object value)
        {
            return In(value);
        }
    }
}