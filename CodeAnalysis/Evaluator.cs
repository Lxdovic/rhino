using Rhino.CodeAnalysis.Binding;

namespace Rhino.CodeAnalysis;

internal sealed class Evaluator {
    private readonly BoundExpression _root;

    public Evaluator(BoundExpression root) {
        _root = root;
    }

    public object Evaluate() {
        return EvaluateExpression(_root);
    }

    private object EvaluateExpression(BoundExpression node) {
        if (node is BoundLitteralExpression n) return n.Value;

        if (node is BoundUnaryExpression u) {
            var operand = EvaluateExpression(u.Operand);

            if (u.Op.Kind == BoundUnaryOperatorKind.Identity) return (int)operand;
            if (u.Op.Kind == BoundUnaryOperatorKind.Negation) return -(int)operand;
            if (u.Op.Kind == BoundUnaryOperatorKind.LogicalNegation) return !(bool)operand;

            throw new Exception($"Unexpected unary operator <{u.Op.Kind}>");
        }

        if (node is BoundBinaryExpression b) {
            var left = EvaluateExpression(b.Left);
            var right = EvaluateExpression(b.Right);

            return b.Op.Kind switch {
                BoundBinaryOperatorKind.Addition => (int)left + (int)right,
                BoundBinaryOperatorKind.Subtraction => (int)left - (int)right,
                BoundBinaryOperatorKind.Multiplication => (int)left * (int)right,
                BoundBinaryOperatorKind.Division => (int)left / (int)right,
                BoundBinaryOperatorKind.LogicalAnd => (bool)left && (bool)right,
                BoundBinaryOperatorKind.LogicalOr => (bool)left || (bool)right,
                _ => throw new Exception($"Unexpected binary operator <{b.Op.Kind}>")
            };
        }

        throw new Exception($"Unexpected node <{node.Kind}>");
    }
}