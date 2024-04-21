namespace Rhino.CodeAnalysis.Binding;

internal enum BoundNodeKind {
    UnaryExpression,
    LiteralExpression,
    BinaryExpression,
    VariableExpression,
    AssignmentExpression,
    ErrorExpression,
    CallExpression,
    ConversionExpression,

    BlockStatement,
    ExpressionStatement,
    VariableDeclaration,
    IfStatement,
    WhileStatement,
    ForStatement,
    GotoStatement,
    LabelStatement,
    ConditionalGotoStatement,
    ReturnStatement
}