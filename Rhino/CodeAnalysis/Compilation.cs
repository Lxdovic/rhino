using Rhino.CodeAnalysis.Binding;
using Rhino.CodeAnalysis.Syntax;

namespace Rhino.CodeAnalysis;

public class Compilation {
    public Compilation(SyntaxTree syntaxTree) {
        SyntaxTree = syntaxTree;
    }

    public SyntaxTree SyntaxTree { get; }

    public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables) {
        var binder = new Binder(variables);
        var boundExpression = binder.BindExpression(SyntaxTree.Root);

        var diagnostics = SyntaxTree.Diagnostics.Concat(binder.Diagnostics).ToArray();
        if (diagnostics.Any()) return new EvaluationResult(diagnostics);

        var evaluator = new Evaluator(boundExpression, variables);
        var value = evaluator.Evaluate();

        return new EvaluationResult(Array.Empty<Diagnostic>(), value);
    }
}