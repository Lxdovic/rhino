namespace Rhino.CodeAnalysis.Syntax;

public sealed class ParameterSyntax : SyntaxNode {
    public ParameterSyntax(SyntaxTree syntaxTree, SyntaxToken identifier, TypeClauseSyntax type) : base(syntaxTree) {
        Identifier = identifier;
        Type = type;
    }

    public SyntaxToken Identifier { get; }
    public TypeClauseSyntax Type { get; }
    public override SyntaxKind Kind => SyntaxKind.Parameter;
}