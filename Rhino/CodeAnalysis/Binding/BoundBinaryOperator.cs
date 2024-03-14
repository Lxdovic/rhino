using Rhino.CodeAnalysis.Syntax;

namespace Rhino.CodeAnalysis.Binding;

internal sealed class BoundBinaryOperator {
    private static readonly BoundBinaryOperator[] _operators = {
        new(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, typeof(int)),
        new(SyntaxKind.MinusToken, BoundBinaryOperatorKind.Subtraction, typeof(int)),
        new(SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication, typeof(int)),
        new(SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division, typeof(int)),
        new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, typeof(int), typeof(bool)),
        new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, typeof(int), typeof(bool)),
        new(SyntaxKind.AmpersandAmpersandToken, BoundBinaryOperatorKind.LogicalAnd, typeof(bool)),
        new(SyntaxKind.PipePipeToken, BoundBinaryOperatorKind.LogicalOr, typeof(bool)),
        new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, typeof(bool)),
        new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, typeof(bool)),
        new(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, typeof(int)),
        new(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, typeof(int)),
        new(SyntaxKind.HatToken, BoundBinaryOperatorKind.BitwiseXor, typeof(int)),
        new(SyntaxKind.LessThanLessThanToken, BoundBinaryOperatorKind.BitwiseLeftShift, typeof(int)),
        new(SyntaxKind.GreaterThanGreaterThanToken, BoundBinaryOperatorKind.BitwiseRightShift, typeof(int)),
        new(SyntaxKind.GreaterThanOrEqualsToken, BoundBinaryOperatorKind.GreaterEquals, typeof(int)),
        new(SyntaxKind.LessThanOrEqualsToken, BoundBinaryOperatorKind.LessEquals, typeof(int))
    };

    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, Type type) :
        this(syntaxKind, kind, type, type, type) { }

    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, Type operandType,
        Type resultType) :
        this(syntaxKind, kind, operandType, operandType, resultType) { }

    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, Type leftType, Type rightType,
        Type type) {
        Type = type;
        SyntaxKind = syntaxKind;
        Kind = kind;
        LeftType = leftType;
        RightType = rightType;
    }

    public Type Type { get; }
    public SyntaxKind SyntaxKind { get; }
    public BoundBinaryOperatorKind Kind { get; }
    public Type LeftType { get; }
    public Type RightType { get; }

    public static BoundBinaryOperator? Bind(SyntaxKind syntaxKind, Type leftType, Type rightType) {
        foreach (var op in _operators)
            if (op.SyntaxKind == syntaxKind && op.LeftType == leftType && op.RightType == rightType)
                return op;

        return null;
    }
}