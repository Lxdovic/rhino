using Rhino.CodeAnalysis.Text;

namespace Rhino.Tests.CodeAnalysis.Text;

public class SourceTextTests {
    [Theory]
    [InlineData(".", 1)]
    [InlineData(".\r\n", 2)]
    [InlineData(".\r\n\r\n", 3)]
    public void SourceTextIncludesLastLine(string text, int numberExprectedLineCount) {
        var sourceText = SourceText.From(text);

        Assert.Equal(numberExprectedLineCount, sourceText.Lines.Length);
    }
}