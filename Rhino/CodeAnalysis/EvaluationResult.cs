using System.Collections.Immutable;

namespace Rhino.CodeAnalysis;

public sealed class EvaluationResult {
    public EvaluationResult(ImmutableArray<Diagnostic> diagnostics, object? value = null) {
        Diagnostics = diagnostics;
        Value = value;
    }

    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public object? Value { get; }
}