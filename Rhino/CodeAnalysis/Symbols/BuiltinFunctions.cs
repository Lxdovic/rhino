using System.Collections.Immutable;
using System.Reflection;

namespace Rhino.CodeAnalysis.Symbols;

internal static class BuiltinFunctions {
    public static readonly FunctionSymbol Print = new("print",
        ImmutableArray.Create(new ParameterSymbol("text", TypeSymbol.String)), TypeSymbol.Void);
    
    public static readonly FunctionSymbol PrintLine = new("println",
        ImmutableArray.Create(new ParameterSymbol("text", TypeSymbol.String)), TypeSymbol.Void);
    
    public static readonly FunctionSymbol
        Input = new("input", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.String);

    public static readonly FunctionSymbol
        Random = new("random",
            ImmutableArray.Create(new ParameterSymbol("min", TypeSymbol.Int),
                new ParameterSymbol("max", TypeSymbol.Int)), TypeSymbol.Int);

    public static IEnumerable<FunctionSymbol> GetAll() {
        return typeof(BuiltinFunctions).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(FunctionSymbol))
            .Select(f => (FunctionSymbol)f.GetValue(null)!);
    }
}