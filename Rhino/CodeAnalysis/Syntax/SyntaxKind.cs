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
    BangToken,
    AmpersandAmpersandToken,
    PipePipeToken,
    EqualsToken,
    EqualsEqualsToken,
    BangEqualsToken,

    FalseKeyword,
    TrueKeyword,

    LiteralExpression,
    BinaryExpression,
    UnaryExpression,
    ParenthesizedExpression,
    NameExpression,
    AssignmentExpression
}