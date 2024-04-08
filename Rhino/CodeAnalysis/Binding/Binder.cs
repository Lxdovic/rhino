using System.Collections.Immutable;
using Rhino.CodeAnalysis.Symbols;
using Rhino.CodeAnalysis.Syntax;

namespace Rhino.CodeAnalysis.Binding;

internal sealed class Binder {
    private BoundScope _scope;

    public Binder(BoundScope parent) {
        _scope = new BoundScope(parent);
    }

    public DiagnosticBag Diagnostics { get; } = new();

    public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, CompilationUnitSyntax syntax) {
        var parentScope = CreateParentScope(previous);
        var binder = new Binder(parentScope);
        var statement = binder.BindStatement(syntax.Statement);
        var variables = binder._scope.GetDeclaredVariables();
        var diagnostics = binder.Diagnostics.ToImmutableArray();

        if (previous != null) diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);

        return new BoundGlobalScope(previous, diagnostics, variables, statement);
    }

    private static BoundScope CreateParentScope(BoundGlobalScope previous) {
        var stack = new Stack<BoundGlobalScope>();

        while (previous != null) {
            stack.Push(previous);
            previous = previous.Previous;
        }

        var parent = CreateRootScope();

        while (stack.Count > 0) {
            previous = stack.Pop();
            var scope = new BoundScope(parent);

            foreach (var variable in previous.Variables) scope.TryDeclareVariable(variable);

            parent = scope;
        }

        return parent;
    }

    private static BoundScope CreateRootScope() {
        var result = new BoundScope(null);

        foreach (var function in BuiltinFunctions.GetAll()) result.TryDeclareFunction(function);

        return result;
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
            case SyntaxKind.ForStatement:
                return BindForStatement((ForStatementSyntax)syntax);

            default:
                throw new Exception($"Unexpected syntax <{syntax.Kind}>");
        }
    }

    private BoundStatement BindForStatement(ForStatementSyntax syntax) {
        var lowerBound = BindExpression(syntax.LowerBound, TypeSymbol.Int);
        var upperBound = BindExpression(syntax.UpperBound, TypeSymbol.Int);

        _scope = new BoundScope(_scope);

        var variable = BindVariable(syntax.Identifier, true, TypeSymbol.Int);
        var body = BindStatement(syntax.Body);

        _scope = _scope.Parent;

        return new BoundForStatement(variable, lowerBound, upperBound, body);
    }

    private BoundStatement BindWhileStatement(WhileStatementSyntax syntax) {
        var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
        var body = BindStatement(syntax.Body);

        return new BoundWhileStatement(condition, body);
    }

    private BoundStatement BindIfStatement(IfStatementSyntax syntax) {
        var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
        var thenStatement = BindStatement(syntax.ThenStatement);
        var elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.ElseStatement);

        return new BoundIfStatement(condition, thenStatement, elseStatement);
    }

    private BoundStatement BindVariableDeclaration(VariableDeclarationSyntax syntax) {
        var initializer = BindExpression(syntax.Initializer);
        var isReadOnly = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
        var variable = BindVariable(syntax.Identifier, isReadOnly, initializer.Type);

        return new BoundVariableDeclaration(variable, initializer);
    }

    private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax) {
        var expression = BindExpression(syntax.Expression, true);

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

        if (!_scope.TryLookupVariable(name, out var variable)) {
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

    public BoundExpression BindExpression(ExpressionSyntax syntax, TypeSymbol targetType) {
        var result = BindExpression(syntax);

        if (targetType != TypeSymbol.Error &&
            result.Type != TypeSymbol.Error &&
            result.Type != targetType) Diagnostics.ReportCannotConvert(syntax.Span, result.Type, targetType);

        return result;
    }

    public BoundExpression BindExpression(ExpressionSyntax syntax, bool canBeVoid = false) {
        var result = BindExpressionInternal(syntax);

        if (!canBeVoid && result.Type == TypeSymbol.Void) {
            Diagnostics.ReportExpressionMustHaveValue(syntax.Span);
            return new BoundErrorExpression();
        }

        return result;
    }

    public BoundExpression BindExpressionInternal(ExpressionSyntax syntax) {
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
            case SyntaxKind.CallExpression:
                return BindCallExpression((CallExpressionSyntax)syntax);
            default:
                throw new Exception($"Unexpected syntax <{syntax.Kind}>");
        }
    }

    private BoundExpression BindCallExpression(CallExpressionSyntax syntax) {
        if (syntax.Arguments.Count == 1 && LookupType(syntax.Identifier.Text) is TypeSymbol type)
            return BindConversion(type, syntax.Arguments[0]);

        var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

        foreach (var argumentSyntax in syntax.Arguments) {
            var boundArgument = BindExpression(argumentSyntax);
            boundArguments.Add(boundArgument);
        }

        if (!_scope.TryLookupFunction(syntax.Identifier.Text, out var function)) {
            Diagnostics.ReportUndefinedFunction(syntax.Identifier.Span, syntax.Identifier.Text);
            return new BoundErrorExpression();
        }

        if (syntax.Arguments.Count != function.Parameters.Length) {
            Diagnostics.ReportWrongArgumentCount(syntax.Span, function.Name, function.Parameters.Length,
                syntax.Arguments.Count);
            return new BoundErrorExpression();
        }

        for (var i = 0; i < syntax.Arguments.Count; i++) {
            var argument = boundArguments[i];
            var parameter = function.Parameters[i];

            if (argument.Type != parameter.Type) {
                Diagnostics.ReportWrongArgumentType(syntax.Span, parameter.Name, parameter.Type, argument.Type);
                return new BoundErrorExpression();
            }
        }

        return new BoundCallExpression(function, boundArguments.ToImmutable());
    }

    private BoundExpression BindConversion(TypeSymbol type, ExpressionSyntax syntax) {
        var expression = BindExpression(syntax);
        var conversion = Conversion.Classify(expression.Type, type);

        if (!conversion.Exists) {
            Diagnostics.ReportCannotConvert(syntax.Span, expression.Type, type);
            return new BoundErrorExpression();
        }

        return new BoundConversionExpression(type, expression);
    }

    private BoundExpression BindNameExpression(NameExpressionSyntax syntax) {
        var name = syntax.IdentifierToken.Text;

        if (syntax.IdentifierToken.IsMissing) return new BoundErrorExpression();

        if (!_scope.TryLookupVariable(name, out var variable)) {
            Diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);

            return new BoundErrorExpression();
        }

        return new BoundVariableExpression(variable);
    }

    private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax) {
        return BindExpression(syntax.Expression);
    }

    private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax) {
        var boundLeft = BindExpression(syntax.Left);
        var boundRight = BindExpression(syntax.Right);

        if (boundLeft.Type == TypeSymbol.Error || boundRight.Type == TypeSymbol.Error)
            return new BoundErrorExpression();

        var boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);

        if (boundOperator == null) {
            Diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text,
                boundLeft.Type, boundRight.Type);

            return new BoundErrorExpression();
        }

        return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
    }

    private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax) {
        var boundOperand = BindExpression(syntax.Operand);

        if (boundOperand.Type == TypeSymbol.Error)
            return new BoundErrorExpression();

        var boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);

        if (boundOperator == null) {
            Diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text,
                boundOperand.Type);

            return new BoundErrorExpression();
        }

        return new BoundUnaryExpression(boundOperator, boundOperand);
    }

    private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax) {
        var value = syntax.Value ?? 0;

        return new BoundLiteralExpression(value);
    }

    private VariableSymbol BindVariable(SyntaxToken identifier, bool isReadOnly, TypeSymbol type) {
        var name = identifier.Text ?? "?";
        var declare = !identifier.IsMissing;
        var variable = new VariableSymbol(name, isReadOnly, type);

        // should never happen because we just declared a new scope and it has no variables
        if (declare && !_scope.TryDeclareVariable(variable))
            Diagnostics.ReportSymbolAlreadyDeclared(identifier.Span, name);
        return variable;
    }

    private TypeSymbol LookupType(string name) {
        switch (name) {
            case "bool":
                return TypeSymbol.Bool;
            case "int":
                return TypeSymbol.Int;
            case "string":
                return TypeSymbol.String;
            default:
                return null;
        }
    }
}