namespace PKHeX.Everywhere.Engine;

[AttributeUsage(AttributeTargets.Method)]
public sealed class QueryAttribute(string name, params string[] topics) : Attribute
{
    public string Name { get; } = name;
    public string[] Topics { get; } = topics;
}

/// <summary>
/// Marks a call that writes to the save. It writes the Topics of its <see cref="IHandle"/> arguments plus the Topics listed here.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class CommandAttribute(string name, params string[] topics) : Attribute
{
    public string Name { get; } = name;
    public string[] Topics { get; } = topics;
}

/// <summary>
/// Names the generated entity hook of a handler class when it shouldn't be <c>use{Entity}</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class EntityHookAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

/// <summary>
/// Marks a record with a single string <c>Value</c>. It crosses the boundary as a plain string and becomes a branded string type in TypeScript.
/// </summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class BrandedAttribute : Attribute;

/// <summary>
/// Locates an entity in the save. Commands taking a handle write its Topic, and item hooks bind it.
/// </summary>
public interface IHandle
{
    string Topic();
}
