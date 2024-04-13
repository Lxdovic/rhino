namespace Rhino.CodeAnalysis.Symbols;

public class LocalVariableSymbol : VariableSymbol {
    public LocalVariableSymbol(string name, bool isReadOnly, TypeSymbol type) : base(name, isReadOnly, type) {
    }

    public override SymbolKind Kind => SymbolKind.LocalVariable;
}