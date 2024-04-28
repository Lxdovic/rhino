using Rhino.CodeAnalysis.Text;

namespace Rhino.CodeAnalysis;

public class Diagnostic {
    public Diagnostic(TextLocation location, string message) {
        Location = location;
        Message = message;
    }

    public TextLocation Location { get; }
    public string Message { get; }

    public override string ToString() {
        return Message;
    }
}