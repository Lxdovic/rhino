using Rhino.CodeAnalysis;
using Rhino.CodeAnalysis.Syntax;

namespace Rhino.Tests.CodeAnalysis;

public class EvaluationTests {
    [Theory]
    [InlineData("1", 1)]
    [InlineData("-1", -1)]
    [InlineData("+1", 1)]
    [InlineData("14 + 12", 26)]
    [InlineData("12 - 2", 10)]
    [InlineData("4 * 2", 8)]
    [InlineData("9 / 3", 3)]
    [InlineData("(10)", 10)]
    [InlineData("1 == 2", false)]
    [InlineData("1 != 2", true)]
    [InlineData("1 <= 2", true)]
    [InlineData("1 >= 2", false)]
    [InlineData("1 < 2", true)]
    [InlineData("1 > 2", false)]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("!true", false)]
    [InlineData("!!true", true)]
    [InlineData("true == true", true)]
    [InlineData("true != true", false)]
    [InlineData("false == false", true)]
    [InlineData("false != false", false)]
    [InlineData("false == true", false)]
    [InlineData("5 | 4", 5 | 4)]
    [InlineData("5 & 4", 5 & 4)]
    [InlineData("5 ^ 4", 5 ^ 4)]
    [InlineData("5 << 2", 5 << 2)]
    [InlineData("5 >> 2", 5 >> 2)]
    [InlineData("true && true", true)]
    [InlineData("true && false", false)]
    [InlineData("false && true", false)]
    [InlineData("false && false", false)]
    [InlineData("true || true", true)]
    [InlineData("true || false", true)]
    [InlineData("false || true", true)]
    [InlineData("false || false", false)]
    [InlineData("{ var a = 0 (a = 10) * a }", 100)]
    [InlineData("{ var a = 0 if a == 0 a = 10 a }", 10)]
    [InlineData("{ var a = 0 if a == 4 a = 10 a }", 0)]
    [InlineData("{ var a = 0 if a == 0 a = 10 else a = 5 a }", 10)]
    [InlineData("{ var a = 0 if a == 4 a = 10 else a = 5 a }", 5)]
    [InlineData("{ var i = 10 var result = 0 while i > 0 { result = result + i i = i - 1 } result }", 55)]
    [InlineData("{ var result = 0 for i = 1 to 10 { result = result + i } result }", 55)]
    public void EvaluatorComputesCorrectValues(string text, object expectedValue) {
        AssertValue(text, expectedValue);
    }

    [Fact]
    public void EvaluatorVariableDeclarationReportsRedeclaration() {
        var text = @"
            {
                var x = 10
                var y = 100
                {
                    var x = 10
                }
                var [x] = 5
            }
        ";

        var diagnostics = @"
            ERROR: variable 'x' is already declared.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorNameReportsUndefined() {
        var text = @"[x] * 10";

        var diagnostics = @"
            ERROR: variable 'x' doesn't exist.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorAssignmentReportsUndefined() {
        var text = @"[x] * 10";

        var diagnostics = @"
            ERROR: variable 'x' doesn't exist.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorAssignmentReportsCannotAssign() {
        var text = @"
            {
                let x = 10
                x [=] 10
            }";

        var diagnostics = @"
            ERROR: cannot assign to variable 'x' because it is read-only.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorAssignmentReportsCannotConvert() {
        var text = @"
            {
                var x = 10
                x = [true]
            }";

        var diagnostics = @"
            ERROR: cannot convert type <System.Boolean> to <System.Int32>.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorUnaryReportsUndefined() {
        var text = @"[+]true";

        var diagnostics = @"
            Unary operator '+' is not defined for type <System.Boolean>";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorBinaryReportsUndefined() {
        var text = @"10 [*] false";

        var diagnostics = @"
            Binary operator '*' is not defined for types <System.Int32> and <System.Boolean>";

        AssertDiagnostics(text, diagnostics);
    }

    private static void AssertValue(string text, object expectedValue) {
        var expression = SyntaxTree.Parse(text);
        var compilation = new Compilation(expression);
        var variables = new Dictionary<VariableSymbol, object>();
        var result = compilation.Evaluate(variables);

        Assert.Empty(result.Diagnostics);
        Assert.Equal(expectedValue, result.Value);
    }

    private void AssertDiagnostics(string text, string diagnosticText) {
        var annotatedText = AnnotatedText.Parse(text);
        var syntaxTree = SyntaxTree.Parse(annotatedText.Text);
        var compilation = new Compilation(syntaxTree);
        var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

        var expectedDiagnostics = AnnotatedText.UnindentLines(diagnosticText);

        Assert.Equal(annotatedText.Spans.Length, expectedDiagnostics.Length);
        Assert.Equal(expectedDiagnostics.Length, result.Diagnostics.Length);

        for (var i = 0; i < expectedDiagnostics.Length; i++) {
            var expectedMessage = expectedDiagnostics[i];
            var actualMessage = result.Diagnostics[i].Message;

            Assert.Equal(expectedMessage, actualMessage);

            var expectedSpan = annotatedText.Spans[i];
            var actualSpan = result.Diagnostics[i].Span;

            Assert.Equal(expectedSpan, actualSpan);
        }
    }
}