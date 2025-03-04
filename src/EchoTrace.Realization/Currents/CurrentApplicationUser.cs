using System.Security.Claims;
using EchoTrace.Infrastructure.DataPersistence.EfCore;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities;
using EchoTrace.Primary.Bases;
using EchoTrace.Realization.Bases;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EchoTrace.Realization.Currents;

[AsType(LifetimeEnum.Scope, typeof(ICurrentSingle<ApplicationUser>))]
public class CurrentApplicationUser : ICurrentSingle<ApplicationUser>
{
    private readonly DbAccessor<ApplicationUser> _applicationUsers;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentApplicationUser(IHttpContextAccessor httpContextAccessor,DbAccessor<ApplicationUser> applicationUsers)
    {
        _httpContextAccessor = httpContextAccessor;
        _applicationUsers = applicationUsers;
    }

    public async Task<ApplicationUser> QueryAsync(CancellationToken cancellationToken = default)
    {
        var id = await GetCurrentUserIdAsync();
        var user = await _applicationUsers.DbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user != null)
        {
            return user;
        }

        throw new BusinessException("當前用戶未登錄或登錄已過期", BusinessExceptionTypeEnum.DataNotExists);
    }

    public async Task<Guid> GetCurrentUserIdAsync()
    {
        var id = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(id))
        {
            if (Guid.TryParse(id, out var guid))
            {   
                return await Task.FromResult(guid);
            }
        }

        throw new BusinessException("用戶未登錄，請先登錄", BusinessExceptionTypeEnum.UnauthorizedIdentity);
    }
}