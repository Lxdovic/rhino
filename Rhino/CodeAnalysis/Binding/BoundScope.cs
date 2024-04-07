using System.Collections.Immutable;
using Rhino.CodeAnalysis.Symbols;

namespace Rhino.CodeAnalysis.Binding;

internal sealed class BoundScope {
    private readonly Dictionary<string, VariableSymbol> _variables = new();

    public BoundScope(BoundScope parent) {
        Parent = parent;
    }

    public BoundScope Parent { get; }

    public bool TryLookup(string name, out VariableSymbol variable) {
        if (_variables.TryGetValue(name, out variable)) return true;

        if (Parent == null) return false;

        return Parent.TryLookup(name, out variable);
    }

    public bool TryDeclare(VariableSymbol variable) {
        if (!_variables.TryAdd(variable.Name, variable)) return false;

        return true;
    }

    public ImmutableArray<VariableSymbol> GetDeclaredVariables() {
        return _variables.Values.ToImmutableArray();
    }
}