using Rhino.CodeAnalysis.Symbols;

namespace Rhino.CodeAnalysis.Binding;

internal sealed class Conversion {
    public static readonly Conversion None = new(false, false, false);
    public static readonly Conversion Identity = new(true, true, true);
    public static readonly Conversion Implicit = new(true, false, true);
    public static readonly Conversion Explicit = new(true, false, false);

    private Conversion(bool exists, bool isIdentity, bool isImplicit) {
        Exists = exists;
        IsIdentity = isIdentity;
        IsImplicit = isImplicit;
    }

    public bool IsImplicit { get; set; }
    public bool IsIdentity { get; set; }
    public bool IsExplicit => Exists && !IsImplicit;

    public bool Exists { get; set; }

    public static Conversion Classify(TypeSymbol from, TypeSymbol to) {
        if (from == to) return Identity;

        if (from == TypeSymbol.Bool)
            if (to == TypeSymbol.String)
                return Explicit;

        if (from == TypeSymbol.String)
            if (to == TypeSymbol.Bool || to == TypeSymbol.Int || to == TypeSymbol.Float)
                return Explicit;

        if (from == TypeSymbol.Int) {
            if (to == TypeSymbol.Float)
                return Implicit;

            if (to == TypeSymbol.String || to == TypeSymbol.Bool)
                return Explicit;
        }

        if (from == TypeSymbol.Float) {
            if (to == TypeSymbol.Int)
                return Implicit;

            if (to == TypeSymbol.String)
                return Explicit;
        }

        return None;
    }
}