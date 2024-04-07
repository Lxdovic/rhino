namespace Rhino.CodeAnalysis.Binding;

internal sealed class BoundGotoStatement : BoundStatement {
    public BoundGotoStatement(BoundLabel label) {
        Label = label;
    }

    public BoundLabel Label { get; }
    public override BoundNodeKind Kind => BoundNodeKind.GotoStatement;
}