using System.Reflection;

namespace PKHeX.Everywhere.Engine.CodeGen;

public enum CallKind
{
    Query,
    Command,
}

public record Parameter(string Name, Type Type, NullabilityInfo Nullability, bool IsOptional)
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
    NullabilityInfo ReturnNullability,
    bool RequiresSave,
    bool RequiresDraft,
    MethodInfo Method)
{
    public string Entity => Name[..Name.IndexOf('.')];
    public string Verb => Name[(Name.IndexOf('.') + 1)..];
}

public record Contract(IReadOnlyList<Call> Calls, IReadOnlyList<Type> Events, IReadOnlyList<string> ErrorCodes, IReadOnlyList<string> Topics)
{
    private const string Namespace = "PKHeX.Everywhere.Engine";
    private const string GameType = "PKHeX.Facade.Game";
    private static readonly string[] InjectedTypes = [GameType, $"{Namespace}.Session"];

    public IEnumerable<Call> Queries => Calls.Where(c => c.Kind == CallKind.Query);

    public static Contract Read(Assembly engine, IEnumerable<Assembly> handlerAssemblies)
    {
        var nullability = new NullabilityInfoContext();

        var assemblies = handlerAssemblies.Prepend(engine).ToList();
        var declared = assemblies
            .SelectMany(a => a.GetTypes())
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            .Select(m => (Method: m, Attribute: m.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName is $"{Namespace}.QueryAttribute" or $"{Namespace}.CommandAttribute")))
            .Where(m => m.Attribute is not null)
            .ToList();

        if (declared.FirstOrDefault(m => m.Attribute!.AttributeType.Name == "QueryAttribute" && DeclaredTopics(m.Attribute).Count == 0 && ReadsSave(m.Method)).Attribute is { } query)
            throw new InvalidOperationException($"Query '{query.ConstructorArguments[0].Value}' reads the save, so it must declare the topics it reads.");

        var calls = declared
            .Select(m => (m.Method, m.Attribute, Requires: Requirements(m.Method)))
            .Select(m => new Call(
                (string)m.Attribute!.ConstructorArguments[0].Value!,
                m.Attribute.AttributeType.Name == "QueryAttribute" ? CallKind.Query : CallKind.Command,
                Hook(m.Method.DeclaringType!),
                DeclaredTopics(m.Attribute),
                m.Method.GetParameters()
                    .Where(p => !InjectedTypes.Contains(p.ParameterType.FullName))
                    .Select(p => new Parameter(p.Name!, p.ParameterType, nullability.Create(p), p.HasDefaultValue))
                    .ToList(),
                Awaited(m.Method.ReturnType),
                Awaited(nullability.Create(m.Method.ReturnParameter)),
                m.Method.GetParameters().Any(p => p.ParameterType.FullName == GameType) || m.Requires.Contains("Save"),
                m.Requires.Contains("Draft"),
                m.Method))
            .OrderBy(c => c.Name, StringComparer.Ordinal)
            .ToList();

        if (calls.GroupBy(c => c.Name).FirstOrDefault(g => g.Count() > 1) is { } duplicate)
            throw new InvalidOperationException($"Call '{duplicate.Key}' is declared more than once.");

        var topics = Constants(engine, "Topics");
        foreach (var call in calls)
        {
            if (!call.Name.Contains('.'))
                throw new InvalidOperationException($"Call '{call.Name}' must be named 'entity.verb'.");
            if (call.Topics.FirstOrDefault(t => !topics.Contains(t)) is { } unknown)
                throw new InvalidOperationException($"Call '{call.Name}' uses '{unknown}', which is not declared in Topics.");
        }

        var events = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(IsEvent)
            .OrderBy(t => t.Name, StringComparer.Ordinal)
            .ToList();

        return new Contract(calls, events, Constants(engine, "ErrorCodes"), topics);
    }

    private static Type Awaited(Type type) =>
        type == typeof(Task) ? typeof(void) : IsTaskOfT(type) ? type.GenericTypeArguments[0] : type;

    private static NullabilityInfo Awaited(NullabilityInfo info) =>
        IsTaskOfT(info.Type) ? info.GenericTypeArguments[0] : info;

    private static bool IsTaskOfT(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>);

    private static bool ReadsSave(MethodInfo method) =>
        method.GetParameters().Any(p => InjectedTypes.Contains(p.ParameterType.FullName));

    private static List<string> Constants(Assembly engine, string type) => engine.GetType($"{Namespace}.{type}", throwOnError: true)!
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.IsLiteral)
        .Select(f => (string)f.GetRawConstantValue()!)
        .ToList();

    private static List<string> DeclaredTopics(CustomAttributeData attribute) =>
        ((IEnumerable<CustomAttributeTypedArgument>)attribute.ConstructorArguments[1].Value!)
            .Select(a => (string)a.Value!)
            .ToList();

    private static List<string> Requirements(MethodInfo method) => method.CustomAttributes
        .Where(a => a.AttributeType.FullName == $"{Namespace}.RequiresAttribute")
        .SelectMany(a => (IEnumerable<CustomAttributeTypedArgument>)a.ConstructorArguments[0].Value!)
        .Select(a => Enum.GetName(a.ArgumentType, a.Value!)!)
        .ToList();

    private static string? Hook(Type handlers) =>
        handlers.CustomAttributes
            .FirstOrDefault(a => a.AttributeType.FullName == $"{Namespace}.EntityHookAttribute")
            ?.ConstructorArguments[0].Value as string;

    public static bool IsBranded(Type type) =>
        type.CustomAttributes.Any(a => a.AttributeType.FullName == $"{Namespace}.BrandedAttribute");

    public static bool IsEvent(Type type) =>
        type is { IsAbstract: false, IsInterface: false } && type.GetInterfaces().Any(i => i.FullName == $"{Namespace}.IEngineEvent");

    public static bool IsHandle(Type type) =>
        type.GetInterfaces().Any(i => i.FullName == $"{Namespace}.IHandle");

    public static bool IsObject(Type type) =>
        (type.IsClass || (type.IsValueType && !type.IsPrimitive && !type.IsEnum)) &&
        !type.IsGenericType &&
        (type.Namespace == Namespace || type.Namespace?.StartsWith(Namespace + ".") == true);
}
