using System.Reflection;

namespace PKHeX.Everywhere.Engine.CodeGen;

public enum CallKind
{
    Query,
    Command,
}

public record Parameter(string Name, Type Type, NullabilityInfo Nullability)
{
    public bool IsHandle => Contract.IsHandle(Type);
}

public record Call(
    string Name,
    CallKind Kind,
    string? Hook,
    IReadOnlyList<string> Topics,
    IReadOnlyList<Parameter> Parameters,
    Type ReturnType,
    NullabilityInfo ReturnNullability)
{
    public string Entity => Name[..Name.IndexOf('.')];
    public string Verb => Name[(Name.IndexOf('.') + 1)..];
}

public record Contract(IReadOnlyList<Call> Calls, IReadOnlyList<string> ErrorCodes, IReadOnlyList<string> Topics)
{
    private const string Namespace = "PKHeX.Everywhere.Engine";
    private static readonly string[] InjectedTypes = ["PKHeX.Facade.Game", $"{Namespace}.Session"];

    public IEnumerable<Call> Queries => Calls.Where(c => c.Kind == CallKind.Query);

    public static Contract Read(Assembly engine, IEnumerable<Assembly> others)
    {
        var nullability = new NullabilityInfoContext();

        var calls = others.Prepend(engine)
            .SelectMany(a => a.GetTypes())
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            .Select(m => (Method: m, Attribute: m.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName is $"{Namespace}.QueryAttribute" or $"{Namespace}.CommandAttribute")))
            .Where(m => m.Attribute is not null)
            .Select(m => new Call(
                (string)m.Attribute!.ConstructorArguments[0].Value!,
                m.Attribute.AttributeType.Name == "QueryAttribute" ? CallKind.Query : CallKind.Command,
                Hook(m.Method.DeclaringType!),
                DeclaredTopics(m.Attribute),
                m.Method.GetParameters()
                    .Where(p => !InjectedTypes.Contains(p.ParameterType.FullName))
                    .Select(p => new Parameter(p.Name!, p.ParameterType, nullability.Create(p)))
                    .ToList(),
                m.Method.ReturnType,
                nullability.Create(m.Method.ReturnParameter)))
            .OrderBy(c => c.Name, StringComparer.Ordinal)
            .ToList();

        if (calls.GroupBy(c => c.Name).FirstOrDefault(g => g.Count() > 1) is { } duplicate)
            throw new InvalidOperationException($"Call '{duplicate.Key}' is declared more than once.");

        var topics = Constants(engine, "Topics");
        foreach (var call in calls)
        {
            if (!call.Name.Contains('.'))
                throw new InvalidOperationException($"Call '{call.Name}' must be named 'entity.verb'.");
            if (call.Kind == CallKind.Query && call.Topics.Count == 0)
                throw new InvalidOperationException($"Query '{call.Name}' must declare the topics it reads.");
            if (call.Kind == CallKind.Command && call.Topics.Count == 0 && !call.Parameters.Any(p => p.IsHandle))
                throw new InvalidOperationException($"Command '{call.Name}' must take a handle or declare the topics it writes.");
            if (call.Topics.FirstOrDefault(t => !topics.Contains(t)) is { } unknown)
                throw new InvalidOperationException($"Call '{call.Name}' uses '{unknown}', which is not declared in Topics.");
        }

        return new Contract(calls, Constants(engine, "ErrorCodes"), topics);
    }

    private static List<string> Constants(Assembly engine, string type) => engine.GetType($"{Namespace}.{type}", throwOnError: true)!
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.IsLiteral)
        .Select(f => (string)f.GetRawConstantValue()!)
        .ToList();

    private static List<string> DeclaredTopics(CustomAttributeData attribute) =>
        ((IEnumerable<CustomAttributeTypedArgument>)attribute.ConstructorArguments[1].Value!)
            .Select(a => (string)a.Value!)
            .ToList();

    private static string? Hook(Type handlers) =>
        handlers.CustomAttributes
            .FirstOrDefault(a => a.AttributeType.FullName == $"{Namespace}.EntityHookAttribute")
            ?.ConstructorArguments[0].Value as string;

    public static bool IsBranded(Type type) =>
        type.CustomAttributes.Any(a => a.AttributeType.FullName == $"{Namespace}.BrandedAttribute");

    public static bool IsHandle(Type type) =>
        type.GetInterfaces().Any(i => i.FullName == $"{Namespace}.IHandle");

    public static bool IsObject(Type type) =>
        (type.IsClass || (type.IsValueType && !type.IsPrimitive && !type.IsEnum)) &&
        !type.IsGenericType &&
        (type.Namespace == Namespace || type.Namespace?.StartsWith(Namespace + ".") == true);
}
