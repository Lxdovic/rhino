using System.Collections;
using Rhino.CodeAnalysis.Syntax;

namespace Rhino.CodeAnalysis;

internal sealed class DiagnosticBag : IEnumerable<Diagnostic> {
    private readonly List<Diagnostic> _diagnostics = new();
    
    private void Report(TextSpan span, string message) {
        var diagnostic = new Diagnostic(span, message);
        _diagnostics.Add(diagnostic);
    }

    public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    public void AddRange(DiagnosticBag diagnostics) {
        _diagnostics.AddRange(diagnostics._diagnostics);
    }
    
    public void ReportInvalidNumber(TextSpan span, string text, Type type) {
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

    public void ReportUndefinedUnaryOperator(TextSpan span, string operatorText, Type operandType) {
        var message = $"Unary operator <{operatorText}> is not defined for type <{operandType}>";
        
        Report(span, message);
    }

    public void ReportUndefinedBinaryOperator(TextSpan span, string operatorText, Type leftType, Type rightType) {
        var message = $"Binary operator <{operatorText}> is not defined for types <{leftType}> and <{rightType}>";
        
        Report(span, message);
    }
}