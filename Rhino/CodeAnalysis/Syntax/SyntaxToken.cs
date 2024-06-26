using Rhino.CodeAnalysis.Text;

namespace Rhino.CodeAnalysis.Syntax;

public class SyntaxToken : SyntaxNode {
    public SyntaxToken(SyntaxTree syntaxTree, SyntaxKind kind, int position, string? text = null, object? value = null)
        : base(syntaxTree) {
        Kind = kind;
        Position = position;
        Text = text;
        Value = value;
    }

    public override SyntaxKind Kind { get; }
    public int Position { get; }
    public string? Text { get; }
    public object? Value { get; }
    public override TextSpan Span => new(Position, Text?.Length ?? 0);

    public bool IsMissing => string.IsNullOrEmpty(Text);
}