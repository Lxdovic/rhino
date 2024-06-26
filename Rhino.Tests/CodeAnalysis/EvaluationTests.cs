using Rhino.CodeAnalysis;
using Rhino.CodeAnalysis.Symbols;
using Rhino.CodeAnalysis.Syntax;

namespace Rhino.Tests.CodeAnalysis;

public class EvaluationTests {
    [Theory]
    [InlineData("1", 1)]
    [InlineData("1.0", 1.0)]
    [InlineData("0.1", 0.1)]
    [InlineData("-1.23", -1.23)]
    [InlineData("cos(0.45)", 0.9004471023526769)]
    [InlineData("sin(0.45)", 0.43496553411123023)]
    [InlineData("floor(0.45)", 0.0)]
    [InlineData("acos(0.45)", 1.1040309877476002)]
    [InlineData("1.23 + 4.56", 1.23 + 4.56)]
    [InlineData("1.23 - 4.56", 1.23 - 4.56)]
    [InlineData("1.23 * 4.56", 1.23 * 4.56)]
    [InlineData("4.56 / 1.23", 4.56 / 1.23)]
    [InlineData("0.1f", 0.1f)]
    [InlineData("-1.23f", -1.23f)]
    [InlineData("1.23f + 4.56f", 1.23f + 4.56f)]
    [InlineData("1.23f - 4.56f", 1.23f - 4.56f)]
    [InlineData("1.23f * 4.56f", 1.23f * 4.56f)]
    [InlineData("4.56f / 1.23f", 4.56f / 1.23f)]
    [InlineData("-1", -1)]
    [InlineData("+1", 1)]
    [InlineData("~1", -2)]
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
    [InlineData("false | false", false)]
    [InlineData("false | true", true)]
    [InlineData("true | false", true)]
    [InlineData("true | true", true)]
    [InlineData("false & false", false)]
    [InlineData("false & true", false)]
    [InlineData("true & false", false)]
    [InlineData("true & true", true)]
    [InlineData("false ^ false", false)]
    [InlineData("false ^ true", true)]
    [InlineData("true ^ false", true)]
    [InlineData("true ^ true", false)]
    [InlineData("true && true", true)]
    [InlineData("true && false", false)]
    [InlineData("false && true", false)]
    [InlineData("false && false", false)]
    [InlineData("true || true", true)]
    [InlineData("true || false", true)]
    [InlineData("false || true", true)]
    [InlineData("false || false", false)]
    [InlineData("\"test\"", "test")]
    [InlineData("\"te\"\"st\"", "te\"st")]
    [InlineData("\"test\" == \"test\"", true)]
    [InlineData("\"test\" == \"tst\"", false)]
    [InlineData("\"test\" != \"tset\"", true)]
    [InlineData("{ var a = 0 (a = 10) * a }", 100)]
    [InlineData("{ var a = 0 if a == 0 a = 10 a }", 10)]
    [InlineData("{ var a = 0 if a == 4 a = 10 a }", 0)]
    [InlineData("{ var a = 0 if a == 0 a = 10 else a = 5 a }", 10)]
    [InlineData("{ var a = 0 if a == 4 a = 10 else a = 5 a }", 5)]
    [InlineData("{ var i = 10 var result = 0 while i > 0 { result = result + i i = i - 1 } result }", 55)]
    [InlineData("{ var result = 0 for i = 1 to 10 { result = result + i } result }", 55)]
    [InlineData("{ var a = 10 for i = 1 to (a = a - 1) {} a }", 9)]
    [InlineData("{ var i = 0 while i < 5 { i = i + 1 if i == 5 continue } i }", 5)]
    public void EvaluatorComputesCorrectValues(string text, object expectedValue) {
        AssertValue(text, expectedValue);
    }

