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
            var operand = (int)EvaluateExpression(u.Operand);

            if (u.OperatorKind == BoundUnaryOperatorKind.Identity) return operand;
            if (u.OperatorKind == BoundUnaryOperatorKind.Negation) return -operand;

            throw new Exception($"Unexpected unary operator <{u.OperatorKind}>");
        }

        if (node is BoundBinaryExpression b) {
            var left = (int)EvaluateExpression(b.Left);
            var right = (int)EvaluateExpression(b.Right);

            return b.OperatorKind switch {
                BoundBinaryOperatorKind.Addition => left + right,
                BoundBinaryOperatorKind.Subtraction => left - right,
                BoundBinaryOperatorKind.Multiplication => left * right,
                BoundBinaryOperatorKind.Division => left / right,
                _ => throw new Exception($"Unexpected binary operator <{b.OperatorKind}>")
            };
        }

        throw new Exception($"Unexpected node <{node.Kind}>");
    }
}