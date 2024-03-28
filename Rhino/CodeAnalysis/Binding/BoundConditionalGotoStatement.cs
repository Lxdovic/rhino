namespace Rhino.CodeAnalysis.Binding;

internal sealed class BoundConditionalGotoStatement : BoundStatement {
    public BoundConditionalGotoStatement(LabelSymbol label, BoundExpression condition, bool jumpIfFalse = false) {
        Label = label;
        Condition = condition;
        JumpIfFalse = jumpIfFalse;
    }

    public LabelSymbol Label { get; }
    public BoundExpression Condition { get; }
    public bool JumpIfFalse { get; }
    public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;
}