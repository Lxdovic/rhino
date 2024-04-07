using Rhino.CodeAnalysis.Symbols;

namespace Rhino.CodeAnalysis.Binding;

internal abstract class BoundExpression : BoundNode {
    public abstract TypeSymbol Type { get; }
}