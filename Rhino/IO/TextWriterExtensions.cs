using System.CodeDom.Compiler;
using Rhino.CodeAnalysis;
using Rhino.CodeAnalysis.Syntax;
using Rhino.CodeAnalysis.Text;

namespace Rhino.IO;

public static class TextWriterExtensions {
    private static bool IsConsoleOut(this TextWriter writer) {
        if (writer == Console.Out) return true;
        if (writer is IndentedTextWriter iw && iw.InnerWriter.IsConsoleOut()) return true;

        return false;
    }

    private static void SetForeground(this TextWriter writer, ConsoleColor color) {
        if (writer.IsConsoleOut()) Console.ForegroundColor = color;
    }

    private static void ResetColor(this TextWriter writer) {
        if (writer.IsConsoleOut()) Console.ResetColor();
    }

    public static void WriteKeyword(this TextWriter writer, SyntaxKind kind) {
        writer.WriteKeyword(SyntaxFacts.GetText(kind));
    }


    public static void WriteKeyword(this TextWriter writer, string text) {
        writer.SetForeground(ConsoleColor.Blue);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteIdentifier(this TextWriter writer, string text) {
        writer.SetForeground(ConsoleColor.Yellow);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteNumber(this TextWriter writer, string text) {
        writer.SetForeground(ConsoleColor.Cyan);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteString(this TextWriter writer, string text) {
        writer.SetForeground(ConsoleColor.Magenta);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteSpace(this TextWriter writer) {
        writer.WritePunctuation(" ");
    }


    public static void WritePunctuation(this TextWriter writer, SyntaxKind kind) {
        writer.WritePunctuation(SyntaxFacts.GetText(kind));
    }

    public static void WritePunctuation(this TextWriter writer, string text) {
        writer.SetForeground(ConsoleColor.DarkGray);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteDiagnostics(this TextWriter writer, IEnumerable<Diagnostic> diagnostics,
        SyntaxTree syntaxTree) {
        foreach (var diagnostic in diagnostics
                     .OrderBy(diag => diag.Location.FileName)
                     .ThenBy(diag => diag.Location.Span.Start)
                     .ThenBy(diag => diag.Location.Span.Length)) {
            var fileName = diagnostic.Location.FileName;
            var startLine = diagnostic.Location.StartLine + 1;
            var startCharacter = diagnostic.Location.StartCharacter + 1;
            var endLine = diagnostic.Location.EndLine + 1;
            var endCharacter = diagnostic.Location.EndCharacter + 1;
            var span = diagnostic.Location.Span;
            var lineIndex = syntaxTree.Text.GetLineIndex(span.Start);
            var line = syntaxTree.Text.Lines[lineIndex];

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write($"{fileName}({startLine},{startCharacter},{endLine},{endCharacter}): ");
            Console.WriteLine(diagnostic);
            Console.ResetColor();

            var prefixSpan = TextSpan.FromBounds(line.Start, span.Start);
            var suffixSpan = TextSpan.FromBounds(span.End, line.End);

            var prefix = syntaxTree.Text.ToString(prefixSpan);
            var error = syntaxTree.Text.ToString(span);
            var suffix = syntaxTree.Text.ToString(suffixSpan);

            Console.Write("    ");
            Console.Write(prefix);

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(error);
            Console.ResetColor();

            Console.Write(suffix);

            Console.WriteLine();
        }
    }
}