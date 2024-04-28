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

        if (args.Length > 1) {
            Console.WriteLine("Error: only one path supported tight now");
            return;
        }

        var path = args.Single();
        var text = File.ReadAllText(path);
        var syntaxTree = SyntaxTree.Parse(text);
        var compilation = new Compilation(syntaxTree);
        var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

        if (result.Diagnostics.Any()) Console.Error.WriteDiagnostics(result.Diagnostics, syntaxTree);
        else if (result.Value is not null) Console.WriteLine(result.Value);
    }
}