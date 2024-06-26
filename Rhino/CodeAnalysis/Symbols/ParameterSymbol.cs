namespace Rhino.CodeAnalysis.Symbols;

public sealed class ParameterSymbol : LocalVariableSymbol {
    public ParameterSymbol(string name, TypeSymbol type) : base(name, true, type) {
        Type = type;
    }

    public TypeSymbol Type { get; }

    public override SymbolKind Kind => SymbolKind.Parameter;
}