using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;

namespace EchoTrace.FilterAndMiddlewares;

public class HandleTimezoneResultFilter : IResultFilter
{
    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    public void OnResultExecuting(ResultExecutingContext context)
    {
        var timezone = context.HttpContext.Request.Headers["x-tz"].FirstOrDefault();
        if (timezone != null && context.Result is ObjectResult { Value: not null } objectResult)
        {
            var newResult = ConvertToSpecificTimezone(objectResult.Value, timezone);
            if (newResult != null) objectResult.Value = newResult;
        }
    }

    private object? ConvertToSpecificTimezone(object value, string timezone)
    {
        if (int.TryParse(timezone, out var timezoneOffset))
        {
            var name = $"utc{timezoneOffset / 60.0:+#;-#;0}";
            var timeZoneInfo =
                TimeZoneInfo.CreateCustomTimeZone(name, TimeSpan.FromMinutes(timezoneOffset), name, name);
            var jToken = JToken.FromObject(value!);
            UpdateDateTimeToSpecificTimezone(jToken, timeZoneInfo);
            return jToken.ToObject(value!.GetType());
        }

        return null;
    }

    private void UpdateDateTimeToSpecificTimezone(JToken token, TimeZoneInfo timeZoneInfo)
    {
        switch (token.Type)
        {
            case JTokenType.Object:
            {
                var propertyList = token.Children();
                foreach (var property in propertyList) UpdateDateTimeToSpecificTimezone(property, timeZoneInfo);

                break;
            }
            case JTokenType.Array:
            {
                var propertyList = token.Children();
                foreach (var item in propertyList) UpdateDateTimeToSpecificTimezone(item, timeZoneInfo);

                break;
            }
            case JTokenType.Property:
            {
                if (token is JProperty prop) UpdateDateTimeToSpecificTimezone(prop.Value, timeZoneInfo);

                break;
            }
            case JTokenType.Date:
            {
                if (token is JValue tokenValue)
                    tokenValue.Value = TimeZoneInfo.ConvertTime(token.ToObject<DateTimeOffset>(), timeZoneInfo);

                break;
            }
        }
    }
}