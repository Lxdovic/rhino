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
    OpenBraceToken,
    CloseBraceToken,
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

    BlockStatement,
    ExpressionStatement,

    LiteralExpression,
    BinaryExpression,
    UnaryExpression,
    ParenthesizedExpression,
    NameExpression,
    AssignmentExpression,

    CompilationUnit
}