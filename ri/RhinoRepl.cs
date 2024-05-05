using Rhino.CodeAnalysis;
using Rhino.CodeAnalysis.Symbols;
using Rhino.CodeAnalysis.Syntax;
using Rhino.IO;

namespace Rhino;

internal sealed class RhinoRepl : Repl {
    private readonly Dictionary<VariableSymbol, object> _variables = new();
    private Compilation _previous;
    private bool _showProgram;
    private bool _showTree;

    protected override void RenderLine(string line) {
        var tokens = SyntaxTree.ParseTokens(line);
        foreach (var token in tokens) {
            var isKeyword = token.Kind.ToString().EndsWith("Keyword");
            var isNumber = token.Kind == SyntaxKind.NumberToken;
            var isString = token.Kind == SyntaxKind.StringToken;
            var isIdentifier = token.Kind == SyntaxKind.IdentifierToken;

            if (isKeyword)
                Console.ForegroundColor = ConsoleColor.Blue;
            else if (isNumber)
                Console.ForegroundColor = ConsoleColor.Cyan;
            else if (isIdentifier)
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            else if (isString)
                Console.ForegroundColor = ConsoleColor.Magenta;
            else Console.ForegroundColor = ConsoleColor.DarkGray;

            Console.Write(token.Text);

            Console.ResetColor();
        }
    }

    [MetaCommand("showTree", "Show parse trees.")]
    private void EvaluateShowTree() {
        _showTree = !_showTree;
        Console.WriteLine(_showTree ? "Showing parse trees." : "Not showing parse trees.");
    }

    [MetaCommand("showProgram", "Show bound tree.")]
    private void EvaluateShowProgram() {
        _showProgram = !_showProgram;
        Console.WriteLine(_showProgram ? "Showing bound tree." : "Not showing bound tree.");
    }

    [MetaCommand("cls", "Clears the console.")]
    private void EvaluateCls() {
        Console.Clear();
    }

    [MetaCommand("reset", "Clears all previous submissions.")]
    private void EvaluateReset() {
        _previous = null;
        _variables.Clear();
    }

    protected override bool IsCompleteSubmission(string text) {
        if (string.IsNullOrEmpty(text))
            return true;

        var lastTwoLinesAreBlank = text.Split(Environment.NewLine).Reverse().Take(2).All(string.IsNullOrWhiteSpace);
        if (lastTwoLinesAreBlank) return true;

        var syntaxTree = SyntaxTree.Parse(text);

        if (syntaxTree.Root.Members.Last().GetLastToken().IsMissing) return false;

        return true;
    }

    protected override void EvaluateSubmission(string text) {
        var syntaxTree = SyntaxTree.Parse(text);

        var compilation = _previous == null
            ? new Compilation(syntaxTree)
            : _previous.ContinueWith(syntaxTree);

        if (_showTree)
            syntaxTree.Root.WriteTo(Console.Out);

        if (_showProgram)
            compilation.EmitTree(Console.Out);

        var result = compilation.Evaluate(_variables);

        if (!result.Diagnostics.Any()) {
            if (result.Value != null) {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(result.Value);
                Console.ResetColor();
            }

            _previous = compilation;
        }

        else {
            Console.Out.WriteDiagnostics(result.Diagnostics);
        }
    }
}