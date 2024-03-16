using System.Reflection;

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
}