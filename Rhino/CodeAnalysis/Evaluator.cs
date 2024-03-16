using Rhino.CodeAnalysis.Binding;

namespace Rhino.CodeAnalysis;

internal sealed class Evaluator {
    private readonly BoundExpression _root;
    private readonly Dictionary<VariableSymbol, object> _variables;

    public Evaluator(BoundExpression root, Dictionary<VariableSymbol, object> variables) {
        _root = root;
        _variables = variables;
    }

    public object Evaluate() {
        return EvaluateExpression(_root);
    }

    private object EvaluateExpression(BoundExpression node) {
        return node.Kind switch {
            BoundNodeKind.LiteralExpression => EvaluateLiteralExpression((BoundLiteralExpression)node),
            BoundNodeKind.VariableExpression => EvaluateVariableExpression((BoundVariableExpression)node),
            BoundNodeKind.AssignmentExpression => EvaluateAssignmentExpression((BoundAssignmentExpression)node),
            BoundNodeKind.UnaryExpression => EvaluateUnaryExpression((BoundUnaryExpression)node),
            BoundNodeKind.BinaryExpression => EvaluateBinaryExpression((BoundBinaryExpression)node),
            _ => throw new Exception($"Unexpected node <{node.Kind}>")
        };
    }

    private object EvaluateBinaryExpression(BoundBinaryExpression b) {
        var left = EvaluateExpression(b.Left);
        var right = EvaluateExpression(b.Right);

        return b.Op.Kind switch {
            BoundBinaryOperatorKind.Addition => (int)left + (int)right,
            BoundBinaryOperatorKind.Subtraction => (int)left - (int)right,
            BoundBinaryOperatorKind.Multiplication => (int)left * (int)right,
            BoundBinaryOperatorKind.Division => (int)left / (int)right,
            BoundBinaryOperatorKind.LogicalAnd => (bool)left && (bool)right,
            BoundBinaryOperatorKind.LogicalOr => (bool)left || (bool)right,
            BoundBinaryOperatorKind.Equals => Equals(left, right),
            BoundBinaryOperatorKind.NotEquals => !Equals(left, right),
            BoundBinaryOperatorKind.BitwiseAnd => (int)left & (int)right,
            BoundBinaryOperatorKind.BitwiseOr => (int)left | (int)right,
            BoundBinaryOperatorKind.BitwiseXor => (int)left ^ (int)right,
            BoundBinaryOperatorKind.BitwiseLeftShift => (int)left << (int)right,
            BoundBinaryOperatorKind.BitwiseRightShift => (int)left >> (int)right,
            BoundBinaryOperatorKind.GreaterEquals => (int)left >= (int)right,
            BoundBinaryOperatorKind.LessEquals => (int)left <= (int)right,
            _ => throw new Exception($"Unexpected binary operator <{b.Op.Kind}>")
        };
    }

    private object EvaluateUnaryExpression(BoundUnaryExpression u) {
        var operand = EvaluateExpression(u.Operand);

        if (u.Op.Kind == BoundUnaryOperatorKind.Identity) return (int)operand;
        if (u.Op.Kind == BoundUnaryOperatorKind.Negation) return -(int)operand;
        if (u.Op.Kind == BoundUnaryOperatorKind.LogicalNegation) return !(bool)operand;
        if (u.Op.Kind == BoundUnaryOperatorKind.BitwiseNegation) return ~(int)operand;

        throw new Exception($"Unexpected unary operator <{u.Op.Kind}>");
    }

    private object EvaluateAssignmentExpression(BoundAssignmentExpression a) {
        var value = EvaluateExpression(a.Expression);

        _variables[a.Variable] = value;

        return value;
    }

    private object EvaluateVariableExpression(BoundVariableExpression v) {
        return _variables[v.Variable];
    }

    private static object EvaluateLiteralExpression(BoundLiteralExpression n) {
        return n.Value;
    }
}