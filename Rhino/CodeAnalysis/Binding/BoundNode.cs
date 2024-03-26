using System.Reflection;

namespace Rhino.CodeAnalysis.Binding;

internal abstract class BoundNode {
    public abstract BoundNodeKind Kind { get; }

    public IEnumerable<BoundNode> GetChildren() {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
            if (typeof(BoundNode).IsAssignableFrom(property.PropertyType)) {
                var child = (BoundNode)property.GetValue(this);

                if (child != null) yield return child;
            }

            else if (typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType)) {
                foreach (var child in (IEnumerable<BoundNode>)property.GetValue(this))
                    if (child != null)
                        yield return child;
            }
    }

    private IEnumerable<(string Name, object value)> GetProperties() {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties) {
            if (property.Name == nameof(Kind) || property.Name == nameof(BoundBinaryExpression.Op)) continue;

            if (typeof(BoundNode).IsAssignableFrom(property.PropertyType) ||
                typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
                continue;

            var value = property.GetValue(this);

            if (value != null) yield return (property.Name, value);
        }
    }


    public void WriteTo(TextWriter writer) {
        PrettyPrint(writer, this);
    }

    private static void PrettyPrint(TextWriter writer, BoundNode node, string indent = "", bool isLast = true) {
        var isToConsole = writer == Console.Out;
        var marker = isLast ? "└──" : "├──";

        if (isToConsole) Console.ForegroundColor = ConsoleColor.DarkGray;

        Console.Write(indent);
        Console.Write(marker);

        if (isToConsole) Console.ForegroundColor = GetColor(node);

        var text = GetText(node);
        writer.Write(text);

        var isFirstProperty = true;

        foreach (var (name, value) in node.GetProperties()) {
            if (isFirstProperty) {
                isFirstProperty = false;
            }

            else {
                if (isToConsole) Console.ForegroundColor = ConsoleColor.DarkGray;
                writer.Write(",");
            }

            writer.Write(" ");

            if (isToConsole) Console.ForegroundColor = ConsoleColor.Yellow;

            writer.Write(name);

            if (isToConsole) Console.ForegroundColor = ConsoleColor.DarkGray;

            writer.Write(" = ");

            if (isToConsole) Console.ForegroundColor = ConsoleColor.DarkYellow;

            writer.Write(value);
        }

        if (isToConsole) Console.ResetColor();

        Console.WriteLine();

        indent += isLast ? "   " : "│  ";

        var lastChild = node.GetChildren().LastOrDefault();

        foreach (var child in node.GetChildren()) PrettyPrint(writer, child, indent, child == lastChild);
    }

    private static object GetText(BoundNode node) {
        if (node is BoundBinaryExpression b) return b.Op.Kind + "Expression";
        if (node is BoundUnaryExpression u) return u.Op.Kind + "Expression";

        return node.Kind.ToString();
    }

    private static ConsoleColor GetColor(BoundNode node) {
        return node switch {
            BoundExpression => ConsoleColor.Blue,
            BoundStatement => ConsoleColor.Cyan,
            _ => ConsoleColor.Yellow
        };
    }

    public override string ToString() {
        using var writer = new StringWriter();
        WriteTo(writer);
        return writer.ToString();
    }
}