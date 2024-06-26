namespace Rhino.CodeAnalysis.Syntax;

public sealed class TypeClauseSyntax : SyntaxNode {
    public TypeClauseSyntax(SyntaxTree syntaxTree, SyntaxToken colonToken, SyntaxToken identifier) : base(syntaxTree) {
        ColonToken = colonToken;
        Identifier = identifier;
    }

    public SyntaxToken ColonToken { get; set; }
    public SyntaxToken Identifier { get; }
    public override SyntaxKind Kind => SyntaxKind.TypeClause;
}