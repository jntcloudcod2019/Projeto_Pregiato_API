using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Pregiato.API.Helper
{
    public static class DateFilterExtensions
    {
        public static IQueryable<T> WhereDateEquals<T>(
       this IQueryable<T> query,
       Expression<Func<T, DateTime>> dateSelector,
       DateTime date)
        {
            var nextDay = date.AddDays(1);
            return query.Where(e =>
                EF.Property<DateTime>(e, ((MemberExpression)dateSelector.Body).Member.Name) >= date &&
                EF.Property<DateTime>(e, ((MemberExpression)dateSelector.Body).Member.Name) < nextDay);
        }
    }
}
