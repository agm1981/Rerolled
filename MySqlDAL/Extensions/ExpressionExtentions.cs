using System;
using System.Linq.Expressions;

namespace Common.Extensions
{
    public static class ExpressionExtentions
    {
        public static Expression<Func<T, bool>> CombineAnd<T>(Expression<Func<T, bool>>  filter1, Expression<Func<T, bool>> filter2)
        {
            // combine two predicates:
            // need to rewrite one of the lambdas, swapping in the parameter from the other
            var rewrittenBody1 = new ReplaceVisitor(
                filter1.Parameters[0], filter2.Parameters[0]).Visit(filter1.Body);
            var newFilter = Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(rewrittenBody1, filter2.Body), filter2.Parameters);
            return newFilter;
        }
        class ReplaceVisitor : ExpressionVisitor
        {
            private readonly Expression from, to;
            public ReplaceVisitor(Expression from, Expression to)
            {
                this.from = from;
                this.to = to;
            }
            public override Expression Visit(Expression node)
            {
                return node == from ? to : base.Visit(node);
            }
        }

    }

}
