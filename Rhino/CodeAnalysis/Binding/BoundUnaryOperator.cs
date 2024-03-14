using Rhino.CodeAnalysis.Syntax;

namespace Rhino.CodeAnalysis.Binding;

internal sealed class BoundUnaryOperator {
    private static readonly BoundUnaryOperator[] _operators = {
        new(SyntaxKind.BangToken, BoundUnaryOperatorKind.LogicalNegation, typeof(bool)),
        new(SyntaxKind.PlusToken, BoundUnaryOperatorKind.Identity, typeof(int)),
        new(SyntaxKind.MinusToken, BoundUnaryOperatorKind.Negation, typeof(int)),
        new(SyntaxKind.TildeToken, BoundUnaryOperatorKind.BitwiseNegation, typeof(int))
    };

    private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, Type operandType) : this(syntaxKind,
        kind, operandType, operandType) { }

    private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, Type operandType, Type type) {
        OperandType = operandType;
        Type = type;
        SyntaxKind = syntaxKind;
        Kind = kind;
    }

    public Type OperandType { get; }
    public Type Type { get; }
    public SyntaxKind SyntaxKind { get; }
    public BoundUnaryOperatorKind Kind { get; }

    public static BoundUnaryOperator Bind(SyntaxKind syntaxKind, Type operandType) {
        foreach (var op in _operators)
            if (op.SyntaxKind == syntaxKind && op.OperandType == operandType)
                return op;

        return null;
    }
}