using System.Collections.Immutable;
using Rhino.CodeAnalysis.Binding;
using Rhino.CodeAnalysis.Symbols;
using Rhino.CodeAnalysis.Syntax;

namespace Rhino.CodeAnalysis;

public class Compilation {
    private BoundGlobalScope _globalScope;

    public Compilation(params SyntaxTree[] syntaxTrees) : this(null, syntaxTrees) {
    }

    private Compilation(Compilation previous, params SyntaxTree[] syntaxTrees) {
        Previous = previous;
        SyntaxTrees = syntaxTrees.ToImmutableArray();
    }

    public Compilation Previous { get; }
    public ImmutableArray<SyntaxTree> SyntaxTrees { get; }

    internal BoundGlobalScope GlobalScope {
        get {
            if (_globalScope == null) {
                var globalScope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTrees);
                Interlocked.CompareExchange(ref _globalScope, globalScope, null);
            }

            return _globalScope;
        }
    }

    public Compilation ContinueWith(SyntaxTree syntaxTree) {
        return new Compilation(this, syntaxTree);
    }

    public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables) {
        var parseDiagnostics = SyntaxTrees.SelectMany(st => st.Diagnostics).ToImmutableArray();
        var diagnostics = parseDiagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
        if (diagnostics.Any()) return new EvaluationResult(diagnostics);

        var program = Binder.BindProgram(GlobalScope);
        var appPath = Environment.GetCommandLineArgs()[0];
        var appDirectory = Path.GetDirectoryName(appPath);
        var controlFlowGraphPath = Path.Combine(appDirectory, "control-flow-graph.dot");

        var controlFlowGraphStatement = !program.Statement.Statements.Any() && program.Functions.Any()
            ? program.Functions.Last().Value
            : program.Statement;

        var controlFlowGraph = ControlFlowGraph.Create(controlFlowGraphStatement);

        using (var writer = new StreamWriter(controlFlowGraphPath)) {
            controlFlowGraph.WriteTo(writer);
        }

        if (program.Diagnostics.Any()) return new EvaluationResult(program.Diagnostics.ToImmutableArray());

        var evaluator = new Evaluator(program, variables);
        var value = evaluator.Evaluate();

        return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
    }

    public void EmitTree(TextWriter writer) {
        var program = Binder.BindProgram(GlobalScope);

        if (program.Statement.Statements.Any())
            program.Statement.WriteTo(writer);

        else
            foreach (var functionBody in program.Functions) {
                if (!GlobalScope.Functions.Contains(functionBody.Key)) continue;

                functionBody.Key.WriteTo(writer);
                functionBody.Value.WriteTo(writer);
            }
    }
}