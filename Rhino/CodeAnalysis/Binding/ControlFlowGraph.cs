using System.CodeDom.Compiler;
using Rhino.CodeAnalysis.Symbols;
using Rhino.CodeAnalysis.Syntax;

namespace Rhino.CodeAnalysis.Binding;

internal sealed class ControlFlowGraph {
    private ControlFlowGraph(BasicBlock start, BasicBlock end, List<BasicBlock> blocks,
        List<BasicBlockBranch> branches) {
        Start = start;
        End = end;
        Blocks = blocks;
        Branches = branches;
    }

    public BasicBlock Start { get; set; }
    public BasicBlock End { get; set; }
    public List<BasicBlock> Blocks { get; }
    public List<BasicBlockBranch> Branches { get; }

    public void WriteTo(TextWriter writer) {
        writer.WriteLine("digraph G {");

        var blockIds = new Dictionary<BasicBlock, string>();

        for (var index = 0; index < Blocks.Count; index++) {
            var id = $"N{index}";

            blockIds.Add(Blocks[index], id);
        }

        foreach (var block in Blocks) {
            var id = blockIds[block];
            var label = Quote(block.ToString().Replace(Environment.NewLine, "\\l"));

            writer.WriteLine($"  {id} [label={label}, shape=box]");
        }

        string Quote(string text) {
            return $"\"{text}\"";
        }

        foreach (var branch in Branches) {
            var fromId = blockIds[branch.From];
            var toId = blockIds[branch.To];
            var label = Quote(branch.ToString());

            writer.WriteLine($"  {fromId} -> {toId} [label={label}]");
        }

        writer.WriteLine("}");
    }

    public static ControlFlowGraph Create(BoundBlockStatement body) {
        var basicBlockBuilder = new BasicBlockBuilder();
        var blocks = basicBlockBuilder.Build(body);

        var graphBuilder = new GraphBuilder();

        return graphBuilder.Build(blocks);
    }

    public static bool AllPathsReturn(BoundBlockStatement body) {
        var graph = Create(body);

        foreach (var branch in graph.End.Incoming) {
            var lastStatement = branch.From.Statements.LastOrDefault();

            if (lastStatement == null || lastStatement.Kind != BoundNodeKind.ReturnStatement)
                return false;
        }

        return true;
    }

    public sealed class BasicBlockBuilder {
        private readonly List<BasicBlock> _blocks = new();
        private readonly List<BoundStatement> _statements = new();

        public List<BasicBlock> Build(BoundBlockStatement block) {
            foreach (var statement in block.Statements)
                switch (statement.Kind) {
                    case BoundNodeKind.ExpressionStatement:
                    case BoundNodeKind.VariableDeclaration:
                        _statements.Add(statement);
                        break;
                    case BoundNodeKind.LabelStatement:
                        StartBlock();
                        _statements.Add(statement);
                        break;
                    case BoundNodeKind.GotoStatement:
                    case BoundNodeKind.ConditionalGotoStatement:
                    case BoundNodeKind.ReturnStatement:
                        _statements.Add(statement);
                        StartBlock();
                        break;

                    default: throw new Exception($"Unexpected statement: {statement.Kind}");
                }

            EndBlock();

            return _blocks.ToList();
        }

        private void StartBlock() {
            EndBlock();
        }

        private void EndBlock() {
            if (_statements.Count > 0) {
                var block = new BasicBlock();

                block.Statements.AddRange(_statements);

                _blocks.Add(block);
                _statements.Clear();
            }
        }
    }

    public sealed class GraphBuilder {
        private readonly Dictionary<BoundLabel, BasicBlock> _blockFromLabel = new();
        private readonly Dictionary<BoundStatement, BasicBlock> _blockFromStatement = new();
        private readonly List<BasicBlockBranch> _branches = new();
        private readonly BasicBlock _end = new(false);
        private readonly BasicBlock _start = new(true);

