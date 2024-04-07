using Rhino.CodeAnalysis.Binding;
using Rhino.CodeAnalysis.Symbols;

namespace Rhino.CodeAnalysis;

internal sealed class Evaluator {
    private readonly BoundBlockStatement _root;
    private readonly Dictionary<VariableSymbol, object> _variables;
    private object _lastValue;

    public Evaluator(BoundBlockStatement root, Dictionary<VariableSymbol, object> variables) {
        _root = root;
        _variables = variables;
    }

    public object Evaluate() {
        var labelToIndex = new Dictionary<BoundLabel, int>();

        for (var i = 0; i < _root.Statements.Length; i++)
            if (_root.Statements[i] is BoundLabelStatement l)
                labelToIndex.Add(l.Label, i + 1);

        var index = 0;

        while (index < _root.Statements.Length) {
            var statement = _root.Statements[index];

            switch (statement.Kind) {
                case BoundNodeKind.VariableDeclaration:
                    EvaluateVariableDeclaration((BoundVariableDeclaration)statement);

                    index++;
                    break;

                case BoundNodeKind.ExpressionStatement:
                    EvaluateExpressionStatement((BoundExpressionStatement)statement);

                    index++;
                    break;

                case BoundNodeKind.GotoStatement:
                    var gotoStatement = (BoundGotoStatement)statement;

                    index = labelToIndex[gotoStatement.Label];
                    break;

                case BoundNodeKind.ConditionalGotoStatement:
                    var conditionalGotoStatement = (BoundConditionalGotoStatement)statement;
                    var condition = (bool)EvaluateExpression(conditionalGotoStatement.Condition);

                    if (condition == conditionalGotoStatement.JumpIfTrue)
                        index = labelToIndex[conditionalGotoStatement.Label];

                    else index++;

                    break;

                case BoundNodeKind.LabelStatement:
                    index++;
                    break;

                default: throw new Exception($"Unexpected node <{statement.Kind}>");
            }
        }

        return _lastValue;
    }

    private void EvaluateVariableDeclaration(BoundVariableDeclaration node) {
        var value = EvaluateExpression(node.Initializer);

        _variables[node.Variable] = value;
        _lastValue = value;
    }

    private void EvaluateExpressionStatement(BoundExpressionStatement node) {
        _lastValue = EvaluateExpression(node.Expression);
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
                if (b.Type == TypeSymbol.Int) return (int)left + (int)right;

                return (string)left + (string)right;
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
                if (b.Type == TypeSymbol.Int) return (int)left & (int)right;

                return (bool)left & (bool)right;
            case BoundBinaryOperatorKind.BitwiseOr:
                if (b.Type == TypeSymbol.Int) return (int)left | (int)right;

                return (bool)left | (bool)right;
            case BoundBinaryOperatorKind.BitwiseXor:
                if (b.Type == TypeSymbol.Int) return (int)left ^ (int)right;

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