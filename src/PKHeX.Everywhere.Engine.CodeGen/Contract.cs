using System.Reflection;

namespace PKHeX.Everywhere.Engine.CodeGen;

public record Parameter(string Name, Type Type, NullabilityInfo Nullability);

public record Call(string Name, IReadOnlyList<Parameter> Parameters, Type ReturnType, NullabilityInfo ReturnNullability);

public record Contract(IReadOnlyList<Call> Calls, IReadOnlyList<string> ErrorCodes)
{
    private const string Namespace = "PKHeX.Everywhere.Engine";
    private static readonly string[] InjectedTypes = ["PKHeX.Facade.Game", $"{Namespace}.Session"];

    public static Contract Read(Assembly engine)
    {
        var nullability = new NullabilityInfoContext();

        var calls = engine.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            .Select(m => (Method: m, Name: QueryName(m)))
            .Where(m => m.Name is not null)
            .Select(m => new Call(
                m.Name!,
                m.Method.GetParameters()
                    .Where(p => !InjectedTypes.Contains(p.ParameterType.FullName))
                    .Select(p => new Parameter(p.Name!, p.ParameterType, nullability.Create(p)))
                    .ToList(),
                m.Method.ReturnType,
                nullability.Create(m.Method.ReturnParameter)))
            .OrderBy(c => c.Name, StringComparer.Ordinal)
            .ToList();

        var errorCodes = engine.GetType($"{Namespace}.ErrorCodes", throwOnError: true)!
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral)
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToList();

        return new Contract(calls, errorCodes);
    }

    private static string? QueryName(MethodInfo method) => method.CustomAttributes
        .FirstOrDefault(a => a.AttributeType.FullName == $"{Namespace}.QueryAttribute")
        ?.ConstructorArguments[0].Value as string;

    public static bool IsBranded(Type type) =>
        type.CustomAttributes.Any(a => a.AttributeType.FullName == $"{Namespace}.BrandedAttribute");

    public static bool IsObject(Type type) =>
        (type.IsClass || (type.IsValueType && !type.IsPrimitive && !type.IsEnum)) &&
        !type.IsGenericType &&
        type.Namespace?.StartsWith(Namespace) == true;
}