    [Fact]
    public void Evaluator_FunctionReturn_Missing() {
        var text = @"
            function [add](a: int, b: int): int {}";

        var diagnostics = @"
            ERROR: Not all code paths return a value for function 'add'.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorAssignmentExpressionReportsNotAVariable() {
        var text = @"[print] = 42";

        var diagnostics = @"
            ERROR: 'print' is not a variable.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorCallExpressionReportsUndefined() {
        var text = @"[foo](42)";

        var diagnostics = @"
            ERROR: function 'foo' doesn't exist.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorCallExpressionReportsNotAFunction() {
        var text = @"
            {
                let foo = 42
                [foo](42)
            }";

        var diagnostics = @"
            ERROR: 'foo' is not a function.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorFunctionWithReturnValueShouldNotReturnVoid() {
        var text = @"
            function test(): int {
                [return]
            }";

        var diagnostics = @"
            ERROR: The return expression is missing for function 'test'.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorNotAllCodePathsReturnValue() {
        var text = @"
                function [test](n: int): bool {
                    if (n > 10)
                       return true
                }";

        var diagnostics = @"
            ERROR: Not all code paths return a value for function 'test'.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorExpressionMustHaveValue() {
        var text = @"
            function test(n: int) {
                return
            }

            let value = [test(100)]";

        var diagnostics = @"
            ERROR: expression must have a value.";

        AssertDiagnostics(text, diagnostics);
    }

    [Theory]
    [InlineData("[break]", "break")]
    [InlineData("[continue]", "continue")]
    public void EvaluatorInvalidBreakOrContinue(string text, string keyword) {
        var diagnostics = $@"
            ERROR: The keyword '{keyword}' can only be used inside of loops.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorInvalidReturn() {
        var text = @"
                [return]";

        var diagnostics = @"
                ERROR: The 'return' keyword can only be used inside of functions.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorParameterAlreadyDeclared() {
        var text = @"
                function sum(a: int, b: int, [a: int]): int {
                    return a + b + c
                }";

        var diagnostics = @"
                ERROR: A parameter with the name 'a' already exists.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorFunctionMustHaveName() {
        var text = @"
                function [(]a: int, b: int): int {
                    return a + b
                }";

        var diagnostics = @"
                ERROR: unexpected token <OpenParenthesisToken>, expected <IdentifierToken>.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorWrongArgumentType() {
        var text = @"
                function test(n: int): bool {
                    return n > 10
                }

                let testValue = ""string""
                test([testValue])";

        var diagnostics = @"
                ERROR: parameter 'n' requires a value of type <int>, but was given a value of type <string>.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorBadType() {
        var text = @"
                function test(n: [invalidtype]) {}";

        var diagnostics = @"
                ERROR: type 'invalidtype' doesn't exist.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorInvokeFunctionArgumentsMissing() {
        var text = @"
                print([)]";

        var diagnostics = @"
                ERROR: function 'print' requires 1 arguments, but was given 0.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorInvokeFunctionArgumentsExceeding() {
        var text = @"
                print(""Hello""[, "" "", "" world!""])";

        var diagnostics = @"
                ERROR: function 'print' requires 1 arguments, but was given 3.";

        AssertDiagnostics(text, diagnostics);
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
            ERROR: 'x' is already declared.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorVariablesCanShadowFunctions() {
        var text = @"
                {
                    let print = 42
                    [print](""test"")
                }
            ";

        var diagnostics = @"
                ERROR: 'print' is not a function.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void Evaluator_InvokeFunctionArguments_NoInfiniteLoop() {
        var text = @"
                print(""Hi""[[=]][)]";

        var diagnostics = @"
                ERROR: unexpected token <EqualsToken>, expected <CloseParenthesisToken>.
                ERROR: unexpected token <EqualsToken>, expected <IdentifierToken>.
                ERROR: unexpected token <CloseParenthesisToken>, expected <IdentifierToken>.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void Evaluator_FunctionParameters_NoInfiniteLoop() {
        var text = @"
                function hi(name: string[[[=]]][)]
                {
                    print(""Hi "" + name + ""!"" )
                }[]";

        var diagnostics = @"
                ERROR: unexpected token <EqualsToken>, expected <CloseParenthesisToken>.
                ERROR: unexpected token <EqualsToken>, expected <OpenBraceToken>.
                ERROR: unexpected token <EqualsToken>, expected <IdentifierToken>.
                ERROR: unexpected token <CloseParenthesisToken>, expected <IdentifierToken>.
                ERROR: unexpected token <EndOfFileToken>, expected <CloseBraceToken>.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorIfStatementReportsCannotConvert() {
        var text = @"
            {
                var x: float = 0.2f
                if [x]
                    x = 10
            }
        ";

        var diagnostics = @"
            ERROR: cannot convert type <float> to <bool>.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorWhileStatementReportsCannotConvert() {
        var text = @"
            {
                var x: float = 0.1f
                while [x]
                    x = 10
            }
        ";

        var diagnostics = @"
            ERROR: cannot convert type <float> to <bool>.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorForStatementReportsCannotConvertLowerBound() {
        var text = @"
            {
                var result = 0
                for i = [false] to 10 
                    result = result + i
            }
        ";

        var diagnostics = @"
            ERROR: cannot convert type <bool> to <int>.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorForStatementReportsCannotConvertUpperBound() {
        var text = @"
            {
                var result = 0
                for i = 1 to [true]
                    result = result + i
            }
        ";

        var diagnostics = @"
            ERROR: cannot convert type <bool> to <int>.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorBlockStatementNoInfiniteLoop() {
        var text = @"
            {
            [)][]";

        var diagnostics = @"
            ERROR: unexpected token <CloseParenthesisToken>, expected <IdentifierToken>.
            ERROR: unexpected token <EndOfFileToken>, expected <CloseBraceToken>.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorNameExpressionReportsUndefined() {
        var text = @"[x] * 10";

        var diagnostics = @"
            ERROR: variable 'x' doesn't exist.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorNameExpressionReportsNoErrorForInsertedToken() {
        var text = @"1 + []";

        var diagnostics = @"
            ERROR: unexpected token <EndOfFileToken>, expected <IdentifierToken>.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorAssignmentExpressionReportsUndefined() {
        var text = @"[x] * 10";

        var diagnostics = @"
            ERROR: variable 'x' doesn't exist.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorAssignmentExpressionReportsCannotAssign() {
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
    public void EvaluatorAssignmentExpressionReportsCannotConvert() {
        var text = @"
            {
                var x = 10
                x = [true]
            }";

        var diagnostics = @"
            ERROR: cannot convert type <bool> to <int>.";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorUnaryExpressionReportsUndefined() {
        var text = @"[+]true";

        var diagnostics = @"
            Unary operator '+' is not defined for type <bool>";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorBinaryExpressionReportsUndefined() {
        var text = @"10 [*] false";

        var diagnostics = @"
            Binary operator '*' is not defined for types <int> and <bool>";

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
            var actualSpan = result.Diagnostics[i].Location.Span;

            Assert.Equal(expectedSpan, actualSpan);
        }
    }
}