using System.Reflection;
using System.Runtime.Loader;
using PKHeX.Everywhere.Engine.CodeGen;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: PKHeX.Everywhere.Engine.CodeGen <engine assembly> <output directory>");
    return 1;
}

var enginePath = Path.GetFullPath(args[0]);
var outputDirectory = Path.GetFullPath(args[1]);
var engineDirectory = Path.GetDirectoryName(enginePath)!;

var context = new AssemblyLoadContext("engine", isCollectible: true);
context.Resolving += (alc, name) =>
{
    var candidate = Path.Combine(engineDirectory, $"{name.Name}.dll");
    return File.Exists(candidate) ? alc.LoadFromAssemblyPath(candidate) : null;
};

var engine = context.LoadFromAssemblyPath(enginePath);
var contract = Contract.Read(engine);

Directory.CreateDirectory(outputDirectory);
foreach (var (file, generated) in TypeScript.Write(contract))
{
    var content = generated.ReplaceLineEndings("\n");
    var path = Path.Combine(outputDirectory, file);
    if (File.Exists(path) && File.ReadAllText(path) == content) continue;

    File.WriteAllText(path, content);
    Console.WriteLine($"Engine CodeGen: wrote {Path.GetRelativePath(Environment.CurrentDirectory, path)}");
}

return 0;
