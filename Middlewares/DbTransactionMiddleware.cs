using Microsoft.EntityFrameworkCore;

namespace pingu.Middlewares;

// ASP.NET Core Web API - How to hide DbContext transaction in the middleware pipeline?
// source: https://stackoverflow.com/questions/58225119/asp-net-core-web-api-how-to-hide-dbcontext-transaction-in-the-middleware-pipel/62587685#62587685
public class DbTransactionMiddleware<TDbContext>(RequestDelegate next)
    where TDbContext : DbContext
{
    public async Task Invoke(HttpContext httpContext, TDbContext dbContext)
    {
        var requestMethod = httpContext.Request.Method;

        if (HttpMethods.IsPost(requestMethod) || HttpMethods.IsPut(requestMethod) || HttpMethods.IsDelete(requestMethod))
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync<object, object>(null!, operation: async (_, _, cancellationToken) =>
            {
                // start the transaction
                await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

                // invoke next middleware 
                await next(httpContext);

                // commit the transaction
                await transaction.CommitAsync(cancellationToken);

                return null!;
            }, null);
        }
        else
        {
            await next(httpContext);
        }
    }
}

public static class DbTransactionMiddlewareExtensions
{
    public static IApplicationBuilder UseDbTransaction<TDbContext>(this IApplicationBuilder builder) where TDbContext : DbContext
    {
        return builder.UseMiddleware<DbTransactionMiddleware<TDbContext>>();
    }
}