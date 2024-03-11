namespace Rhino.CodeAnalysis;

public sealed class EvaluationResult {
    public EvaluationResult(IEnumerable<string> diagnostics, object value) {
        Diagnostics = diagnostics.ToArray();
        Value = value;
    }

    public IEnumerable<string> Diagnostics { get; }
    public object Value { get; }
}