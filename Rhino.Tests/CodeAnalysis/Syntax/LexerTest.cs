using Rhino.CodeAnalysis.Syntax;

namespace Rhino.Tests.CodeAnalysis.Syntax;

public class LexerTest {
    [Theory]
    [MemberData(nameof(GetTokensData))]
    public void LexerLexesToken(SyntaxKind kind, string text) {
        var tokens = SyntaxTree.ParseTokens(text).ToArray();
        var token = Assert.Single(tokens);

        Assert.Equal(kind, token.Kind);
        Assert.Equal(text, token.Text);
    }

    [Theory]
    [MemberData(nameof(GetTokenPairsData))]
    public void LexerLexesTokenPairs(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text) {
        var text = t1Text + t2Text;
        var tokens = SyntaxTree.ParseTokens(text).ToArray();

        Assert.Equal(2, tokens.Length);
        Assert.Equal(tokens[0].Kind, t1Kind);
        Assert.Equal(tokens[0].Text, t1Text);
        Assert.Equal(tokens[1].Kind, t2Kind);
        Assert.Equal(tokens[1].Text, t2Text);
    }

    [Theory]
    [MemberData(nameof(GetTokenPairsWithSeparatorData))]
    public void LexerLexesTokenPairsWithSeparator(SyntaxKind t1Kind, string t1Text, SyntaxKind separatorKind,
        string separatorText, SyntaxKind t2Kind, string t2Text) {
        var text = t1Text + separatorText + t2Text;
        var tokens = SyntaxTree.ParseTokens(text).ToArray();

        Assert.Equal(3, tokens.Length);
        Assert.Equal(tokens[0].Kind, t1Kind);
        Assert.Equal(tokens[0].Text, t1Text);
        ;
        Assert.Equal(tokens[1].Kind, separatorKind);
        Assert.Equal(tokens[1].Text, separatorText);
        Assert.Equal(tokens[2].Kind, t2Kind);
        Assert.Equal(tokens[2].Text, t2Text);
    }

    private static bool RequiresSeparator(SyntaxKind t1Kind, SyntaxKind t2Kind) {
        var t1IsKeyword = t1Kind.ToString().EndsWith("Keyword");
        var t2IsKeyword = t2Kind.ToString().EndsWith("Keyword");

        if (t1Kind == SyntaxKind.IdentifierToken && t2Kind == SyntaxKind.IdentifierToken)
            return true;

        if (t1IsKeyword && t2IsKeyword)
            return true;

        if (t1IsKeyword && t2Kind == SyntaxKind.IdentifierToken)
            return true;

        if (t1Kind == SyntaxKind.IdentifierToken && t2IsKeyword)
            return true;

        if (t1Kind == SyntaxKind.NumberToken && t2Kind == SyntaxKind.NumberToken)
            return true;

        if (t1Kind == SyntaxKind.BangToken && t2Kind == SyntaxKind.EqualsToken)
            return true;

        if (t1Kind == SyntaxKind.BangToken && t2Kind == SyntaxKind.EqualsEqualsToken)
            return true;

        if (t1Kind == SyntaxKind.EqualsToken && t2Kind == SyntaxKind.EqualsToken)
            return true;

        if (t1Kind == SyntaxKind.EqualsToken && t2Kind == SyntaxKind.EqualsEqualsToken)
            return true;

        if (t1Kind == SyntaxKind.BinaryAndToken && t2Kind == SyntaxKind.AmpersandAmpersandToken)
            return true;

        if (t1Kind == SyntaxKind.BinaryOrToken && t2Kind == SyntaxKind.PipePipeToken)
            return true;

        if (t1Kind == SyntaxKind.BinaryAndToken && t2Kind == SyntaxKind.BinaryAndToken)
            return true;

        if (t1Kind == SyntaxKind.BinaryOrToken && t2Kind == SyntaxKind.BinaryOrToken)
            return true;


        return false;
    }

    public static IEnumerable<object[]> GetTokensData() {
        foreach (var (kind, text) in GetTokens().Concat(GetSeparators()))
            yield return new object[] { kind, text };
    }

    public static IEnumerable<object[]> GetTokenPairsData() {
        foreach (var pair in GetTokenPairs())
            yield return new object[] { pair.t1Kind, pair.t1Text, pair.t2Kind, pair.t2Text };
    }

    public static IEnumerable<object[]> GetTokenPairsWithSeparatorData() {
        foreach (var pair in GetTokenPairsWithSeparator())
            yield return new object[]
                { pair.t1Kind, pair.t1Text, pair.separatorKind, pair.separatorText, pair.t2Kind, pair.t2Text };
    }

    private static IEnumerable<(SyntaxKind kind, string text)> GetTokens() {
        return new[] {
            (SyntaxKind.PlusToken, "+"),
            (SyntaxKind.MinusToken, "-"),
            (SyntaxKind.StarToken, "*"),
            (SyntaxKind.SlashToken, "/"),
            (SyntaxKind.BangToken, "!"),
            (SyntaxKind.EqualsToken, "="),
            (SyntaxKind.AmpersandAmpersandToken, "&&"),
            (SyntaxKind.PipePipeToken, "||"),
            (SyntaxKind.EqualsEqualsToken, "=="),
            (SyntaxKind.BangEqualsToken, "!="),
            (SyntaxKind.OpenParenthesisToken, "("),
            (SyntaxKind.CloseParenthesisToken, ")"),
            (SyntaxKind.FalseKeyword, "false"),
            (SyntaxKind.TrueKeyword, "true"),
            (SyntaxKind.BinaryAndToken, "&"),
            (SyntaxKind.BinaryOrToken, "|"),
            (SyntaxKind.HatToken, "^"),
            (SyntaxKind.LessThanLessThanToken, "<<"),
            (SyntaxKind.GreaterThanGreaterThanToken, ">>"),
            (SyntaxKind.TildeToken, "~"),
            (SyntaxKind.SmallerThanEqualsToken, "<="),
            (SyntaxKind.GreaterThanEqualsToken, ">="),

            (SyntaxKind.NumberToken, "1"),
            (SyntaxKind.NumberToken, "123"),
            (SyntaxKind.IdentifierToken, "a"),
            (SyntaxKind.IdentifierToken, "abc")
        };
    }

    private static IEnumerable<(SyntaxKind kind, string text)> GetSeparators() {
        return new[] {
            (SyntaxKind.WhiteSpaceToken, " "),
            (SyntaxKind.WhiteSpaceToken, "  "),
            (SyntaxKind.WhiteSpaceToken, "\r"),
            (SyntaxKind.WhiteSpaceToken, "\n"),
            (SyntaxKind.WhiteSpaceToken, "\r\n")
        };
    }

    private static
        IEnumerable<(SyntaxKind t1Kind, string t1Text, SyntaxKind separatorKind, string separatorText, SyntaxKind t2Kind
            , string t2Text)> GetTokenPairsWithSeparator() {
        foreach (var t1 in GetTokens())
        foreach (var t2 in GetTokens())
            if (!RequiresSeparator(t1.kind, t2.kind))
                foreach (var separator in GetSeparators())
                    yield return (t1.kind, t1.text, separator.kind, separator.text, t2.kind, t2.text);
    }

    private static IEnumerable<(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text)> GetTokenPairs() {
        foreach (var t1 in GetTokens())
        foreach (var t2 in GetTokens())
            if (!RequiresSeparator(t1.kind, t2.kind))
                yield return (t1.kind, t1.text, t2.kind, t2.text);
    }
}