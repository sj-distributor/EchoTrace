using EchoTrace.Realization.Bases;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Serilog;

namespace EchoTrace.Engines.ConventionEngines;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        context.Response.ContentType = "application/json";
        if (exception is BusinessException businessException)
        {
            if (businessException.Type == BusinessExceptionTypeEnum.UnauthorizedIdentity)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            }
            else if (businessException.Type == BusinessExceptionTypeEnum.Forbidden)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }

            await context.Response.WriteAsJsonAsync(new ExceptionResponseDetailModel
            {
                Type = businessException.Name,
                StatusCode = context.Response.StatusCode,
                Error = businessException.TypeName,
                Message = businessException.Message
            }, cancellationToken);
            Log.Information("已預知的業務異常，響應信息：異常名-{ExceptionName},Code-{StatusCode},錯誤類型-{ErrorType},異常信息-{Message}",
                businessException.Name ?? nameof(BusinessException), context.Response.StatusCode,
                businessException.TypeName, businessException.Message);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new ExceptionResponseDetailModel
            {
                Type = "SystemException",
                StatusCode = StatusCodes.Status500InternalServerError,
                Error = "程序異常",
                Message = "出現了程序預料之外的錯誤，請聯繫管理員"
            }, cancellationToken);
            Log.Error("預料之外的程序異常，異常名-{ExceptionName},異常詳情：{Exception}", "SystemException",
                JsonConvert.SerializeObject(exception));
        }

        return true;
    }
}

public class ExceptionResponseDetailModel
{
    /// <summary>
    /// 異常信息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 異常屬於哪類錯誤
    /// </summary>
    public string Error { get; set; }

    /// <summary>
    /// htttp狀態碼
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// 具體的異常名稱
    /// </summary>
    public string? Type { get; set; }
}