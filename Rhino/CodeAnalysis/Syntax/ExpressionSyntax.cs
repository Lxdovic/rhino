namespace Rhino.CodeAnalysis.Syntax;

public abstract class ExpressionSyntax : SyntaxNode {
    protected ExpressionSyntax(SyntaxTree syntaxTree) : base(syntaxTree) {
    }
}