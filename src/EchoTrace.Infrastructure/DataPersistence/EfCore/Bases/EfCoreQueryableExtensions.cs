using System.Linq.Expressions;
using EchoTrace.Infrastructure.DataPersistence.DataEntityBases;
using Microsoft.EntityFrameworkCore;

namespace EchoTrace.Infrastructure.DataPersistence.EfCore.Bases;

public static class EfCoreQueryableExtensions
{
    public static IQueryable<T> Sort<T>(this IQueryable<T> query, ISortable? sortable) where T : class
    {
        if (sortable is { Sort: not null })
        {
            var type = typeof(T);
            var prop = type.GetProperties().FirstOrDefault(e =>
                e.Name.Equals(sortable.Sort.FieldName, StringComparison.OrdinalIgnoreCase));
            if (prop != null)
            {
                var parameterExpression = Expression.Parameter(type, "x");
                var memberExpression = Expression.MakeMemberAccess(parameterExpression, prop);
                var filterExpress =
                    Expression.Lambda<Func<T, object>>(Expression.Convert(memberExpression, typeof(object)),
                        parameterExpression);
                query = sortable.Sort.Direction == SortDirectionEnum.Descending
                    ? query.OrderByDescending(filterExpress)
                    : query.OrderBy(filterExpress);
            }
        }

        return query;
    }

    public static async Task<PaginatedResult<T>> PaginateAsync<T>(this IOrderedQueryable<T> orderByQuery,
        IPageable? pageable,
        CancellationToken cancellationToken) where T : class
    {
        PaginatedResult<T> paginatedResult = new()
        {
            Total = await orderByQuery.CountAsync(cancellationToken)
        };
        var query = pageable != null ? orderByQuery.Skip((pageable.Offset - 1) * pageable.PageSize).Take(pageable.PageSize) : orderByQuery;
        
        paginatedResult.List = await query.ToListAsync(cancellationToken);
        return paginatedResult;
    }

    public static IQueryable<T> DynamicFilterAnd<T>(this IQueryable<T> query, List<DynamicFilterParameter>? filters)
        where T : class
    {
        if (filters == null || filters.Count == 0)
        {
            return query;
        }

        var type = typeof(T);
        var parameterExpression = Expression.Parameter(type, "x");
        Expression finalBody = null;

        foreach (var filter in filters)
        {
            if (filter is { Value: not null, FieldName: not null, Operator: not null })
            {
                var prop = type.GetProperties().FirstOrDefault(e =>
                    e.Name.Equals(filter.FieldName, StringComparison.OrdinalIgnoreCase));
                MemberExpression memberExpression = null;
                Type propType = null;
                if (prop != null)
                {
                    memberExpression = Expression.MakeMemberAccess(parameterExpression, prop);
                    propType = prop.PropertyType;
                }
                else
                {
                    var fieldInfo = type.GetFields()
                        .FirstOrDefault(e => e.Name.Equals(filter.FieldName, StringComparison.OrdinalIgnoreCase));
                    if (fieldInfo != null)
                    {
                        memberExpression = Expression.MakeMemberAccess(parameterExpression, fieldInfo);
                        propType = fieldInfo.FieldType;
                    }
                }

                if (memberExpression != null)
                {
                    Expression body = null;
                    var valueExpression = Expression.Convert(Expression.Constant(filter.Value), propType);
                    switch (filter.Operator)
                    {
                        case DynamicFilterOperator.Equal:
                            body = Expression.Equal(memberExpression, valueExpression);
                            break;
                        case DynamicFilterOperator.NotEqual:
                            body = Expression.NotEqual(memberExpression, valueExpression);
                            break;
                        case DynamicFilterOperator.GreaterThan:
                            body = Expression.GreaterThan(memberExpression, valueExpression);
                            break;
                        case DynamicFilterOperator.GreaterThanOrEqual:
                            body = Expression.GreaterThanOrEqual(memberExpression, valueExpression);
                            break;
                        case DynamicFilterOperator.LessThan:
                            body = Expression.LessThan(memberExpression, valueExpression);
                            break;
                        case DynamicFilterOperator.LessThanOrEqual:
                            body = Expression.LessThanOrEqual(memberExpression, valueExpression);
                            break;
                        case DynamicFilterOperator.Contains:
                            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                            if (containsMethod != null)
                            {
                                body = Expression.Call(memberExpression, containsMethod, valueExpression);
                            }

                            break;
                    }

                    if (body != null)
                    {
                        if (finalBody == null)
                        {
                            finalBody = body;
                        }
                        else
                        {
                            finalBody = Expression.AndAlso(finalBody, body);
                        }
                    }
                }
            }
        }

        if (finalBody != null)
        {
            var finalExpression = Expression.Lambda<Func<T, bool>>(finalBody, parameterExpression);
            query = query.Where(finalExpression);
        }

        return query;
    }

