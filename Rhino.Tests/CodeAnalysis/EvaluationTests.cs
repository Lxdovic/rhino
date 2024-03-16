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
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("!true", false)]
    [InlineData("!!true", true)]
    [InlineData("true == true", true)]
    [InlineData("true != true", false)]
    [InlineData("false == false", true)]
    [InlineData("false != false", false)]
    [InlineData("false == true", false)]
    [InlineData("(a = 10) * a", 100)]
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
    public void SyntaxFactGetTextRoundTrips(string text, object expectedValue) {
        var expression = SyntaxTree.Parse(text);
        var compilation = new Compilation(expression);
        var variables = new Dictionary<VariableSymbol, object>();
        var result = compilation.Evaluate(variables);

        Assert.Empty(result.Diagnostics);
        Assert.Equal(expectedValue, result.Value);
    }
}