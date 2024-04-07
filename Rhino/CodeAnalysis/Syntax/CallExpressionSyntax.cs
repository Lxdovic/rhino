namespace Rhino.CodeAnalysis.Syntax;

public sealed class CallExpressionSyntax : ExpressionSyntax {
    public CallExpressionSyntax(SyntaxToken identifierToken, SyntaxToken openParenthesissToken,
        SeparatedSyntaxList<ExpressionSyntax> arguments, SyntaxToken closeParenthesissToken) {
        Identifier = identifierToken;
        OpenParenthesissToken = openParenthesissToken;
        Arguments = arguments;
        CloseParenthesissToken = closeParenthesissToken;
    }

    public SyntaxToken Identifier { get; }
    public SyntaxToken OpenParenthesissToken { get; }
    public SeparatedSyntaxList<ExpressionSyntax> Arguments { get; }
    public SyntaxToken CloseParenthesissToken { get; }
    public override SyntaxKind Kind => SyntaxKind.CallExpression;
}