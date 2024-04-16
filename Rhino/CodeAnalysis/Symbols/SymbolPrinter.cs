using Rhino.IO;

namespace Rhino.CodeAnalysis.Symbols;

internal static class SymbolPrinter {
    public static void WriteTo(Symbol symbol, TextWriter writer) {
        switch (symbol.Kind) {
            case SymbolKind.Function:
                WriteFunctionTo((FunctionSymbol)symbol, writer);
                break;
            case SymbolKind.Parameter:
                WriteParameterTo((ParameterSymbol)symbol, writer);
                break;
            case SymbolKind.Type:
                WriteTypeTo((TypeSymbol)symbol, writer);
                break;
            case SymbolKind.GlobalVariable:
                WriteGlobalVariableTo((GlobalVariableSymbol)symbol, writer);
                break;
            case SymbolKind.LocalVariable:
                WriteLocalVariableTo((LocalVariableSymbol)symbol, writer);
                break;
            default:
                throw new Exception($"Unexpected symbol: {symbol.Kind}");
        }
    }

    private static void WriteLocalVariableTo(LocalVariableSymbol symbol, TextWriter writer) {
        writer.WriteKeyword(symbol.IsReadOnly ? "let" : "var");
        writer.WriteIdentifier(symbol.Name);
        writer.WritePonctuation(":");

        symbol.Type.WriteTo(writer);
    }

    private static void WriteGlobalVariableTo(GlobalVariableSymbol symbol, TextWriter writer) {
        writer.WriteKeyword(symbol.IsReadOnly ? "let " : "var ");
        writer.WriteIdentifier(symbol.Name);
        writer.WritePonctuation(":");

        symbol.Type.WriteTo(writer);
    }

    private static void WriteTypeTo(TypeSymbol symbol, TextWriter writer) {
        writer.WriteIdentifier(symbol.Name);
    }

    private static void WriteParameterTo(ParameterSymbol symbol, TextWriter writer) {
        writer.WriteIdentifier(symbol.Name);
        writer.WritePonctuation(":");

        symbol.Type.WriteTo(writer);
    }

    private static void WriteFunctionTo(FunctionSymbol symbol, TextWriter writer) {
        writer.WriteKeyword("function ");
        writer.WriteIdentifier(symbol.Name);
        writer.WritePonctuation("(");

        for (var index = 0; index < symbol.Parameters.Length; index++) {
            if (index > 0) writer.WritePonctuation(", ");

            symbol.Parameters[index].WriteTo(writer);
        }

        writer.WritePonctuation(")");
        writer.WriteLine();
    }
}