using Rhino.CodeAnalysis.Binding;
using Rhino.CodeAnalysis.Symbols;

namespace Rhino.CodeAnalysis;

internal sealed class Evaluator {
    private readonly Dictionary<VariableSymbol, object> _globals;
    private readonly Stack<Dictionary<VariableSymbol, object>> _locals = new();
    private readonly BoundProgram _program;
    private readonly Random _random = new();
    private object _lastValue;

    public Evaluator(BoundProgram program, Dictionary<VariableSymbol, object> variables) {
        _program = program;
        _globals = variables;
        _locals.Push(new Dictionary<VariableSymbol, object>());
    }

    public object Evaluate() {
        return EvaluateStatement(_program.Statement);
    }

    private object EvaluateStatement(BoundBlockStatement body) {
        var labelToIndex = new Dictionary<BoundLabel, int>();

        for (var i = 0; i < body.Statements.Length; i++)
            if (body.Statements[i] is BoundLabelStatement l)
                labelToIndex.Add(l.Label, i + 1);

        var index = 0;

        while (index < body.Statements.Length) {
            var statement = body.Statements[index];

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

        _lastValue = value;

        Assign(node.Variable, value);
    }

    private void EvaluateExpressionStatement(BoundExpressionStatement node) {
        _lastValue = EvaluateExpression(node.Expression);
    }

    private object? EvaluateExpression(BoundExpression node) {
        return node.Kind switch {
            BoundNodeKind.LiteralExpression => EvaluateLiteralExpression((BoundLiteralExpression)node),
            BoundNodeKind.VariableExpression => EvaluateVariableExpression((BoundVariableExpression)node),
            BoundNodeKind.AssignmentExpression => EvaluateAssignmentExpression((BoundAssignmentExpression)node),
            BoundNodeKind.UnaryExpression => EvaluateUnaryExpression((BoundUnaryExpression)node),
            BoundNodeKind.BinaryExpression => EvaluateBinaryExpression((BoundBinaryExpression)node),
            BoundNodeKind.CallExpression => EvaluateCallExpression((BoundCallExpression)node),
            BoundNodeKind.ConversionExpression => EvaluateConversionExpression((BoundConversionExpression)node),
            _ => throw new Exception($"Unexpected node <{node.Kind}>")
        };
    }

    private object EvaluateConversionExpression(BoundConversionExpression node) {
        var value = EvaluateExpression(node.Expression);

        if (node.Type == TypeSymbol.Bool) return Convert.ToBoolean(value);
        if (node.Type == TypeSymbol.Int) return Convert.ToInt32(value);
        if (node.Type == TypeSymbol.Float) return Convert.ToSingle(value);
        if (node.Type == TypeSymbol.Double) return Convert.ToDouble(value);
        if (node.Type == TypeSymbol.String) return Convert.ToString(value);

        throw new Exception($"Unexpected type <{node.Type}>");
    }

    private object? EvaluateCallExpression(BoundCallExpression node) {
        return node.Function.Name switch {
            "input" => EvaluateInputFunction(node),
            "print" => EvaluatePrintFunction(node),
            "println" => EvaluatePrintLineFunction(node),
            "random" => EvaluateRandomFunction(node),
            "cos" => EvaluateCosFunction(node),
            "sin" => EvaluateSinFunction(node),
            "acos" => EvaluateAcosFunction(node),
            "floor" => EvaluateFloorFunction(node),
            _ => EvaluateFunction(node)
        };
    }

    private object? EvaluateInputFunction(BoundCallExpression node) {
        return Console.ReadLine();
    }


    private object EvaluateFunction(BoundCallExpression node) {
        var locals = new Dictionary<VariableSymbol, object>();

        for (var i = 0; i < node.Arguments.Length; i++) {
            var parameter = node.Function.Parameters[i];
            var value = EvaluateExpression(node.Arguments[i]);

            locals.Add(parameter, value);
        }

        _locals.Push(locals);

        var statement = _program.Functions[node.Function];
        var result = EvaluateStatement(statement);

        _locals.Pop();

        return result;
    }


    private float? EvaluateFloorFunction(BoundCallExpression node) {
        var value = EvaluateExpression(node.Arguments[0]);

        if (value == null) return null;


        return (float)Math.Floor((float)value);
    }

    private object EvaluateRandomFunction(BoundCallExpression node) {
        var min = EvaluateExpression(node.Arguments[0]);
        var max = EvaluateExpression(node.Arguments[1]);


        if (min == null || max == null) return null;

        return _random.Next((int)min, (int)max);
    }

    private float? EvaluateSinFunction(BoundCallExpression node) {
        var value = EvaluateExpression(node.Arguments[0]);

        if (value == null) return null;

        return (float)Math.Sin((float)value);
    }

