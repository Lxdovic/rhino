namespace Rhino.CodeAnalysis.Syntax;

public static class SyntaxFacts {
    public static int GetBinaryOperatorPrecedence(this SyntaxKind kind) {
        switch (kind) {
            case SyntaxKind.StarToken:
            case SyntaxKind.SlashToken:
            case SyntaxKind.ModuloToken:
                return 5;
            case SyntaxKind.PlusToken:
            case SyntaxKind.MinusToken:
            case SyntaxKind.LessThanLessThanToken:
            case SyntaxKind.GreaterThanGreaterThanToken:
                return 4;
            case SyntaxKind.EqualsEqualsToken:
            case SyntaxKind.BangEqualsToken:
            case SyntaxKind.GreaterThanOrEqualsToken:
            case SyntaxKind.LessThanOrEqualsToken:
            case SyntaxKind.LessToken:
            case SyntaxKind.GreaterToken:
                return 3;
            case SyntaxKind.AmpersandAmpersandToken:
            case SyntaxKind.AmpersandToken:
                return 2;
            case SyntaxKind.PipePipeToken:
            case SyntaxKind.PipeToken:
            case SyntaxKind.HatToken:
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
                return 0; // not a unary operator
        }
    }

    public static SyntaxKind GetKeywordKind(string text) {
        switch (text) {
            case "true":
                return SyntaxKind.TrueKeyword;
            case "false":
                return SyntaxKind.FalseKeyword;
            case "let":
                return SyntaxKind.LetKeyword;
            case "var":
                return SyntaxKind.VarKeyword;
            case "if":
                return SyntaxKind.IfKeyword;
            case "else":
                return SyntaxKind.ElseKeyword;
            case "while":
                return SyntaxKind.WhileKeyword;
            case "for":
                return SyntaxKind.ForKeyword;
            case "function":
                return SyntaxKind.FunctionKeyword;
            case "to":
                return SyntaxKind.ToKeyword;
            default:
                return SyntaxKind.IdentifierToken;
        }
    }

    public static string? GetText(SyntaxKind kind) {
        switch (kind) {
            case SyntaxKind.PlusToken: return "+";
            case SyntaxKind.MinusToken: return "-";
            case SyntaxKind.StarToken: return "*";
            case SyntaxKind.SlashToken: return "/";
            case SyntaxKind.BangToken: return "!";
            case SyntaxKind.EqualsToken: return "=";
            case SyntaxKind.AmpersandAmpersandToken: return "&&";
            case SyntaxKind.PipePipeToken: return "||";
            case SyntaxKind.EqualsEqualsToken: return "==";
            case SyntaxKind.BangEqualsToken: return "!=";
            case SyntaxKind.OpenParenthesisToken: return "(";
            case SyntaxKind.CloseParenthesisToken: return ")";
            case SyntaxKind.OpenBraceToken: return "{";
            case SyntaxKind.CloseBraceToken: return "}";
            case SyntaxKind.FalseKeyword: return "false";
            case SyntaxKind.TrueKeyword: return "true";
            case SyntaxKind.LetKeyword: return "let";
            case SyntaxKind.VarKeyword: return "var";
            case SyntaxKind.WhileKeyword: return "while";
            case SyntaxKind.ForKeyword: return "for";
            case SyntaxKind.FunctionKeyword: return "function";
            case SyntaxKind.ToKeyword: return "to";
            case SyntaxKind.IfKeyword: return "if";
            case SyntaxKind.ElseKeyword: return "else";
            case SyntaxKind.AmpersandToken: return "&";
            case SyntaxKind.PipeToken: return "|";
            case SyntaxKind.HatToken: return "^";
            case SyntaxKind.LessThanLessThanToken: return "<<";
            case SyntaxKind.GreaterThanGreaterThanToken: return ">>";
            case SyntaxKind.TildeToken: return "~";
            case SyntaxKind.LessThanOrEqualsToken: return "<=";
            case SyntaxKind.GreaterThanOrEqualsToken: return ">=";
            case SyntaxKind.LessToken: return "<";
            case SyntaxKind.GreaterToken: return ">";
            case SyntaxKind.CommaToken: return ",";
            case SyntaxKind.ModuloToken: return "%";
            case SyntaxKind.ColonToken: return ":";
            default: return null;
        }
    }

    public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds() {
        var kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));

        foreach (var kind in kinds)
            if (GetBinaryOperatorPrecedence(kind) > 0)
                yield return kind;
    }

    public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds() {
        var kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));

        foreach (var kind in kinds)
            if (GetUnaryOperatorPrecedence(kind) > 0)
                yield return kind;
    }
}