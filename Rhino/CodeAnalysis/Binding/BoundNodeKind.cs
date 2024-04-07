namespace Rhino.CodeAnalysis.Binding;

internal enum BoundNodeKind {
    UnaryExpression,
    LiteralExpression,
    BinaryExpression,
    VariableExpression,
    AssignmentExpression,
    ErrorExpression,

    BlockStatement,
    ExpressionStatement,
    VariableDeclaration,
    IfStatement,
    WhileStatement,
    ForStatement,
    GotoStatement,
    LabelStatement,
    ConditionalGotoStatement
}