        public ControlFlowGraph Build(List<BasicBlock> blocks) {
            if (!blocks.Any())
                Connect(_start, _end);

            else Connect(_start, blocks.First());

            foreach (var block in blocks)
            foreach (var statement in block.Statements) {
                _blockFromStatement.Add(statement, block);

                if (statement is BoundLabelStatement labelStatement)
                    _blockFromLabel.Add(labelStatement.Label, block);
            }

            for (var index = 0; index < blocks.Count; index++) {
                var current = blocks[index];
                var next = index == blocks.Count - 1 ? _end : blocks[index + 1];

                foreach (var statement in current.Statements) {
                    var isLastStatement = statement == current.Statements.Last();

                    switch (statement.Kind) {
                        case BoundNodeKind.LabelStatement:
                        case BoundNodeKind.ExpressionStatement:
                        case BoundNodeKind.VariableDeclaration:
                            if (isLastStatement)
                                Connect(current, next);
                            break;
                        case BoundNodeKind.GotoStatement:
                            var gotoStatement = (BoundGotoStatement)statement;
                            var toBlock = _blockFromLabel[gotoStatement.Label];

                            Connect(current, toBlock);

                            break;
                        case BoundNodeKind.ConditionalGotoStatement:
                            var conditionalGoto = (BoundConditionalGotoStatement)statement;
                            var thenBlock = _blockFromLabel[conditionalGoto.Label];
                            var elseBlock = next;
                            var negatedCondition = Negate(conditionalGoto.Condition);
                            var thenCondition = conditionalGoto.JumpIfTrue
                                ? conditionalGoto.Condition
                                : negatedCondition;
                            var elseCondition = conditionalGoto.JumpIfTrue
                                ? negatedCondition
                                : conditionalGoto.Condition;

                            Connect(current, thenBlock, thenCondition);
                            Connect(current, elseBlock, elseCondition);

                            break;
                        case BoundNodeKind.ReturnStatement:
                            Connect(current, _end);
                            break;
                        default: throw new Exception($"Unexpected statement: {statement.Kind}");
                    }
                }
            }

            ScanAgain:
            foreach (var block in blocks)
                if (!block.Incoming.Any()) {
                    RemoveBlock(blocks, block);
                    goto ScanAgain;
                }

            blocks.Insert(0, _start);
            blocks.Add(_end);

            return new ControlFlowGraph(_start, _end, blocks, _branches);
        }

        private void RemoveBlock(List<BasicBlock> blocks, BasicBlock block) {
            foreach (var branch in block.Incoming) {
                branch.From.Outgoing.Remove(branch);
                _branches.Remove(branch);
            }

            foreach (var branch in block.Outgoing) {
                branch.To.Incoming.Remove(branch);
                _branches.Remove(branch);
            }

            blocks.Remove(block);
        }


        private BoundExpression Negate(BoundExpression condition) {
            if (condition is BoundLiteralExpression literal) {
                var value = (bool)literal.Value;

                return new BoundLiteralExpression(!value);
            }

            var unaryOperator = BoundUnaryOperator.Bind(SyntaxKind.BangToken, TypeSymbol.Bool);

            return new BoundUnaryExpression(unaryOperator, condition);
        }

        private void Connect(BasicBlock start, BasicBlock end, BoundExpression condition = null) {
            if (condition is BoundLiteralExpression l) {
                var value = (bool)l.Value;
                if (value) condition = null;
                else return;
            }

            var branch = new BasicBlockBranch(start, end, condition);

            _branches.Add(branch);
            start.Outgoing.Add(branch);
            end.Incoming.Add(branch);
        }
    }

    public sealed class BasicBlock {
        public BasicBlock() {
        }

        public BasicBlock(bool isStart = false) {
            IsStart = isStart;
            IsEnd = !isStart;
        }

        public bool IsStart { get; }
        public bool IsEnd { get; }

        public List<BoundStatement> Statements { get; } = new();
        public List<BasicBlockBranch> Incoming { get; } = new();
        public List<BasicBlockBranch> Outgoing { get; } = new();

        public override string ToString() {
            if (IsStart) return "<Start>";
            if (IsEnd) return "<End>";

            using (var writer = new StringWriter())
            using (var indentedTextWriter = new IndentedTextWriter(writer)) {
                foreach (var statement in Statements)
                    statement.WriteTo(indentedTextWriter);

                return indentedTextWriter.ToString();
            }
        }
    }

    public sealed class BasicBlockBranch {
        public BasicBlockBranch(BasicBlock from, BasicBlock to, BoundExpression condition) {
            From = from;
            To = to;
            Condition = condition;
        }

        public BasicBlock From { get; }
        public BasicBlock To { get; }
        public BoundExpression Condition { get; }

        public override string ToString() {
            if (Condition == null) return string.Empty;

            return Condition.ToString();
        }
    }
}