using Rhino.CodeAnalysis.Symbols;
using Rhino.CodeAnalysis.Syntax;

namespace Rhino.CodeAnalysis.Binding;

internal sealed class BoundUnaryOperator {
    private static readonly BoundUnaryOperator[] Operators = {
        new(SyntaxKind.BangToken, BoundUnaryOperatorKind.LogicalNegation, TypeSymbol.Bool),
        new(SyntaxKind.PlusToken, BoundUnaryOperatorKind.Identity, TypeSymbol.Int),
        new(SyntaxKind.MinusToken, BoundUnaryOperatorKind.Negation, TypeSymbol.Int),
        new(SyntaxKind.TildeToken, BoundUnaryOperatorKind.BitwiseNegation, TypeSymbol.Int),
        new(SyntaxKind.PlusToken, BoundUnaryOperatorKind.Identity, TypeSymbol.Float),
        new(SyntaxKind.MinusToken, BoundUnaryOperatorKind.Negation, TypeSymbol.Float),
        new(SyntaxKind.TildeToken, BoundUnaryOperatorKind.BitwiseNegation, TypeSymbol.Float)
    };

    private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, TypeSymbol operandType) : this(
        syntaxKind,
        kind, operandType, operandType) {
    }

    private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, TypeSymbol operandType,
        TypeSymbol type) {
        OperandType = operandType;
        Type = type;
        SyntaxKind = syntaxKind;
        Kind = kind;
    }

    public TypeSymbol OperandType { get; }
    public TypeSymbol Type { get; }
    public SyntaxKind SyntaxKind { get; }
    public BoundUnaryOperatorKind Kind { get; }

    public static BoundUnaryOperator? Bind(SyntaxKind syntaxKind, TypeSymbol operandType) {
        foreach (var op in Operators)
            if (op.SyntaxKind == syntaxKind && op.OperandType == operandType)
                return op;

        return null;
    }
}