    private float? EvaluateAcosFunction(BoundCallExpression node) {
        var value = EvaluateExpression(node.Arguments[0]);

        if (value == null) return null;

        return (float)Math.Acos((float)value);
    }

    private float? EvaluateCosFunction(BoundCallExpression node) {
        var value = EvaluateExpression(node.Arguments[0]);

        if (value == null) return null;

        return (float)Math.Cos((float)value);
    }

    private object? EvaluatePrintFunction(BoundCallExpression node) {
        var message = EvaluateExpression(node.Arguments[0]);

        if (message == null) return null;

        Console.Write((string)message);

        return null;
    }

    private object? EvaluatePrintLineFunction(BoundCallExpression node) {
        var message = EvaluateExpression(node.Arguments[0]);

        if (message == null) return null;

        Console.WriteLine((string)message);

        return null;
    }

    private object EvaluateBinaryExpression(BoundBinaryExpression b) {
        var left = EvaluateExpression(b.Left);
        var right = EvaluateExpression(b.Right);

        switch (b.Op.Kind) {
            case BoundBinaryOperatorKind.Addition:
                if (b.Type == TypeSymbol.Int) return (int)left + (int)right;
                if (b.Type == TypeSymbol.Float) return (float)left + (float)right;
                if (b.Type == TypeSymbol.Double) return (double)left + (double)right;

                return (string)left + (string)right;
            case BoundBinaryOperatorKind.Subtraction:
                if (b.Type == TypeSymbol.Float) return (float)left - (float)right;
                if (b.Type == TypeSymbol.Double) return (double)left - (double)right;

                return (int)left - (int)right;
            case BoundBinaryOperatorKind.Multiplication:
                if (b.Type == TypeSymbol.Float) return (float)left * (float)right;
                if (b.Type == TypeSymbol.Double) return (double)left * (double)right;

                return (int)left * (int)right;
            case BoundBinaryOperatorKind.Division:
                if (b.Type == TypeSymbol.Float) return (float)left / (float)right;
                if (b.Type == TypeSymbol.Double) return (double)left / (double)right;

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
                if (b.Type == TypeSymbol.Float) return (float)left >= (float)right;
                if (b.Type == TypeSymbol.Double) return (double)left >= (double)right;

                return (int)left >= (int)right;
            case BoundBinaryOperatorKind.LessEquals:
                if (b.Type == TypeSymbol.Float) return (float)left <= (float)right;
                if (b.Type == TypeSymbol.Double) return (double)left <= (double)right;

                return (int)left <= (int)right;
            case BoundBinaryOperatorKind.LessThan:
                if (b.Type == TypeSymbol.Float) return (float)left < (float)right;
                if (b.Type == TypeSymbol.Double) return (double)left < (double)right;

                return (int)left < (int)right;
            case BoundBinaryOperatorKind.GreaterThan:
                if (b.Type == TypeSymbol.Float) return (float)left > (float)right;
                if (b.Type == TypeSymbol.Double) return (double)left > (double)right;

                return (int)left > (int)right;
            case BoundBinaryOperatorKind.Modulus:
                return (int)left % (int)right;
            default:
                throw new Exception($"Unexpected binary operator <{b.Op.Kind}>");
        }
    }

    private object EvaluateUnaryExpression(BoundUnaryExpression u) {
        var operand = EvaluateExpression(u.Operand);

        switch (u.Op.Kind) {
            case BoundUnaryOperatorKind.Identity:
                if (u.Type == TypeSymbol.Float) return (float)operand;
                if (u.Type == TypeSymbol.Double) return (double)operand;

                return (int)operand;
            case BoundUnaryOperatorKind.Negation:
                if (u.Type == TypeSymbol.Float) return -(float)operand;
                if (u.Type == TypeSymbol.Double) return -(double)operand;

                return -(int)operand;
            case BoundUnaryOperatorKind.LogicalNegation:
                return !(bool)operand;
            case BoundUnaryOperatorKind.BitwiseNegation:
                return ~(int)operand;
            default:
                throw new Exception($"Unexpected unary operator <{u.Op.Kind}>");
        }
    }

    private object EvaluateAssignmentExpression(BoundAssignmentExpression a) {
        var value = EvaluateExpression(a.Expression);

        Assign(a.Variable, value);

        return value;
    }

    private object EvaluateVariableExpression(BoundVariableExpression v) {
        if (v.Variable.Kind == SymbolKind.GlobalVariable) return _globals[v.Variable];

        var locals = _locals.Peek();

        return locals[v.Variable];
    }

    private static object EvaluateLiteralExpression(BoundLiteralExpression n) {
        return n.Value;
    }

    private void Assign(VariableSymbol variable, object value) {
        if (variable.Kind == SymbolKind.GlobalVariable) {
            _globals[variable] = value;
        }

        else {
            var locals = _locals.Peek();
            locals[variable] = value;
        }
    }
}