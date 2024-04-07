namespace Rhino.CodeAnalysis.Symbols;

public sealed class VariableSymbol : Symbol {
    public VariableSymbol(string name, bool isReadOnly, TypeSymbol type) : base(name) {
        IsReadOnly = isReadOnly;
        Type = type;
    }

    public override SymbolKind Kind => SymbolKind.Variable;
    public bool IsReadOnly { get; }
    public TypeSymbol Type { get; }
}