using System.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace EchoTrace.Infrastructure.DataPersistence.EfCore.Bases;

public class TopRowsQueryTranslationPostprocessorFactory : IQueryTranslationPostprocessorFactory
{
    private readonly QueryTranslationPostprocessorDependencies dependencies;
    private readonly RelationalQueryTranslationPostprocessorDependencies relationalDependencies;

    public TopRowsQueryTranslationPostprocessorFactory(QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies)
    {
        this.dependencies = dependencies;
        this.relationalDependencies = relationalDependencies;
    }

    public QueryTranslationPostprocessor Create(QueryCompilationContext queryCompilationContext)
    {
        return new TopRowsQueryTranslationPostprocessor(dependencies, relationalDependencies, queryCompilationContext);
    }
}

public class TopRowsQueryTranslationPostprocessor : RelationalQueryTranslationPostprocessor
{
    private readonly RelationalQueryTranslationPostprocessorDependencies relationalDependencies;

    public TopRowsQueryTranslationPostprocessor(QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext) : base(dependencies, relationalDependencies,
        queryCompilationContext)
    {
        this.relationalDependencies = relationalDependencies;
    }

    public override Expression Process(Expression query)
    {
        query = base.Process(query);
        var intMapping = new IntTypeMapping(SqlDbType.Int.ToString(), DbType.Int32);
        const int limit = 1000;
        if (query is ShapedQueryExpression shaped)
        {
            if (shaped.QueryExpression is SelectExpression select)
                if (select.Limit == null)
                    select.ApplyLimit(relationalDependencies.SqlExpressionFactory.Constant(limit, intMapping));
        }
        else if (query is SelectExpression select1)
        {
            if (select1.Limit == null)
                select1.ApplyLimit(relationalDependencies.SqlExpressionFactory.Constant(limit, intMapping));
        }

        return query;
    }
}