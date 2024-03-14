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
    AmpersandToken,
    PipeToken,
    HatToken,
    LessThanLessThanToken,
    GreaterThanGreaterThanToken,
    TildeToken,
    GreaterThanOrEqualsToken,
    LessThanOrEqualsToken,

    FalseKeyword,
    TrueKeyword,

    LiteralExpression,
    BinaryExpression,
    UnaryExpression,
    ParenthesizedExpression,
    NameExpression,
    AssignmentExpression
}