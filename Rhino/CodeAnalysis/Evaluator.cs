using Rhino.CodeAnalysis.Binding;

namespace Rhino.CodeAnalysis;

internal sealed class Evaluator {
    private readonly BoundStatement _root;
    private readonly Dictionary<VariableSymbol, object> _variables;
    private object _lastValue;

    public Evaluator(BoundStatement root, Dictionary<VariableSymbol, object> variables) {
        _root = root;
        _variables = variables;
    }

    public object Evaluate() {
        EvaluateStatement(_root);

        return _lastValue;
    }

    private void EvaluateStatement(BoundStatement node) {
        switch (node.Kind) {
            case BoundNodeKind.BlockStatement:
                EvaluateBlockStatement((BoundBlockStatement)node);
                break;

            case BoundNodeKind.ExpressionStatement:
                EvaluateExpressionStatement((BoundExpressionStatement)node);
                break;

            case BoundNodeKind.VariableDeclaration:
                EvaluateVariableDeclaration((BoundVariableDeclaration)node);
                break;

            case BoundNodeKind.IfStatement:
                EvaluateIfStatement((BoundIfStatement)node);
                break;

            case BoundNodeKind.WhileStatement:
                EvaluateWhileStatement((BoundWhileStatement)node);
                break;

            case BoundNodeKind.ForStatement:
                EvaluateForStatement((BoundForStatement)node);
                break;

            default: throw new Exception($"Unexpected node <{node.Kind}>");
        }
    }

    private void EvaluateForStatement(BoundForStatement node) {
        var lowerBound = (int)EvaluateExpression(node.LowerBound);
        var upperBound = (int)EvaluateExpression(node.UpperBound);

        for (var i = lowerBound; i <= upperBound; i++) {
            _variables[node.Variable] = i;
            EvaluateStatement(node.Body);
        }
    }

    private void EvaluateWhileStatement(BoundWhileStatement node) {
        while ((bool)EvaluateExpression(node.Condition)) EvaluateStatement(node.Body);
    }

    private void EvaluateIfStatement(BoundIfStatement node) {
        var condition = (bool)EvaluateExpression(node.Condition);

        if (condition) EvaluateStatement(node.ThenStatement);
        else if (node.ElseStatement != null) EvaluateStatement(node.ElseStatement);
    }

    private void EvaluateVariableDeclaration(BoundVariableDeclaration node) {
        var value = EvaluateExpression(node.Initializer);

        _variables[node.Variable] = value;
        _lastValue = value;
    }

    private void EvaluateExpressionStatement(BoundExpressionStatement node) {
        _lastValue = EvaluateExpression(node.Expression);
    }

    private void EvaluateBlockStatement(BoundBlockStatement node) {
        foreach (var statement in node.Statements) EvaluateStatement(statement);
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

        switch (b.Op.Kind) {
            case BoundBinaryOperatorKind.Addition:
                return (int)left + (int)right;
            case BoundBinaryOperatorKind.Subtraction:
                return (int)left - (int)right;
            case BoundBinaryOperatorKind.Multiplication:
                return (int)left * (int)right;
            case BoundBinaryOperatorKind.Division:
                return (int)left / (int)right;
            case BoundBinaryOperatorKind.LogicalAnd:
                return (bool)left && (bool)right;
            case BoundBinaryOperatorKind.LogicalOr:
                return (bool)left || (bool)right;
            case BoundBinaryOperatorKind.Equals:
                return Equals(left, right);
            case BoundBinaryOperatorKind.NotEquals:
                return !Equals(left, right);
            case BoundBinaryOperatorKind.BitwiseAnd:
                if (b.Type == typeof(int)) return (int)left & (int)right;

                return (bool)left & (bool)right;
            case BoundBinaryOperatorKind.BitwiseOr:
                if (b.Type == typeof(int)) return (int)left | (int)right;

                return (bool)left | (bool)right;
            case BoundBinaryOperatorKind.BitwiseXor:
                if (b.Type == typeof(int)) return (int)left ^ (int)right;

                return (bool)left ^ (bool)right;
            case BoundBinaryOperatorKind.BitwiseLeftShift:
                return (int)left << (int)right;
            case BoundBinaryOperatorKind.BitwiseRightShift:
                return (int)left >> (int)right;
            case BoundBinaryOperatorKind.GreaterEquals:
                return (int)left >= (int)right;
            case BoundBinaryOperatorKind.LessEquals:
                return (int)left <= (int)right;
            case BoundBinaryOperatorKind.LessThan:
                return (int)left < (int)right;
            case BoundBinaryOperatorKind.GreaterThan:
                return (int)left > (int)right;
            default:
                throw new Exception($"Unexpected binary operator <{b.Op.Kind}>");
        }
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