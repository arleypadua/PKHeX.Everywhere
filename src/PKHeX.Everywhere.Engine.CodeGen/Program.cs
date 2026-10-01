using System.Reflection;
using System.Runtime.Loader;
using PKHeX.Everywhere.Engine.CodeGen;

if (args.Length < 2)
{
    Console.Error.WriteLine("Usage: PKHeX.Everywhere.Engine.CodeGen <SDK packages directory> <engine assembly> [<handler assembly>...]");
    return 1;
}

var outputDirectory = Path.GetFullPath(args[0]);
var assemblyPaths = args[1..].Select(Path.GetFullPath).ToList();

var context = new AssemblyLoadContext("engine", isCollectible: true);
context.Resolving += (alc, name) => assemblyPaths
    .Select(path => Path.Combine(Path.GetDirectoryName(path)!, $"{name.Name}.dll"))
    .Where(File.Exists)
    .Select(alc.LoadFromAssemblyPath)
    .FirstOrDefault();

var assemblies = assemblyPaths.Select(context.LoadFromAssemblyPath).ToList();
var contract = Contract.Read(assemblies[0], assemblies.Skip(1));

foreach (var (file, generated) in TypeScript.Write(contract))
{
    var content = generated.ReplaceLineEndings("\n");
    var path = Path.Combine(outputDirectory, file);
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    if (File.Exists(path) && File.ReadAllText(path) == content) continue;

    File.WriteAllText(path, content);
    Console.WriteLine($"Engine CodeGen: wrote {Path.GetRelativePath(Environment.CurrentDirectory, path)}");
}

return 0;
