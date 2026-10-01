using System.Reflection;

namespace PKHeX.Everywhere.Engine.CodeGen;

public record Parameter(string Name, Type Type, NullabilityInfo Nullability);

public record Call(string Name, IReadOnlyList<string> Topics, IReadOnlyList<Parameter> Parameters, Type ReturnType, NullabilityInfo ReturnNullability);

public record Contract(IReadOnlyList<Call> Calls, IReadOnlyList<string> ErrorCodes, IReadOnlyList<string> Topics)
{
    private const string Namespace = "PKHeX.Everywhere.Engine";
    private static readonly string[] InjectedTypes = ["PKHeX.Facade.Game", $"{Namespace}.Session"];

    public static Contract Read(Assembly engine)
    {
        var nullability = new NullabilityInfoContext();

        var calls = engine.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            .Select(m => (Method: m, Query: m.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == $"{Namespace}.QueryAttribute")))
            .Where(m => m.Query is not null)
            .Select(m => new Call(
                (string)m.Query!.ConstructorArguments[0].Value!,
                QueryTopics(m.Query),
                m.Method.GetParameters()
                    .Where(p => !InjectedTypes.Contains(p.ParameterType.FullName))
                    .Select(p => new Parameter(p.Name!, p.ParameterType, nullability.Create(p)))
                    .ToList(),
                m.Method.ReturnType,
                nullability.Create(m.Method.ReturnParameter)))
            .OrderBy(c => c.Name, StringComparer.Ordinal)
            .ToList();

        var topics = Constants(engine, "Topics");
        foreach (var call in calls)
        {
            if (call.Topics.Count == 0)
                throw new InvalidOperationException($"Query '{call.Name}' must declare the topics it reads.");
            if (call.Topics.FirstOrDefault(t => !topics.Contains(t)) is { } unknown)
                throw new InvalidOperationException($"Query '{call.Name}' reads '{unknown}', which is not declared in Topics.");
        }

        return new Contract(calls, Constants(engine, "ErrorCodes"), topics);
    }

    private static List<string> Constants(Assembly engine, string type) => engine.GetType($"{Namespace}.{type}", throwOnError: true)!
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.IsLiteral)
        .Select(f => (string)f.GetRawConstantValue()!)
        .ToList();

    private static List<string> QueryTopics(CustomAttributeData query) =>
        ((IEnumerable<CustomAttributeTypedArgument>)query.ConstructorArguments[1].Value!)
            .Select(a => (string)a.Value!)
            .ToList();

    public static bool IsBranded(Type type) =>
        type.CustomAttributes.Any(a => a.AttributeType.FullName == $"{Namespace}.BrandedAttribute");

    public static bool IsObject(Type type) =>
        (type.IsClass || (type.IsValueType && !type.IsPrimitive && !type.IsEnum)) &&
        !type.IsGenericType &&
        (type.Namespace == Namespace || type.Namespace?.StartsWith(Namespace + ".") == true);
}
