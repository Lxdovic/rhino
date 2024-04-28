using Rhino.CodeAnalysis;
using Rhino.CodeAnalysis.Symbols;
using Rhino.CodeAnalysis.Syntax;
using Rhino.IO;

namespace rc;

internal class Program {
    private static void Main(string[] args) {
        if (args.Length == 0) {
            Console.WriteLine("Usage: rc <filename>");
            return;
        }

        var paths = GetFilePaths(args);
        var syntaxTrees = new List<SyntaxTree>();
        var hasErrors = false;

        foreach (var path in paths) {
            if (!File.Exists(path)) {
                Console.WriteLine($"ERROR: file {path} does not exist.");
                hasErrors = true;

                continue;
            }

            syntaxTrees.Add(SyntaxTree.Load(path));
        }

        if (hasErrors) return;

        var compilation = new Compilation(syntaxTrees.ToArray());
        var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

        if (result.Diagnostics.Any()) Console.Error.WriteDiagnostics(result.Diagnostics);
        else if (result.Value is not null) Console.WriteLine(result.Value);
    }

    private static IEnumerable<string?> GetFilePaths(IEnumerable<string> args) {
        var result = new SortedSet<string>();

        foreach (var path in args)
            if (Directory.Exists(path))
                result.UnionWith(Directory.EnumerateFiles(path, "*.ri", SearchOption.AllDirectories));
            else
                result.Add(path);

        return result;
    }
}