using System.Reflection;
using Rhino.CodeAnalysis.Text;

namespace Rhino.CodeAnalysis.Syntax;

public abstract class SyntaxNode {
    public abstract SyntaxKind Kind { get; }

    public virtual TextSpan Span {
        get {
            var first = GetChildren().First().Span;
            var last = GetChildren().Last().Span;
            return TextSpan.FromBounds(first.Start, last.End);
        }
    }

    public IEnumerable<SyntaxNode> GetChildren() {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
            if (typeof(SyntaxNode).IsAssignableFrom(property.PropertyType))
                yield return (SyntaxNode)property.GetValue(this);

            else if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(property.PropertyType))
                foreach (var child in (IEnumerable<SyntaxNode>)property.GetValue(this))
                    yield return child;
    }

    public void WriteTo(TextWriter writer) {
        PrettyPrint(writer, this);
    }

    private static void PrettyPrint(TextWriter writer, SyntaxNode node, string indent = "", bool isLast = true) {
        var isToConsole = writer == Console.Out;
        var marker = isLast ? "└──" : "├──";

        Console.Write(indent);

        if (isToConsole) {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(marker);
            Console.ResetColor();
        }

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