using System.Collections.Immutable;
using Rhino.CodeAnalysis.Syntax;

namespace Rhino.CodeAnalysis.Symbols;

public sealed class FunctionSymbol : Symbol {
    public FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol returnType,
        FunctionDeclarationSyntax? declaration = null) : base(name) {
        Parameters = parameters;
        ReturnType = returnType;
        Declaration = declaration;
    }

    public TypeSymbol ReturnType { get; }
    public FunctionDeclarationSyntax? Declaration { get; }
    public ImmutableArray<ParameterSymbol> Parameters { get; }
    public override SymbolKind Kind => SymbolKind.Function;
}