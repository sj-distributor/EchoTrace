using System.Net.Http.Headers;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities;
using EchoTrace.Infrastructure.JwtFunction;
using Hangfire.Dashboard;
using HangfireBasicAuthenticationFilter;
using JobHub.Infrastructure.Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace EchoTrace.Infrastructure.Hangfire.HangfireFilters;

public class HangfireDashboardAuthorizationFilter(
    IServiceProvider serviceProvider, HangfireSettings hangfireSettings) : IDashboardAsyncAuthorizationFilter
{
    private const string AuthenticationScheme = "Basic";
    
    private readonly TimeSpan _loginTimeout = TimeSpan.FromMinutes(hangfireSettings.JobMemoryCacheTimeout);

    public async Task<bool> AuthorizeAsync(DashboardContext context)
    {
        var jobManager = context.GetRecurringJobManager();
        var lifetimeScope = serviceProvider.GetAutofacRoot();
        var httpContext = context.GetHttpContext();
        var header = httpContext.Request.Headers["Authorization"];
        
        if (Missing_Authorization_Header(header))
        {
            SetChallengeResponse(httpContext);
            return false;
        }

        var authValues = AuthenticationHeaderValue.Parse(header!);
        if (Not_Basic_Authentication(authValues))
        {
            SetChallengeResponse(httpContext);
            return false;
        }

        var authenticationTokens = Extract_Authentication_Tokens(authValues);
        if (authenticationTokens.Are_Invalid())
        {
            SetChallengeResponse(httpContext);
            return false;
        }

        var applicationUserDbSet = lifetimeScope.Resolve<DbSet<ApplicationUser>>();
        var passwordHasher = lifetimeScope.Resolve<IPasswordHasher>();
        var memoryCache = lifetimeScope.Resolve<IMemoryCache>();
        var user = applicationUserDbSet.AsNoTracking().FirstOrDefault(x => x.UserName == authenticationTokens.Username);
        if (user == null)
        {
            SetChallengeResponse(httpContext);
            return false;
        }

        if (!passwordHasher.VerifyHashedPasswordV3(user.PasswordHash, authenticationTokens.Password))
        {
            SetChallengeResponse(httpContext);
            return false;
        }

        if (passwordHasher.VerifyHashedPasswordV3(user.PasswordHash, authenticationTokens.Password))
        {
            var cacheKey = $"{AuthenticationScheme} + {user.Id}";
            var useCache = memoryCache.Get<HangfireAuthenticationUser>(cacheKey);
            if (useCache == null)
            {
                var hangfireAuthenticationUser = new HangfireAuthenticationUser
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    LastLoginTime = DateTime.UtcNow
                };
                memoryCache.Set(cacheKey, hangfireAuthenticationUser, _loginTimeout);
                return false;
            }
            return true;
        }
        
        SetChallengeResponse(httpContext);
        return false;
    }

    private static bool Missing_Authorization_Header(StringValues header)
    {
        return string.IsNullOrWhiteSpace((string)header!);
    }

    private static BasicAuthenticationTokens Extract_Authentication_Tokens(
        AuthenticationHeaderValue authValues)
    {
        return new BasicAuthenticationTokens(Encoding.UTF8.GetString(Convert.FromBase64String(authValues.Parameter!))
            .Split(':'));
    }

    private static bool Not_Basic_Authentication(AuthenticationHeaderValue authValues)
    {
        return !AuthenticationScheme.Equals(authValues.Scheme, StringComparison.InvariantCultureIgnoreCase);
    }

    private void SetChallengeResponse(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = 401;
        httpContext.Response.Headers.Append("WWW-Authenticate",
            (StringValues)$"{AuthenticationScheme} realm=\"Hangfire Dashboard\"");
        httpContext.Response.WriteAsync("Authentication is required.");
    }

    private class HangfireAuthenticationUser
    {
        public Guid Id { get; set; }

        public string UserName { get; set; }

        public DateTime LastLoginTime { get; set; }
    }
}