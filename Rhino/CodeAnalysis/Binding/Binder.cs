using System.Collections.Immutable;
using Rhino.CodeAnalysis.Lowering;
using Rhino.CodeAnalysis.Symbols;
using Rhino.CodeAnalysis.Syntax;
using Rhino.CodeAnalysis.Text;

namespace Rhino.CodeAnalysis.Binding;

internal sealed class Binder {
    private readonly FunctionSymbol _function;
    private readonly Stack<(BoundLabel breakLabel, BoundLabel continueLabel)> _loopStack = new();
    private int _labelCounter;
    private BoundScope _scope;

    public Binder(BoundScope parent, FunctionSymbol? function) {
        _function = function;
        _scope = new BoundScope(parent);

        if (function != null)
            foreach (var parameter in function.Parameters)
                _scope.TryDeclareVariable(parameter);
    }

    public DiagnosticBag Diagnostics { get; } = new();

    public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, CompilationUnitSyntax syntax) {
        var parentScope = CreateParentScope(previous);
        var binder = new Binder(parentScope, null);

        foreach (var function in syntax.Members.OfType<FunctionDeclarationSyntax>())
            binder.BindFunctionDeclaration(function);

        var statements = ImmutableArray.CreateBuilder<BoundStatement>();

        foreach (var globalStatement in syntax.Members.OfType<GlobalStatementSyntax>()) {
            var s = binder.BindStatement(globalStatement.Statement);
            statements.Add(s);
        }

        var functions = binder._scope.GetDeclaredFunctions();
        var variables = binder._scope.GetDeclaredVariables();
        var diagnostics = binder.Diagnostics.ToImmutableArray();

        if (previous != null) diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);

        return new BoundGlobalScope(previous, diagnostics, functions, variables, statements.ToImmutable());
    }

    public static BoundProgram BindProgram(BoundGlobalScope globalScope) {
        var parentScope = CreateParentScope(globalScope);

        var functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

        var scope = globalScope;

        while (scope != null) {
            foreach (var function in scope.Functions) {
                var binder = new Binder(parentScope, function);
                var body = binder.BindStatement(function.Declaration.Body);
                var loweredBody = Lowerer.Lower(body);

                if (function.ReturnType != TypeSymbol.Void && !ControlFlowGraph.AllPathsReturn(loweredBody))
                    binder.Diagnostics.ReportAllPathsMustReturn(function.Declaration.Identifier.Location,
                        function.Name);

                functionBodies.Add(function, loweredBody);

                diagnostics.AddRange(binder.Diagnostics);
            }

            scope = scope.Previous;
        }

        var statement = Lowerer.Lower(new BoundBlockStatement(globalScope.Statements));

        return new BoundProgram(diagnostics.ToImmutable(), functionBodies.ToImmutable(), statement);
    }

    private void BindFunctionDeclaration(FunctionDeclarationSyntax syntax) {
        var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();

        var seenParameterNames = new HashSet<string>();

        foreach (var parameterSyntax in syntax.Parameters) {
            var parameterName = parameterSyntax.Identifier.Text;
            var parameterType = BindTypeClause(parameterSyntax.Type);

            if (!seenParameterNames.Add(parameterName)) {
                Diagnostics.ReportParameterAlreadyDeclared(parameterSyntax.Location, parameterName);
            }

            else {
                var parameter = new ParameterSymbol(parameterName, parameterType);
                parameters.Add(parameter);
            }
        }

        var type = BindTypeClause(syntax.Type) ?? TypeSymbol.Void;
        var function = new FunctionSymbol(syntax.Identifier.Text, parameters.ToImmutable(), type, syntax);

        if (function.Declaration.Identifier.Text != null && !_scope.TryDeclareFunction(function))
            Diagnostics.ReportSymbolAlreadyDeclared(syntax.Identifier.Location, function.Name);
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

            foreach (var function in previous.Functions) scope.TryDeclareFunction(function);
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
            case SyntaxKind.BreakStatement:
                return BindBreakStatement((BreakStatementSyntax)syntax);
            case SyntaxKind.ContinueStatement:
                return BindContinueStatement((ContinueStatementSyntax)syntax);
            case SyntaxKind.ReturnStatement:
                return BindReturnStatement((ReturnStatementSyntax)syntax);

            default:
                throw new Exception($"Unexpected syntax <{syntax.Kind}>");
        }
    }

    private BoundStatement BindReturnStatement(ReturnStatementSyntax syntax) {
        var expression = syntax.Expression == null ? null : BindExpression(syntax.Expression);

        if (_function == null) {
            Diagnostics.ReportInvalidReturn(syntax.ReturnKeyword.Location);
        }

        else {
            if (_function.ReturnType == TypeSymbol.Void) {
                if (expression != null)
                    Diagnostics.ReportInvalidReturnExpression(syntax.Expression.Location, _function.Name);
            }

            else {
                if (expression == null)
                    Diagnostics.ReportMissingReturnExpression(syntax.ReturnKeyword.Location, _function.Name);

                else expression = BindConversion(syntax.Expression.Location, expression, _function.ReturnType);
            }
        }

        return new BoundReturnStatement(expression);
    }

    private BoundStatement BindContinueStatement(ContinueStatementSyntax syntax) {
        if (_loopStack.Count == 0) {
            Diagnostics.ReportInvalidBreakOrContinue(syntax.Keyword.Location, syntax.Keyword.Text);
            return BindErrorStatement();
        }

        var continueLabel = _loopStack.Peek().continueLabel;

        return new BoundGotoStatement(continueLabel);
    }

    private BoundStatement BindBreakStatement(BreakStatementSyntax syntax) {
        if (_loopStack.Count == 0) {
            Diagnostics.ReportInvalidBreakOrContinue(syntax.Keyword.Location, syntax.Keyword.Text);
            return BindErrorStatement();
        }

        var breakLabel = _loopStack.Peek().breakLabel;

        return new BoundGotoStatement(breakLabel);
    }

    private BoundStatement BindErrorStatement() {
        return new BoundExpressionStatement(new BoundErrorExpression());
    }

    private BoundStatement BindForStatement(ForStatementSyntax syntax) {
        var lowerBound = BindExpression(syntax.LowerBound, TypeSymbol.Int);
        var upperBound = BindExpression(syntax.UpperBound, TypeSymbol.Int);

        _scope = new BoundScope(_scope);

        var variable = BindVariableDeclaration(syntax.Identifier, true, TypeSymbol.Int);
        var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);

        _scope = _scope.Parent;

        return new BoundForStatement(variable, lowerBound, upperBound, body, breakLabel, continueLabel);
    }

    private BoundStatement BindWhileStatement(WhileStatementSyntax syntax) {
        var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
        var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);

        return new BoundWhileStatement(condition, body, breakLabel, continueLabel);
    }

    private BoundStatement BindLoopBody(StatementSyntax syntax, out BoundLabel breakLabel,
        out BoundLabel continueLabel) {
        _labelCounter++;
        breakLabel = new BoundLabel($"break{_labelCounter}");
        continueLabel = new BoundLabel($"continue{_labelCounter}");

        _loopStack.Push((breakLabel, continueLabel));
        var boundBody = BindStatement(syntax);
        _loopStack.Pop();

        return boundBody;
    }

    private BoundStatement BindIfStatement(IfStatementSyntax syntax) {
        var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
        var thenStatement = BindStatement(syntax.ThenStatement);
        var elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.ElseStatement);

        return new BoundIfStatement(condition, thenStatement, elseStatement);
    }

    private BoundStatement BindVariableDeclaration(VariableDeclarationSyntax syntax) {
        var initializer = BindExpression(syntax.Initializer);
        var type = BindTypeClause(syntax.TypeClause);
        var variableType = type ?? initializer.Type;
        var isReadOnly = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
        var variable = BindVariableDeclaration(syntax.Identifier, isReadOnly, initializer.Type);
        var convertedInitializer = BindConversion(syntax.Initializer.Location, initializer, variableType);

        return new BoundVariableDeclaration(variable, convertedInitializer);
    }

    private TypeSymbol BindTypeClause(TypeClauseSyntax syntax) {
        if (syntax == null) return null;

        var type = LookupType(syntax.Identifier.Text);

        if (type == null) Diagnostics.ReportUndefinedType(syntax.Identifier.Location, syntax.Identifier.Text);

        return type;
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

        var variable = BindVariableReference(syntax.IdentifierToken);
        if (variable == null) return boundExpression;

        if (variable.IsReadOnly) Diagnostics.ReportCannotAssign(syntax.EqualsToken.Location, name);

        var convertedExpression = BindConversion(syntax.Expression.Location, boundExpression, variable.Type);

        return new BoundAssignmentExpression(variable, convertedExpression);
    }

    public BoundExpression BindExpression(ExpressionSyntax syntax, TypeSymbol targetType) {
        return BindConversion(syntax, targetType);
    }

    public BoundExpression BindExpression(ExpressionSyntax syntax, bool canBeVoid = false) {
        var result = BindExpressionInternal(syntax);

        if (!canBeVoid && result.Type == TypeSymbol.Void) {
            Diagnostics.ReportExpressionMustHaveValue(syntax.Location);
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
            return BindConversion(syntax.Arguments[0], type, true);

        var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

        foreach (var argument in syntax.Arguments) {
            var boundArgument = BindExpression(argument);
            boundArguments.Add(boundArgument);
        }


        var symbol = _scope.TryLookupSymbol(syntax.Identifier.Text);
        if (symbol == null) {
            Diagnostics.ReportUndefinedFunction(syntax.Identifier.Location, syntax.Identifier.Text);
            return new BoundErrorExpression();
        }

        var function = symbol as FunctionSymbol;
        if (function == null) {
            Diagnostics.ReportNotAFunction(syntax.Identifier.Location, syntax.Identifier.Text);
            return new BoundErrorExpression();
        }

        if (syntax.Arguments.Count != function.Parameters.Length) {
            TextSpan span;
            if (syntax.Arguments.Count > function.Parameters.Length) {
                SyntaxNode firstExceedingNode;
                if (function.Parameters.Length > 0)
                    firstExceedingNode = syntax.Arguments.GetSeparator(function.Parameters.Length - 1);

                else
                    firstExceedingNode = syntax.Arguments[0];

                var lastExceedingArgument = syntax.Arguments[syntax.Arguments.Count - 1];

                span = TextSpan.FromBounds(firstExceedingNode.Span.Start, lastExceedingArgument.Span.End);
            }

            else {
                span = syntax.CloseParenthesissToken.Span;
            }

            var location = new TextLocation(syntax.SyntaxTree.Text, span);

            Diagnostics.ReportWrongArgumentCount(location, function.Name, function.Parameters.Length,
                syntax.Arguments.Count);

            return new BoundErrorExpression();
        }

        var hasErrors = false;

        for (var i = 0; i < syntax.Arguments.Count; i++) {
            var argument = boundArguments[i];
            var parameter = function.Parameters[i];

            if (argument.Type != parameter.Type) {
                if (argument.Type != TypeSymbol.Error)
                    Diagnostics.ReportWrongArgumentType(syntax.Arguments[i].Location, parameter.Name, parameter.Type,
                        argument.Type);

                hasErrors = true;
            }
        }

        if (hasErrors)
            return new BoundErrorExpression();

        return new BoundCallExpression(function, boundArguments.ToImmutable());
    }

    private BoundExpression BindConversion(ExpressionSyntax syntax, TypeSymbol type, bool allowExplicit = false) {
        var expression = BindExpression(syntax);

        return BindConversion(syntax.Location, expression, type, allowExplicit);
    }

    private BoundExpression BindConversion(TextLocation diagnosticLocation, BoundExpression expression, TypeSymbol type,
        bool allowExplicit = false) {
        var conversion = Conversion.Classify(expression.Type, type);

        if (!conversion.Exists) {
            if (expression.Type != TypeSymbol.Error && type != TypeSymbol.Error)
                Diagnostics.ReportCannotConvert(diagnosticLocation, expression.Type, type);

            return new BoundErrorExpression();
        }

        if (!allowExplicit && conversion.IsExplicit)
            Diagnostics.ReportCannotConvertImplicitly(diagnosticLocation, expression.Type, type);

        if (conversion.IsIdentity) return expression;

        return new BoundConversionExpression(type, expression);
    }

    private BoundExpression BindNameExpression(NameExpressionSyntax syntax) {
        var name = syntax.IdentifierToken.Text;

        if (syntax.IdentifierToken.IsMissing) return new BoundErrorExpression();

        var variable = BindVariableReference(syntax.IdentifierToken);
        if (variable == null)

            return new BoundErrorExpression();

        return new BoundVariableExpression(variable);
    }

    private VariableSymbol BindVariableReference(SyntaxToken identifier) {
        var name = identifier.Text;
        switch (_scope.TryLookupSymbol(name)) {
            case VariableSymbol variable:
                return variable;

            case null:
                Diagnostics.ReportUndefinedVariable(identifier.Location, name);
                return null;

            default:
                Diagnostics.ReportNotAVariable(identifier.Location, name);
                return null;
        }
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
            Diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Location, syntax.OperatorToken.Text,
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
            Diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Location, syntax.OperatorToken.Text,
                boundOperand.Type);

            return new BoundErrorExpression();
        }

        return new BoundUnaryExpression(boundOperator, boundOperand);
    }

    private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax) {
        var value = syntax.Value ?? 0;

        return new BoundLiteralExpression(value);
    }

    private VariableSymbol BindVariableDeclaration(SyntaxToken identifier, bool isReadOnly, TypeSymbol type) {
        var name = identifier.Text ?? "?";
        var declare = !identifier.IsMissing;
        var variable = _function == null
            ? (VariableSymbol)new GlobalVariableSymbol(name, isReadOnly, type)
            : new LocalVariableSymbol(name, isReadOnly, type);

        // should never happen because we just declared a new scope and it has no variables
        if (declare && !_scope.TryDeclareVariable(variable))
            Diagnostics.ReportSymbolAlreadyDeclared(identifier.Location, name);

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
            case "float":
                return TypeSymbol.Float;
            case "double":
                return TypeSymbol.Double;
            default:
                return null;
        }
    }
}