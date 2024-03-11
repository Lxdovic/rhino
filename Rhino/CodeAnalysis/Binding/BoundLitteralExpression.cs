namespace Rhino.CodeAnalysis.Binding;

internal sealed class BoundLitteralExpression : BoundExpression {
    public BoundLitteralExpression(object value) {
        Value = value;
    }

    public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
    public override Type Type => Value.GetType();
    public object Value { get; }
}