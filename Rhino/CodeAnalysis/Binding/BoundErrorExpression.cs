using Rhino.CodeAnalysis.Symbols;

namespace Rhino.CodeAnalysis.Binding;

internal sealed class BoundErrorExpression : BoundExpression {
    public override TypeSymbol Type => TypeSymbol.Error;
    public override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;
}