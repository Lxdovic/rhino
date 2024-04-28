using System.Collections.Immutable;
using Rhino.CodeAnalysis.Symbols;

namespace Rhino.CodeAnalysis.Binding;

internal sealed class BoundScope {
    private Dictionary<string, Symbol> _symbols;

    public BoundScope(BoundScope parent) {
        Parent = parent;
    }

    public BoundScope Parent { get; }

    public bool TryDeclareVariable(VariableSymbol variable) {
        return TryDeclareSymbol(variable);
    }

    public bool TryDeclareFunction(FunctionSymbol function) {
        return TryDeclareSymbol(function);
    }

    private bool TryDeclareSymbol<TSymbol>(TSymbol symbol)
        where TSymbol : Symbol {
        if (_symbols == null)
            _symbols = new Dictionary<string, Symbol>();
        else if (_symbols.ContainsKey(symbol.Name))
            return false;

        _symbols.Add(symbol.Name, symbol);
        return true;
    }

    public Symbol TryLookupSymbol(string name) {
        if (_symbols != null && _symbols.TryGetValue(name, out var symbol)) return symbol;

        return Parent?.TryLookupSymbol(name);
    }

    public ImmutableArray<VariableSymbol> GetDeclaredVariables() {
        return GetDeclaredSymbols<VariableSymbol>();
    }

    public ImmutableArray<FunctionSymbol> GetDeclaredFunctions() {
        return GetDeclaredSymbols<FunctionSymbol>();
    }

    private ImmutableArray<TSymbol> GetDeclaredSymbols<TSymbol>()
        where TSymbol : Symbol {
        if (_symbols == null)
            return ImmutableArray<TSymbol>.Empty;

        return _symbols.Values.OfType<TSymbol>().ToImmutableArray();
    }
}