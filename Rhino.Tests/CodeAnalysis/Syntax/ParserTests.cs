using Rhino.CodeAnalysis.Syntax;

namespace Rhino.Tests.CodeAnalysis.Syntax;

public class ParserTests {
    [Theory]
    [MemberData(nameof(GetBinaryOperatorPairsData))]
    public void ParserBinaryExpressionHonorsPrecedences(SyntaxKind operator1, SyntaxKind operator2) {
        var op1Precedence = operator1.GetBinaryOperatorPrecedence();
        var op2Precedence = operator2.GetBinaryOperatorPrecedence();
        var op1Text = SyntaxFacts.GetText(operator1);
        var op2Text = SyntaxFacts.GetText(operator2);
        var text = $"a {op1Text} b {op2Text} c";
        var expression = ParseExpression(text);

        if (op1Precedence >= op2Precedence)
            using (var e = new AssertingEnumerator(expression)) {
                e.AssertNode(SyntaxKind.BinaryExpression);
                e.AssertNode(SyntaxKind.BinaryExpression);

                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "a");

                e.AssertToken(operator1, op1Text);

                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "b");

                e.AssertToken(operator2, op2Text);

                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "c");
            }
        else
            using (var e = new AssertingEnumerator(expression)) {
                e.AssertNode(SyntaxKind.BinaryExpression);

                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "a");

                e.AssertToken(operator1, op1Text);

                e.AssertNode(SyntaxKind.BinaryExpression);
                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "b");

                e.AssertToken(operator2, op2Text);

                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "c");
            }
    }

    [Theory]
    [MemberData(nameof(GetUnaryOperatorPairsData))]
    public void ParserUnaryExpressionHonorsPrecedences(SyntaxKind unaryKind, SyntaxKind binaryKind) {
        var unaryPrecedence = unaryKind.GetUnaryOperatorPrecedence();
        var binaryPrecedence = binaryKind.GetBinaryOperatorPrecedence();
        var unaryText = SyntaxFacts.GetText(unaryKind);
        var binaryText = SyntaxFacts.GetText(binaryKind);
        var text = $"{unaryText} a {binaryText} b";
        var expression = ParseExpression(text);

        if (unaryPrecedence >= binaryPrecedence)
            using (var e = new AssertingEnumerator(expression)) {
                e.AssertNode(SyntaxKind.BinaryExpression);
                e.AssertNode(SyntaxKind.UnaryExpression);

                e.AssertToken(unaryKind, unaryText);

                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "a");


                e.AssertToken(binaryKind, binaryText);

                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "b");
            }
        else
            using (var e = new AssertingEnumerator(expression)) {
                e.AssertNode(SyntaxKind.UnaryExpression);

                e.AssertToken(unaryKind, unaryText);

                e.AssertNode(SyntaxKind.BinaryExpression);
                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "a");

                e.AssertToken(binaryKind, binaryText);

                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "b");
            }
    }

    private static ExpressionSyntax ParseExpression(string text) {
        var syntaxTree = SyntaxTree.Parse(text);
        var root = syntaxTree.Root;
        var member = Assert.Single(root.Members);
        var globalStatement = Assert.IsType<GlobalStatementSyntax>(member);
        return Assert.IsType<ExpressionStatementSyntax>(globalStatement.Statement).Expression;
    }

    public static IEnumerable<object[]> GetBinaryOperatorPairsData() {
        foreach (var op1 in SyntaxFacts.GetBinaryOperatorKinds())
        foreach (var op2 in SyntaxFacts.GetBinaryOperatorKinds())
            yield return new object[] { op1, op2 };
    }

    public static IEnumerable<object[]> GetUnaryOperatorPairsData() {
        foreach (var op1 in SyntaxFacts.GetUnaryOperatorKinds())
        foreach (var op2 in SyntaxFacts.GetBinaryOperatorKinds())
            yield return new object[] { op1, op2 };
    }
}