using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace EchoTrace.Infrastructure.JwtFunction;

public class JwtService
{
    public const string TokenTypeConst = "tokenType";
    private readonly JwtSetting _jwtSetting;

    public JwtService(JwtSetting jwtSetting)
    {
        _jwtSetting = jwtSetting;
    }

    /// <summary>
    ///     生成刷新Token
    /// </summary>
    /// <returns></returns>
    public TokenResult GenerateRefreshJwtToken(string accessToken)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(accessToken));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(TokenTypeConst, TokenTypeEnum.RefreshToken.ToString()),
        };
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtSetting.Issuer,
            Audience = _jwtSetting.Audience,
            Expires = DateTime.UtcNow.AddMinutes(_jwtSetting.LongExpiresInMinutes),
            NotBefore = DateTime.UtcNow,
            SigningCredentials = credentials,
            Subject = new ClaimsIdentity(claims)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var refreshToken = tokenHandler.WriteToken(token);
        return new TokenResult()
        {
            Token = refreshToken,
            ExpireAt = _jwtSetting.LongExpiresInMinutes,
            TokenType = TokenTypeEnum.RefreshToken.ToString()
        };
    }

    /// <summary>
    ///     生成访问令牌
    /// </summary>
    /// <param name="sid"></param>
    /// <returns></returns>
    public TokenResult GenerateAccessJwtToken(string sid)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.SignKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(TokenTypeConst, TokenTypeEnum.AccessToken.ToString()),
            new Claim(ClaimTypes.NameIdentifier, sid)
        };
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtSetting.Issuer,
            Audience = _jwtSetting.Audience,
            Expires = DateTime.UtcNow.AddMinutes(_jwtSetting.ShortExpiresInMinutes),
            NotBefore = DateTime.UtcNow,
            SigningCredentials = credentials,
            Subject = new ClaimsIdentity(claims)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(token);
        return new TokenResult
        {
            Token = accessToken,
            ExpireAt = _jwtSetting.ShortExpiresInMinutes,
            TokenType = TokenTypeEnum.AccessToken.ToString()
        };
    }

    /// <summary>
    ///     验证token
    /// </summary>
    /// <param name="token">token</param>
    /// <param name="validateLifetime">是否验证过期时间</param>
    /// <returns></returns>
    public Task<TokenValidationResult> ValidateAccessTokenAsync(string token, bool validateLifetime = true)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        TokenValidationParameters tokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = validateLifetime,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSetting.Issuer,
            ValidAudience = _jwtSetting.Audience,
            RequireExpirationTime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.SignKey))
        };
        return tokenHandler.ValidateTokenAsync(token, tokenValidationParameters);
    }

    /// <summary>
    ///     验证RefreshToken
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <param name="accessToken"></param>
    /// <param name="validateLifetime"></param>
    /// <returns></returns>
    public Task<TokenValidationResult> ValidateRefreshTokenAsync(string refreshToken, string accessToken,
        bool validateLifetime = true)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        TokenValidationParameters tokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = validateLifetime,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSetting!.Issuer,
            ValidAudience = _jwtSetting.Audience,
            RequireExpirationTime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(accessToken))
        };
        return tokenHandler.ValidateTokenAsync(refreshToken, tokenValidationParameters);
    }

    /// <summary>
    ///     凭证是不是Bearer
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    /// <returns></returns>
    public static bool AuthenticationIsBearerScheme(IHttpContextAccessor httpContextAccessor)
    {
        var authenticationValue = GetAuthentication(httpContextAccessor);
        return authenticationValue != null &&
               "Bearer".Equals(authenticationValue.Scheme, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     获取验证信息
    /// </summary>
    /// <returns></returns>
    private static AuthenticationHeaderValue? GetAuthentication(IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor is { HttpContext: not null })
        {
            var authorization = httpContextAccessor.HttpContext.Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(authorization)) return AuthenticationHeaderValue.Parse(authorization);
        }

        return null;
    }
}

/// <summary>
///     Token的类型
/// </summary>
public enum TokenTypeEnum
{
    AccessToken,
    RefreshToken
}

public class TokenResult
{
    public string Token { get; set; }
    public string TokenType { get; set; }
    public int ExpireAt { get; set; }
}