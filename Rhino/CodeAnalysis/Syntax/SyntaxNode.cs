using System.Reflection;
using Rhino.CodeAnalysis.Text;

namespace Rhino.CodeAnalysis.Syntax;

public abstract class SyntaxNode {
    protected SyntaxNode(SyntaxTree syntaxTree) {
        SyntaxTree = syntaxTree;
    }

    public SyntaxTree SyntaxTree { get; }

    public abstract SyntaxKind Kind { get; }

    public virtual TextSpan Span {
        get {
            var first = GetChildren().First().Span;
            var last = GetChildren().Last().Span;
            return TextSpan.FromBounds(first.Start, last.End);
        }
    }

    public TextLocation Location => new(SyntaxTree.Text, Span);

    public IEnumerable<SyntaxNode> GetChildren() {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
            if (typeof(SyntaxNode).IsAssignableFrom(property.PropertyType)) {
                var child = (SyntaxNode)property.GetValue(this);

                if (child != null) yield return child;
            }

            else if (typeof(SeparatedSyntaxList).IsAssignableFrom(property.PropertyType)) {
                var sepratedSyntaxList = (SeparatedSyntaxList)property.GetValue(this);
                foreach (var child in sepratedSyntaxList.GetWithSeparators())
                    yield return child;
            }

            else if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(property.PropertyType)) {
                foreach (var child in (IEnumerable<SyntaxNode>)property.GetValue(this))
                    if (child != null)
                        yield return child;
            }
    }

    public SyntaxToken GetLastToken() {
        if (this is SyntaxToken token) return token;

        return GetChildren().Last().GetLastToken();
    }

    public void WriteTo(TextWriter writer) {
        PrettyPrint(writer, this);
    }

    private static void PrettyPrint(TextWriter writer, SyntaxNode node, string indent = "", bool isLast = true) {
        var isToConsole = writer == Console.Out;
        var marker = isLast ? "└──" : "├──";

        if (isToConsole) Console.ForegroundColor = ConsoleColor.DarkGray;

        Console.Write(indent);
        Console.Write(marker);

        if (isToConsole) Console.ForegroundColor = node is SyntaxToken ? ConsoleColor.Blue : ConsoleColor.Cyan;

        Console.Write(node.Kind);

        if (node is SyntaxToken t && t.Value != null) {
            Console.Write(" ");
            Console.Write(t.Value);
        }

        if (isToConsole) Console.ResetColor();

        Console.WriteLine();

        indent += isLast ? "   " : "│  ";

        var lastChild = node.GetChildren().LastOrDefault();

        foreach (var child in node.GetChildren()) PrettyPrint(writer, child, indent, child == lastChild);
    }

    public override string ToString() {
        using var writer = new StringWriter();
        WriteTo(writer);
        return writer.ToString();
    }
}