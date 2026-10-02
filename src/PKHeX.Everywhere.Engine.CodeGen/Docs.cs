using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace PKHeX.Everywhere.Engine.CodeGen;

/// <summary>
/// Reads the XML documentation the compiler writes next to each engine assembly and turns it into TSDoc.
/// </summary>
public sealed partial class Docs
{
    private readonly Dictionary<Assembly, Dictionary<string, XElement>> _members = new();

    public string? Comment(Type type, string indent = "") => Format(Summary(type), indent);

    public string? Comment(PropertyInfo property, string indent = "")
    {
        var type = property.DeclaringType!;
        var text = Text(Member(type.Assembly, $"P:{Id(type)}.{property.Name}")?.Element("summary"))
                   // A positional record documents its properties as <param> tags on the type.
                   ?? Text(Member(type.Assembly, $"T:{Id(type)}")?.Elements("param").FirstOrDefault(p => (string?)p.Attribute("name") == property.Name));
        return Format(text, indent);
    }

    private string? Summary(Type type) => Text(Member(type.Assembly, $"T:{Id(type)}")?.Element("summary"));

    private XElement? Member(Assembly assembly, string id)
    {
        if (!_members.TryGetValue(assembly, out var members))
        {
            var path = Path.ChangeExtension(assembly.Location, ".xml");
            members = File.Exists(path)
                ? XDocument.Load(path).Descendants("member").ToDictionary(m => (string)m.Attribute("name")!, m => m)
                : [];
            _members[assembly] = members;
        }

        return members.GetValueOrDefault(id);
    }

    private static string Id(Type type) => type.FullName!.Replace('+', '.');

    private static string? Text(XElement? element)
    {
        if (element is null) return null;

        var text = string.Concat(element.Nodes().Select(node => node switch
        {
            XText t => t.Value,
            XElement { Name.LocalName: "c" } c => $"`{c.Value}`",
            XElement { Name.LocalName: "see" or "paramref" or "typeparamref" } see => $"`{Reference(see)}`",
            XElement other => other.Value,
            _ => "",
        }));

        text = Whitespace().Replace(text, " ").Trim();
        return text == "" ? null : text;
    }

    // Properties and record parameters reach TypeScript in camelCase, types keep their name.
    private static string Reference(XElement see)
    {
        var cref = (string?)see.Attribute("cref");
        var name = ((string?)see.Attribute("name") ?? cref ?? (string?)see.Attribute("langword") ?? "").Split('.', ':').Last();
        var isMember = see.Name.LocalName == "paramref" || cref?.StartsWith("P:") == true;
        return isMember ? JsonNamingPolicy.CamelCase.ConvertName(name) : name;
    }

    private static string? Format(string? text, string indent) =>
        text is null ? null : $"{indent}/** {text.Replace("*/", "*\\/")} */\n";

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
