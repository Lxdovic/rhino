namespace Rhino.CodeAnalysis.Binding;

internal enum BoundBinaryOperatorKind {
    Addition,
    Subtraction,
    Multiplication,
    Division,
    LogicalAnd,
    LogicalOr,
    Equals,
    GreaterEquals,
    LessEquals,
    GreaterThan,
    LessThan,
    NotEquals,
    BitwiseAnd,
    BitwiseOr,
    BitwiseXor,
    BitwiseLeftShift,
    BitwiseRightShift,
    Modulus
}