using System.Collections.Immutable;
using Rhino.CodeAnalysis.Symbols;

namespace Rhino.CodeAnalysis.Binding;

internal sealed class BoundScope {
    private Dictionary<string, FunctionSymbol> _functions;
    private Dictionary<string, VariableSymbol> _variables;

    public BoundScope(BoundScope parent) {
        Parent = parent;
    }

    public BoundScope Parent { get; }

    public bool TryLookupVariable(string name, out VariableSymbol variable) {
        variable = null;

        if (_variables != null && _variables.TryGetValue(name, out variable)) return true;

        if (Parent == null) return false;

        return Parent.TryLookupVariable(name, out variable);
    }

    public bool TryDeclareVariable(VariableSymbol variable) {
        if (_variables == null) _variables = new Dictionary<string, VariableSymbol>();
        if (!_variables.TryAdd(variable.Name, variable)) return false;

        return true;
    }

    public bool TryLookupFunction(string name, out FunctionSymbol function) {
        function = null;

        if (_functions != null && _functions.TryGetValue(name, out function)) return true;

        if (Parent == null) return false;

        return Parent.TryLookupFunction(name, out function);
    }

    public bool TryDeclareFunction(FunctionSymbol function) {
        if (_functions == null) _functions = new Dictionary<string, FunctionSymbol>();
        if (!_functions.TryAdd(function.Name, function)) return false;

        return true;
    }

    public ImmutableArray<VariableSymbol> GetDeclaredVariables() {
        if (_variables == null) return ImmutableArray<VariableSymbol>.Empty;
        return _variables.Values.ToImmutableArray();
    }

    public ImmutableArray<FunctionSymbol> GetDeclaredFunctions() {
        if (_functions == null) return ImmutableArray<FunctionSymbol>.Empty;
        return _functions.Values.ToImmutableArray();
    }
}