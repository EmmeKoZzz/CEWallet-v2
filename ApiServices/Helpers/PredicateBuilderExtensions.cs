using System.Linq.Expressions;

namespace ApiServices.Helpers;

public static class PredicateBuilderExtensions {
	public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
		Expression<Func<T, bool>> expr2) {
		var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);

		return Expression.Lambda<Func<T, bool>>(Expression.Or(expr1.Body, invokedExpr), expr1.Parameters);
	}
}