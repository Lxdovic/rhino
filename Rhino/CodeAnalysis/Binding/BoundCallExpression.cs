using System.Collections.Immutable;
using Rhino.CodeAnalysis.Symbols;

namespace Rhino.CodeAnalysis.Binding;

internal sealed class BoundCallExpression : BoundExpression {
    public BoundCallExpression(FunctionSymbol function, ImmutableArray<BoundExpression> arguments) {
        Function = function;
        Arguments = arguments;
    }

    public override TypeSymbol Type => Function.ReturnType;
    public FunctionSymbol Function { get; }
    public ImmutableArray<BoundExpression> Arguments { get; }

    public override BoundNodeKind Kind => BoundNodeKind.CallExpression;
}