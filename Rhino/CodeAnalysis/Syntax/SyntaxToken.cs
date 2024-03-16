namespace Rhino.CodeAnalysis.Syntax;

public class SyntaxToken : SyntaxNode {
    public SyntaxToken(SyntaxKind kind, int position, string? text = null, object? value = null) {
        Kind = kind;
        Position = position;
        Text = text;
        Value = value;
    }

    public override SyntaxKind Kind { get; }
    public int Position { get; }
    public string? Text { get; }
    public object? Value { get; }
    public override TextSpan Span => new(Position, Text.Length);
}