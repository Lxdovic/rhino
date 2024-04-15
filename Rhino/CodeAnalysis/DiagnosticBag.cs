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

    private void Report(TextSpan span, string message) {
        var diagnostic = new Diagnostic(span, message);
        _diagnostics.Add(diagnostic);
    }

    public void AddRange(DiagnosticBag diagnostics) {
        _diagnostics.AddRange(diagnostics._diagnostics);
    }

    public void ReportInvalidNumber(TextSpan span, string text, TypeSymbol type) {
        var message = $"ERROR: The number {text} isn't a valid {type}.";

        Report(span, message);
    }

    public void ReportBadCharacter(int position, char character) {
        var message = $"ERROR: bad character input: '{character}'";
        var span = new TextSpan(position, 1);

        Report(span, message);
    }

    public void ReportUnexpectedToken(TextSpan span, SyntaxKind actualKind, SyntaxKind expectedKind) {
        var message = $"ERROR: unexpected token <{actualKind}>, expected <{expectedKind}>.";

        Report(span, message);
    }

    public void ReportUndefinedUnaryOperator(TextSpan span, string operatorText, TypeSymbol operandType) {
        var message = $"Unary operator '{operatorText}' is not defined for type <{operandType}>";

        Report(span, message);
    }

    public void ReportUndefinedBinaryOperator(TextSpan span, string operatorText, TypeSymbol leftType,
        TypeSymbol rightType) {
        var message = $"Binary operator '{operatorText}' is not defined for types <{leftType}> and <{rightType}>";

        Report(span, message);
    }

    public void ReportUndefinedName(TextSpan span, string name) {
        var message = $"ERROR: variable '{name}' doesn't exist.";

        Report(span, message);
    }

    public void ReportCannotConvert(TextSpan span, TypeSymbol fromType, TypeSymbol toType) {
        var message = $"ERROR: cannot convert type <{fromType}> to <{toType}>.";

        Report(span, message);
    }

    public void ReportSymbolAlreadyDeclared(TextSpan span, string name) {
        var message = $"ERROR: '{name}' is already declared.";

        Report(span, message);
    }

    public void ReportCannotAssign(TextSpan span, string name) {
        var message = $"ERROR: cannot assign to variable '{name}' because it is read-only.";

        Report(span, message);
    }

    public void ReportUnterminatedString(TextSpan span) {
        var message = "ERROR: unterminated string literal.";

        Report(span, message);
    }

    public void ReportWrongArgumentCount(TextSpan span, string name, int expectedCount, int actualCount) {
        var message = $"ERROR: function '{name}' requires {expectedCount} arguments, but was given {actualCount}.";

        Report(span, message);
    }

    public void ReportUndefinedFunction(TextSpan span, string? identifierText) {
        var message = $"ERROR: function '{identifierText}' doesn't exist.";

        Report(span, message);
    }

    public void ReportWrongArgumentType(TextSpan span, string name, TypeSymbol expectedType, TypeSymbol actualType) {
        var message =
            $"ERROR: parameter '{name}' requires a value of type <{expectedType}>, but was given a value of type <{actualType}>.";

        Report(span, message);
    }

    public void ReportExpressionMustHaveValue(TextSpan span) {
        var message = "ERROR: expression must have a value.";

        Report(span, message);
    }

    public void ReportUndefinedType(TextSpan span, string text) {
        var message = $"ERROR: type '{text}' doesn't exist.";

        Report(span, message);
    }

    public void ReportCannotConvertImplicitly(TextSpan span, TypeSymbol fromType, TypeSymbol toType) {
        var message =
            $"ERROR: cannot implicitly convert type <{fromType}> to <{toType}>. An explicit conversion exists. Are you missing a cast?";

        Report(span, message);
    }

    public void ReportFunctionsAreUnsupported(TextSpan span) {
        var message = "ERROR: functions are unsupported in this language.";

        Report(span, message);
    }

    public void ReportParameterAlreadyDeclared(TextSpan span, string parameterName) {
        var message = $"ERROR: A parameter with the name '{parameterName}' already exists.";

        Report(span, message);
    }

    public void ReportInvalidBreakOrContinue(TextSpan span, string? text) {
        var message = $"ERROR: The keyword '{text}' can only be used inside of loops.";

        Report(span, message);
    }
}