using Rhino.CodeAnalysis.Symbols;

namespace Rhino.CodeAnalysis.Binding;

internal sealed class BoundVariableExpression : BoundExpression {
    public BoundVariableExpression(VariableSymbol variable) {
        Variable = variable;
    }

    public VariableSymbol Variable { get; }
    public override TypeSymbol Type => Variable.Type;
    public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
}