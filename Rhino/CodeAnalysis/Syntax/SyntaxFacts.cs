namespace Rhino.CodeAnalysis.Syntax;

internal static class SyntaxFacts {
    public static int GetBinaryOperatorPrecedence(this SyntaxKind kind) {
        switch (kind) {
            case SyntaxKind.StarToken:
            case SyntaxKind.SlashToken:
                return 5;
            case SyntaxKind.PlusToken:
            case SyntaxKind.MinusToken:
            case SyntaxKind.AmpersandToken:
            case SyntaxKind.PipeToken:
            case SyntaxKind.HatToken:
            case SyntaxKind.LessThanLessThanToken:
            case SyntaxKind.GreaterThanGreaterThanToken:
                return 4;
            case SyntaxKind.EqualsEqualsToken:
            case SyntaxKind.BangEqualsToken:
            case SyntaxKind.GreaterThanOrEqualsToken:
            case SyntaxKind.LessThanOrEqualsToken:
                return 3;
            case SyntaxKind.AmpersandAmpersandToken:
                return 2;
            case SyntaxKind.PipePipeToken:
                return 1;
            default:
                return 0;
        }
    }

    public static int GetUnaryOperatorPrecedence(this SyntaxKind kind) {
        switch (kind) {
            case SyntaxKind.PlusToken:
            case SyntaxKind.MinusToken:
            case SyntaxKind.BangToken:
            case SyntaxKind.TildeToken:
                return 6;
            default:
                return 0;
        }
    }

    public static SyntaxKind GetKeywordKind(string text) {
        switch (text) {
            case "true":
                return SyntaxKind.TrueKeyword;
            case "false":
                return SyntaxKind.FalseKeyword;
            default:
                return SyntaxKind.IdentifierToken;
        }
    }
}