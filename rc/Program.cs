namespace Rhino;

internal static class Program {
    private static void Main(string[] args) {
        var repl = new RhinoRepl();
        repl.Run();
    }
}