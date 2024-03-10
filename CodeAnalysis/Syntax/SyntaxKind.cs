namespace Rhino.CodeAnalysis.Syntax;

public enum SyntaxKind {
    BadToken,
    EndOfFileToken,
    WhiteSpaceToken,
    NumberToken,
    PlusToken,
    MinusToken,
    StarToken,
    SlashToken,
    OpenParenthesisToken,
    CloseParenthesisToken,
    IdentifierToken,

    FalseKeyword,
    TrueKeyword,

    LiteralExpression,
    BinaryExpression,
    UnaryExpression,
    ParenthesizedExpression
}