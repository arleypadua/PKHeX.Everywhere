namespace PKHeX.Everywhere.Engine;

[AttributeUsage(AttributeTargets.Method)]
public sealed class QueryAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

/// <summary>
/// Marks a record with a single string <c>Value</c>. It crosses the boundary as a plain string and becomes a branded string type in TypeScript.
/// </summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class BrandedAttribute : Attribute;
