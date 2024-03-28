namespace Rhino.CodeAnalysis.Binding;

internal sealed class BoundGotoStatement : BoundStatement {
    public BoundGotoStatement(LabelSymbol label) {
        Label = label;
    }

    public LabelSymbol Label { get; }
    public override BoundNodeKind Kind => BoundNodeKind.GotoStatement;
}