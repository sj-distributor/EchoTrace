using AutoBogus;
using MongoDB.Bson;

namespace EchoTrace.Realization.Bases;

public sealed class BusinessFaker<T> : AutoFaker<T> where T : class
{
    public BusinessFaker()
    {
        RuleForType(typeof(int), f => f.Random.Int(0, 100));
        RuleForType(typeof(float), f => f.Random.Float(0, 100));
        RuleForType(typeof(double), f => f.Random.Double(0, 100));
        RuleForType(typeof(decimal), f => f.Random.Decimal(0, 100));
        var objectIdProperties = typeof(T).GetProperties()
            .Where(prop => prop.PropertyType == typeof(ObjectId));
        foreach (var prop in objectIdProperties)
        {
            Ignore(prop.Name);
        }
        Configure(option => { option.WithLocale("zh_CN"); });
    }

    public static T Create()
    {
        BusinessFaker<T> businessFaker = new();
        return businessFaker.Generate();
    }

    public static List<T> Create(int number)
    {
        BusinessFaker<T> businessFaker = new();
        return businessFaker.Generate(number);
    }

    public static ValueTask<List<T>> CreateAsync(int number)
    {
        return new ValueTask<List<T>>(Create(number));
    }

    public static ValueTask<T> CreateAsync()
    {
        return new ValueTask<T>(Create());
    }
}