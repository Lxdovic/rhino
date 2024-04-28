using System.Collections.Immutable;
using Rhino.CodeAnalysis.Text;

namespace Rhino.CodeAnalysis.Syntax;

public sealed class SyntaxTree {
    private SyntaxTree(SourceText text, ParseHandler handler) {
        Text = text;

        handler(this, out var root, out var diagnostics);

        Diagnostics = diagnostics;
        Root = root;
    }

    public SourceText Text { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public CompilationUnitSyntax Root { get; }

    public static SyntaxTree Load(string fileName) {
        var text = File.ReadAllText(fileName);
        var sourceText = SourceText.From(text, fileName);

        return Parse(sourceText);
    }

    public static SyntaxTree Parse(string text) {
        var sourceText = SourceText.From(text);
        return Parse(sourceText);
    }

    public static SyntaxTree Parse(SourceText text) {
        return new SyntaxTree(text, Parse);
    }

    public static ImmutableArray<SyntaxToken> ParseTokens(SourceText text, out ImmutableArray<Diagnostic> diagnostics) {
        var tokens = new List<SyntaxToken>();

        void ParseTokens(SyntaxTree st, out CompilationUnitSyntax root,
            out ImmutableArray<Diagnostic> d) {
            root = null;

            var lexer = new Lexer(st);

            while (true) {
                var token = lexer.Lex();

                if (token.Kind == SyntaxKind.EndOfFileToken) {
                    root = new CompilationUnitSyntax(st, ImmutableArray<MemberSyntax>.Empty, token);
                    break;
                }

                tokens.Add(token);
            }

            d = lexer.Diagnostics.ToImmutableArray();
        }

        var syntaxTree = new SyntaxTree(text, ParseTokens);

        diagnostics = syntaxTree.Diagnostics.ToImmutableArray();

        return tokens.ToImmutableArray();
    }

    public static ImmutableArray<SyntaxToken> ParseTokens(string text) {
        var sourceText = SourceText.From(text);
        return ParseTokens(sourceText);
    }

    public static ImmutableArray<SyntaxToken> ParseTokens(string text, out ImmutableArray<Diagnostic> diagnostics) {
        var sourceText = SourceText.From(text);
        return ParseTokens(sourceText, out diagnostics);
    }

    public static ImmutableArray<SyntaxToken> ParseTokens(SourceText text) {
        return ParseTokens(text, out _);
    }

    private static void Parse(SyntaxTree syntaxTree, out CompilationUnitSyntax root,
        out ImmutableArray<Diagnostic> diagnostics) {
        var parser = new Parser(syntaxTree);

        root = parser.ParseCompilationUnit();
        diagnostics = parser.Diagnostics.ToImmutableArray();
    }

    private delegate void ParseHandler(SyntaxTree syntaxTree, out CompilationUnitSyntax root,
        out ImmutableArray<Diagnostic> diagnostics);
}