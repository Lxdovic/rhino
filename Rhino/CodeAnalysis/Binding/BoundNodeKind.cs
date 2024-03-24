namespace Rhino.CodeAnalysis.Binding;

internal enum BoundNodeKind {
    UnaryExpression,
    LiteralExpression,
    BinaryExpression,
    VariableExpression,
    AssignmentExpression,

    BlockStatement,
    ExpressionStatement,
    VariableDeclaration,
    IfStatement
}