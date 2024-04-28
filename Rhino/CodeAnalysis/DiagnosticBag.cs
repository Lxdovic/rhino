using System.Collections;
using Rhino.CodeAnalysis.Symbols;
using Rhino.CodeAnalysis.Syntax;
using Rhino.CodeAnalysis.Text;

namespace Rhino.CodeAnalysis;

internal sealed class DiagnosticBag : IEnumerable<Diagnostic> {
    private readonly List<Diagnostic> _diagnostics = new();

    public IEnumerator<Diagnostic> GetEnumerator() {
        return _diagnostics.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    private void Report(TextLocation location, string message) {
        var diagnostic = new Diagnostic(location, message);
        _diagnostics.Add(diagnostic);
    }

    public void AddRange(DiagnosticBag diagnostics) {
        _diagnostics.AddRange(diagnostics._diagnostics);
    }

    public void ReportInvalidNumber(TextLocation location, string text, TypeSymbol type) {
        var message = $"ERROR: The number {text} isn't a valid {type}.";

        Report(location, message);
    }

    public void ReportBadCharacter(TextLocation location, char character) {
        var message = $"ERROR: bad character input: '{character}'";

        Report(location, message);
    }

    public void ReportUnexpectedToken(TextLocation location, SyntaxKind actualKind, SyntaxKind expectedKind) {
        var message = $"ERROR: unexpected token <{actualKind}>, expected <{expectedKind}>.";

        Report(location, message);
    }

    public void ReportUndefinedUnaryOperator(TextLocation location, string operatorText, TypeSymbol operandType) {
        var message = $"Unary operator '{operatorText}' is not defined for type <{operandType}>";

        Report(location, message);
    }

    public void ReportUndefinedBinaryOperator(TextLocation location, string operatorText, TypeSymbol leftType,
        TypeSymbol rightType) {
        var message = $"Binary operator '{operatorText}' is not defined for types <{leftType}> and <{rightType}>";

        Report(location, message);
    }

    public void ReportUndefinedName(TextLocation location, string name) {
        var message = $"ERROR: variable '{name}' doesn't exist.";

        Report(location, message);
    }

    public void ReportCannotConvert(TextLocation location, TypeSymbol fromType, TypeSymbol toType) {
        var message = $"ERROR: cannot convert type <{fromType}> to <{toType}>.";

        Report(location, message);
    }

    public void ReportSymbolAlreadyDeclared(TextLocation location, string name) {
        var message = $"ERROR: '{name}' is already declared.";

        Report(location, message);
    }

    public void ReportCannotAssign(TextLocation location, string name) {
        var message = $"ERROR: cannot assign to variable '{name}' because it is read-only.";

        Report(location, message);
    }

    public void ReportUnterminatedString(TextLocation location) {
        var message = "ERROR: unterminated string literal.";

        Report(location, message);
    }

    public void ReportWrongArgumentCount(TextLocation location, string name, int expectedCount, int actualCount) {
        var message = $"ERROR: function '{name}' requires {expectedCount} arguments, but was given {actualCount}.";

        Report(location, message);
    }

    public void ReportUndefinedFunction(TextLocation location, string? identifierText) {
        var message = $"ERROR: function '{identifierText}' doesn't exist.";

        Report(location, message);
    }

    public void ReportWrongArgumentType(TextLocation location, string name, TypeSymbol expectedType,
        TypeSymbol actualType) {
        var message =
            $"ERROR: parameter '{name}' requires a value of type <{expectedType}>, but was given a value of type <{actualType}>.";

        Report(location, message);
    }

    public void ReportExpressionMustHaveValue(TextLocation location) {
        var message = "ERROR: expression must have a value.";

        Report(location, message);
    }

    public void ReportUndefinedType(TextLocation location, string text) {
        var message = $"ERROR: type '{text}' doesn't exist.";

        Report(location, message);
    }

    public void ReportCannotConvertImplicitly(TextLocation location, TypeSymbol fromType, TypeSymbol toType) {
        var message =
            $"ERROR: cannot implicitly convert type <{fromType}> to <{toType}>. An explicit conversion exists. Are you missing a cast?";

        Report(location, message);
    }

    public void ReportParameterAlreadyDeclared(TextLocation location, string parameterName) {
        var message = $"ERROR: A parameter with the name '{parameterName}' already exists.";

        Report(location, message);
    }

    public void ReportInvalidBreakOrContinue(TextLocation location, string? text) {
        var message = $"ERROR: The keyword '{text}' can only be used inside of loops.";

        Report(location, message);
    }

    public void ReportInvalidReturn(TextLocation location) {
        var message = "ERROR: The 'return' keyword can only be used inside of functions.";

        Report(location, message);
    }

    public void ReportInvalidReturnExpression(TextLocation location, string functionName) {
        var message = $"ERROR: The return expression is not valid for function '{functionName}'.";

        Report(location, message);
    }

    public void ReportMissingReturnExpression(TextLocation location, string functionName) {
        var message = $"ERROR: The return expression is missing for function '{functionName}'.";

        Report(location, message);
    }

    public void ReportAllPathsMustReturn(TextLocation location, string functionName) {
        var message = $"ERROR: Not all code paths return a value for function '{functionName}'.";

        Report(location, message);
    }

    public void ReportNotAVariable(TextLocation location, string name) {
        var message = $"ERROR: '{name}' is not a variable.";

        Report(location, message);
    }

    public void ReportUndefinedVariable(TextLocation location, string name) {
        var message = $"ERROR: variable '{name}' doesn't exist.";

        Report(location, message);
    }

    public void ReportNotAFunction(TextLocation location, string text) {
        var message = $"ERROR: '{text}' is not a function.";

        Report(location, message);
    }
}