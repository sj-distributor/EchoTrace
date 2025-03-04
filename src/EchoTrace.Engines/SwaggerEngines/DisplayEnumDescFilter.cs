using System.ComponentModel;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EchoTrace.Engines.SwaggerEngines;

public class DisplayEnumDescFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        if (type.IsEnum)
        {
            var descriptions = GetTypeDescriptions(type);
            if (descriptions.Any())
                schema.Description = string.Join("  ", descriptions.Select(e => $"{e.Key}.{e.Value}"));
        }
    }

    private static List<KeyValuePair<string, string>> GetTypeDescriptions(Type type)
    {
        List<KeyValuePair<string, string>> descriptions = new();
        var fieldInfos = type.GetFields().Where(e => e.FieldType == type);
        foreach (var field in fieldInfos)
        {
            var descType = field.GetCustomAttribute<DescriptionAttribute>();
            var key = field.GetRawConstantValue();
            if (descType != null)
                descriptions.Add(new KeyValuePair<string, string>(Convert.ToString(key!)!, descType.Description));
            else
                descriptions.Add(new KeyValuePair<string, string>(Convert.ToString(key!)!, "--"));
        }

        return descriptions;
    }
}