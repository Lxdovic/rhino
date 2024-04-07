using Rhino.CodeAnalysis.Symbols;

namespace Rhino.CodeAnalysis.Binding;

internal sealed class BoundConversionExpression : BoundExpression {
    public BoundConversionExpression(TypeSymbol type, BoundExpression expression) {
        Expression = expression;
        Type = type;
    }

    public BoundExpression Expression { get; }
    public override TypeSymbol Type { get; }
    public override BoundNodeKind Kind => BoundNodeKind.ConversionExpression;
}