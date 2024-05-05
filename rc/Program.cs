using Rhino.CodeAnalysis;
using Rhino.CodeAnalysis.Symbols;
using Rhino.CodeAnalysis.Syntax;
using Rhino.IO;

namespace rc;

internal class Program {
    private static int Main(string[] args) {
        if (args.Length == 0) {
            Console.WriteLine("Usage: rc <filename>");
            return 1;
        }

        var paths = GetFilePaths(args);
        var syntaxTrees = new List<SyntaxTree>();
        var hasErrors = false;

        foreach (var path in paths) {
            if (!File.Exists(path)) {
                Console.Error.WriteLine($"ERROR: file {path} does not exist.");
                hasErrors = true;

                continue;
            }

            syntaxTrees.Add(SyntaxTree.Load(path));
        }

        if (hasErrors) return 1;

        var compilation = new Compilation(syntaxTrees.ToArray());
        var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

        if (result.Diagnostics.Any()) {
            Console.Error.WriteDiagnostics(result.Diagnostics);
            return 1;
        }
        
        return 0;
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