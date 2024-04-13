namespace Rhino.CodeAnalysis.Symbols;

public class GlobalVariableSymbol : VariableSymbol {
    public GlobalVariableSymbol(string name, bool isReadOnly, TypeSymbol type) : base(name, isReadOnly, type) {
    }

    public override SymbolKind Kind => SymbolKind.GlobalVariable;
}