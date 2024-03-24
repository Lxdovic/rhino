using System.Collections.Immutable;
using Rhino.CodeAnalysis.Syntax;

namespace Rhino.CodeAnalysis.Binding;

internal sealed class Binder {
    private BoundScope _scope;

    public Binder(BoundScope parent) {
        _scope = new BoundScope(parent);
    }

    public DiagnosticBag Diagnostics { get; } = new();

    public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, CompilationUnitSyntax syntax) {
        var parentScope = CreateParentScopes(previous);
        var binder = new Binder(parentScope);
        var statement = binder.BindStatement(syntax.Statement);
        var variables = binder._scope.GetDeclaredVariables();
        var diagnostics = binder.Diagnostics.ToImmutableArray();

        if (previous != null) diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);

        return new BoundGlobalScope(previous, diagnostics, variables, statement);
    }

    private static BoundScope CreateParentScopes(BoundGlobalScope previous) {
        var stack = new Stack<BoundGlobalScope>();

        while (previous != null) {
            stack.Push(previous);
            previous = previous.Previous;
        }

        BoundScope parent = null;

        while (stack.Count > 0) {
            previous = stack.Pop();
            var scope = new BoundScope(parent);

            foreach (var variable in previous.Variables) scope.TryDeclare(variable);

            parent = scope;
        }

        return parent;
    }

    private BoundStatement BindStatement(StatementSyntax syntax) {
        switch (syntax.Kind) {
            case SyntaxKind.BlockStatement:
                return BindBlockStatement((BlockStatementSyntax)syntax);
            case SyntaxKind.ExpressionStatement:
                return BindExpressionStatement((ExpressionStatementSyntax)syntax);
            case SyntaxKind.VariableDeclaration:
                return BindVariableDeclaration((VariableDeclarationSyntax)syntax);
            case SyntaxKind.IfStatement:
                return BindIfStatement((IfStatementSyntax)syntax);
            case SyntaxKind.WhileStatement:
                return BindWhileStatement((WhileStatementSyntax)syntax);
            default:
                throw new Exception($"Unexpected syntax <{syntax.Kind}>");
        }
    }

    private BoundStatement BindWhileStatement(WhileStatementSyntax syntax) {
        var condition = BindExpression(syntax.Condition, typeof(bool));
        var body = BindStatement(syntax.Body);

        return new BoundWhileStatement(condition, body);
    }

    private BoundStatement BindIfStatement(IfStatementSyntax syntax) {
        var condition = BindExpression(syntax.Condition, typeof(bool));
        var thenStatement = BindStatement(syntax.ThenStatement);
        var elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.ElseStatement);

        return new BoundIfStatement(condition, thenStatement, elseStatement);
    }

    private BoundStatement BindVariableDeclaration(VariableDeclarationSyntax syntax) {
        var name = syntax.Identifier.Text;
        var isReadOnly = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
        var initializer = BindExpression(syntax.Initializer);
        var variable = new VariableSymbol(name, isReadOnly, initializer.Type);

        if (!_scope.TryDeclare(variable)) Diagnostics.ReportVariableAlreadyDeclared(syntax.Identifier.Span, name);

        return new BoundVariableDeclaration(variable, initializer);
    }

    private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax) {
        var expression = BindExpression(syntax.Expression);

        return new BoundExpressionStatement(expression);
    }

    private BoundStatement BindBlockStatement(BlockStatementSyntax syntax) {
        var statements = ImmutableArray.CreateBuilder<BoundStatement>();

        _scope = new BoundScope(_scope);

        foreach (var statementSyntax in syntax.Statements) {
            var statement = BindStatement(statementSyntax);
            statements.Add(statement);
        }

        _scope = _scope.Parent;

        return new BoundBlockStatement(statements.ToImmutable());
    }

    private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax) {
        var boundExpression = BindExpression(syntax.Expression);
        var name = syntax.IdentifierToken.Text;

        if (!_scope.TryLookup(name, out var variable)) {
            Diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);

            return boundExpression;
        }

        if (variable.IsReadOnly) {
            Diagnostics.ReportCannotAssign(syntax.EqualsToken.Span, name);

            return boundExpression;
        }

        if (boundExpression.Type != variable.Type) {
            Diagnostics.ReportCannotConvert(syntax.Expression.Span, boundExpression.Type, variable.Type);
            return boundExpression;
        }

        return new BoundAssignmentExpression(variable, boundExpression);
    }

    public BoundExpression BindExpression(ExpressionSyntax syntax, Type targetType) {
        var result = BindExpression(syntax);

        if (result.Type != targetType) Diagnostics.ReportCannotConvert(syntax.Span, result.Type, targetType);

        return result;
    }

    public BoundExpression BindExpression(ExpressionSyntax syntax) {
        switch (syntax.Kind) {
            case SyntaxKind.LiteralExpression:
                return BindLiteralExpression((LiteralExpressionSyntax)syntax);
            case SyntaxKind.UnaryExpression:
                return BindUnaryExpression((UnaryExpressionSyntax)syntax);
            case SyntaxKind.BinaryExpression:
                return BindBinaryExpression((BinaryExpressionSyntax)syntax);
            case SyntaxKind.ParenthesizedExpression:
                return BindParenthesizedExpression((ParenthesizedExpressionSyntax)syntax);
            case SyntaxKind.NameExpression:
                return BindNameExpression((NameExpressionSyntax)syntax);
            case SyntaxKind.AssignmentExpression:
                return BindAssignmentExpression((AssignmentExpressionSyntax)syntax);
            default:
                throw new Exception($"Unexpected syntax <{syntax.Kind}>");
        }
    }

    private BoundExpression BindNameExpression(NameExpressionSyntax syntax) {
        var name = syntax.IdentifierToken.Text;

        if (!_scope.TryLookup(name, out var variable)) {
            Diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);

            return new BoundLiteralExpression(0);
        }

        return new BoundVariableExpression(variable);
    }

    private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax) {
        return BindExpression(syntax.Expression);
    }

    private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax) {
        var boundLeft = BindExpression(syntax.Left);
        var boundRight = BindExpression(syntax.Right);
        var boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);

        if (boundOperator == null) {
            Diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text,
                boundLeft.Type, boundRight.Type);

            return boundLeft;
        }

        return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
    }

    private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax) {
        var boundOperand = BindExpression(syntax.Operand);
        var boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);

        if (boundOperator == null) {
            Diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text,
                boundOperand.Type);

            return boundOperand;
        }

        return new BoundUnaryExpression(boundOperator, boundOperand);
    }

    private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax) {
        var value = syntax.Value ?? 0;

        return new BoundLiteralExpression(value);
    }
}