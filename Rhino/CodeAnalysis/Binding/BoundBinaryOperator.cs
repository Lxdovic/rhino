using Rhino.CodeAnalysis.Symbols;
using Rhino.CodeAnalysis.Syntax;

namespace Rhino.CodeAnalysis.Binding;

internal sealed class BoundBinaryOperator {
    private static readonly BoundBinaryOperator[] _operators = {
        new(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, TypeSymbol.Int),
        new(SyntaxKind.MinusToken, BoundBinaryOperatorKind.Subtraction, TypeSymbol.Int),
        new(SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication, TypeSymbol.Int),
        new(SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division, TypeSymbol.Int),
        new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.Int, TypeSymbol.Bool),
        new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.Int, TypeSymbol.Bool),
        new(SyntaxKind.LessToken, BoundBinaryOperatorKind.LessThan, TypeSymbol.Int, TypeSymbol.Bool),
        new(SyntaxKind.GreaterToken, BoundBinaryOperatorKind.GreaterThan, TypeSymbol.Int, TypeSymbol.Bool),
        new(SyntaxKind.GreaterThanOrEqualsToken, BoundBinaryOperatorKind.GreaterEquals, TypeSymbol.Int,
            TypeSymbol.Bool),
        new(SyntaxKind.LessThanOrEqualsToken, BoundBinaryOperatorKind.LessEquals, TypeSymbol.Int, TypeSymbol.Bool),
        new(SyntaxKind.AmpersandAmpersandToken, BoundBinaryOperatorKind.LogicalAnd, TypeSymbol.Bool),
        new(SyntaxKind.PipePipeToken, BoundBinaryOperatorKind.LogicalOr, TypeSymbol.Bool),
        new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.Bool),
        new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.Bool),
        new(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Int),
        new(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Int),
        new(SyntaxKind.HatToken, BoundBinaryOperatorKind.BitwiseXor, TypeSymbol.Int),
        new(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Bool),
        new(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Bool),
        new(SyntaxKind.HatToken, BoundBinaryOperatorKind.BitwiseXor, TypeSymbol.Bool),
        new(SyntaxKind.LessThanLessThanToken, BoundBinaryOperatorKind.BitwiseLeftShift, TypeSymbol.Int),
        new(SyntaxKind.GreaterThanGreaterThanToken, BoundBinaryOperatorKind.BitwiseRightShift, TypeSymbol.Int),
        new(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, TypeSymbol.String),
        new(SyntaxKind.ModuloToken, BoundBinaryOperatorKind.Modulus, TypeSymbol.Int),
        new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.String, TypeSymbol.Bool),
        new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.String, TypeSymbol.Bool),

        new(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, TypeSymbol.Float),
        new(SyntaxKind.MinusToken, BoundBinaryOperatorKind.Subtraction, TypeSymbol.Float),
        new(SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication, TypeSymbol.Float),
        new(SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division, TypeSymbol.Float),
        new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.Float, TypeSymbol.Bool),
        new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.Float, TypeSymbol.Bool),
        new(SyntaxKind.LessToken, BoundBinaryOperatorKind.LessThan, TypeSymbol.Float, TypeSymbol.Bool),
        new(SyntaxKind.GreaterToken, BoundBinaryOperatorKind.GreaterThan, TypeSymbol.Float, TypeSymbol.Bool),
        new(SyntaxKind.GreaterThanOrEqualsToken, BoundBinaryOperatorKind.GreaterEquals, TypeSymbol.Float,
            TypeSymbol.Bool),
        new(SyntaxKind.LessThanOrEqualsToken, BoundBinaryOperatorKind.LessEquals, TypeSymbol.Float, TypeSymbol.Bool),

        new(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, TypeSymbol.Double),
        new(SyntaxKind.MinusToken, BoundBinaryOperatorKind.Subtraction, TypeSymbol.Double),
        new(SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication, TypeSymbol.Double),
        new(SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division, TypeSymbol.Double),
        new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.Double, TypeSymbol.Bool),
        new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.Double, TypeSymbol.Bool),
        new(SyntaxKind.LessToken, BoundBinaryOperatorKind.LessThan, TypeSymbol.Double, TypeSymbol.Bool),
        new(SyntaxKind.GreaterToken, BoundBinaryOperatorKind.GreaterThan, TypeSymbol.Double, TypeSymbol.Bool),
        new(SyntaxKind.GreaterThanOrEqualsToken, BoundBinaryOperatorKind.GreaterEquals, TypeSymbol.Double,
            TypeSymbol.Bool),
        new(SyntaxKind.LessThanOrEqualsToken, BoundBinaryOperatorKind.LessEquals, TypeSymbol.Double, TypeSymbol.Bool)
    };

    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol type) :
        this(syntaxKind, kind, type, type, type) { }

    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol operandType,
        TypeSymbol resultType) :
        this(syntaxKind, kind, operandType, operandType, resultType) { }

    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol leftType,
        TypeSymbol rightType,
        TypeSymbol type) {
        Type = type;
        SyntaxKind = syntaxKind;
        Kind = kind;
        LeftType = leftType;
        RightType = rightType;
    }

    public TypeSymbol Type { get; }
    public SyntaxKind SyntaxKind { get; }
    public BoundBinaryOperatorKind Kind { get; }
    public TypeSymbol LeftType { get; }
    public TypeSymbol RightType { get; }

    public static BoundBinaryOperator? Bind(SyntaxKind syntaxKind, TypeSymbol leftType, TypeSymbol rightType) {
        foreach (var op in _operators)
            if (op.SyntaxKind == syntaxKind && op.LeftType == leftType && op.RightType == rightType)
                return op;

        return null;
    }
}