    public static IQueryable<T> DynamicFilterOr<T>(this IQueryable<T> query, List<DynamicFilterParameter>? filters)
        where T : class
    {
        if (filters == null || filters.Count == 0)
        {
            return query;
        }

        var type = typeof(T);
        var parameterExpression = Expression.Parameter(type, "x");
        Expression finalBody = null;

        foreach (var filter in filters)
        {
            var prop = type.GetProperties().FirstOrDefault(e =>
                e.Name.Equals(filter.FieldName, StringComparison.OrdinalIgnoreCase));
            MemberExpression memberExpression = null;
            Type propType = null;
            if (prop != null)
            {
                memberExpression = Expression.MakeMemberAccess(parameterExpression, prop);
                propType = prop.PropertyType;
            }
            else
            {
                var fieldInfo = type.GetFields()
                    .FirstOrDefault(e => e.Name.Equals(filter.FieldName, StringComparison.OrdinalIgnoreCase));
                if (fieldInfo != null)
                {
                    memberExpression = Expression.MakeMemberAccess(parameterExpression, fieldInfo);
                    propType = fieldInfo.FieldType;
                }
            }

            if (memberExpression != null)
            {
                Expression body = null;
                var valueExpression = Expression.Convert(Expression.Constant(filter.Value), propType);
                switch (filter.Operator)
                {
                    case DynamicFilterOperator.Equal:
                        body = Expression.Equal(memberExpression, valueExpression);
                        break;
                    case DynamicFilterOperator.NotEqual:
                        body = Expression.NotEqual(memberExpression, valueExpression);
                        break;
                    case DynamicFilterOperator.GreaterThan:
                        body = Expression.GreaterThan(memberExpression, valueExpression);
                        break;
                    case DynamicFilterOperator.GreaterThanOrEqual:
                        body = Expression.GreaterThanOrEqual(memberExpression, valueExpression);
                        break;
                    case DynamicFilterOperator.LessThan:
                        body = Expression.LessThan(memberExpression, valueExpression);
                        break;
                    case DynamicFilterOperator.LessThanOrEqual:
                        body = Expression.LessThanOrEqual(memberExpression, valueExpression);
                        break;
                    case DynamicFilterOperator.Contains:
                        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        if (containsMethod != null)
                        {
                            body = Expression.Call(memberExpression, containsMethod, valueExpression);
                        }

                        break;
                }

                if (body != null)
                {
                    if (finalBody == null)
                    {
                        finalBody = body;
                    }
                    else
                    {
                        finalBody = Expression.OrElse(finalBody, body);
                    }
                }
            }
        }

        if (finalBody != null)
        {
            var finalExpression = Expression.Lambda<Func<T, bool>>(finalBody, parameterExpression);
            query = query.Where(finalExpression);
        }

        return query;
    }


    public static IQueryable<T> WhereWhile<T>(this IQueryable<T> query, bool predicate,
        Expression<Func<T, bool>> expression) where T : class
    {
        if (predicate) return query.Where(expression);
        return query;
    }

    public static async Task MergeAsync<T>(this DbSet<T> db, IQueryable<T> existedEntitiesQueryable,
        IEnumerable<T>? mergingObjects, Func<T, T, bool> filterToUpdate, Action<T, T>? updateAction,
        CancellationToken cancellationToken) where T : class
    {
        var existedEntities = await existedEntitiesQueryable.ToListAsync(cancellationToken);
        var addingEntities = new List<T>();
        var updatingEntities = new List<T>();
        if (mergingObjects != null)
        {
            foreach (var mergingObject in mergingObjects)
            {
                var existingEntity = existedEntities.FirstOrDefault(e => filterToUpdate(e, mergingObject));
                if (existingEntity != null)
                {
                    updateAction?.Invoke(existingEntity, mergingObject);
                    updatingEntities.Add(existingEntity);
                }
                else
                {
                    addingEntities.Add(mergingObject);
                }
            }
        }

        var deletingEntities = existedEntities.Where(e => !updatingEntities.Any(x => filterToUpdate(e, x))).ToList();
        if (deletingEntities.Any()) db.RemoveRange(deletingEntities);
        if (addingEntities.Any()) await db.AddRangeAsync(addingEntities, cancellationToken);
    }

    public static async Task MergeAsync<T>(this DbSet<T> db, Expression<Func<T, bool>> existedEntitiesFilter,
        IEnumerable<T>? mergingObjects, Func<T, T, bool> filterToUpdate, Action<T, T>? updateAction,
        CancellationToken cancellationToken) where T : class
    {
        await db.MergeAsync(db.Where(existedEntitiesFilter), mergingObjects, filterToUpdate, updateAction,
            cancellationToken);
    }
}

public class PaginatedResult<T>
{
    public List<T> List { get; set; }

    public int Total { get; set; }
}