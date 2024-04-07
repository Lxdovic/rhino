using System.Collections.Immutable;

namespace Rhino.CodeAnalysis.Symbols;

public sealed class FunctionSymbol : Symbol {
    public FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol returnType) : base(name) {
        Parameters = parameters;
        ReturnType = returnType;
    }

    public TypeSymbol ReturnType { get; }
    public ImmutableArray<ParameterSymbol> Parameters { get; }
    public override SymbolKind Kind => SymbolKind.Function;
}