using System.Collections.Immutable;
using Rhino.CodeAnalysis.Binding;
using Rhino.CodeAnalysis.Symbols;
using Rhino.CodeAnalysis.Syntax;

namespace Rhino.CodeAnalysis.Lowering;

internal sealed class Lowerer : BoundTreeRewriter {
    private int _labelCount;

    private Lowerer() {
    }

    public static BoundBlockStatement Lower(BoundStatement statement) {
        var lowerer = new Lowerer();
        var result = lowerer.RewriteStatement(statement);
        return Flatten(result);
    }

    private static BoundBlockStatement Flatten(BoundStatement statement) {
        var builder = ImmutableArray.CreateBuilder<BoundStatement>();
        var stack = new Stack<BoundStatement>();

        stack.Push(statement);

        while (stack.Count > 0) {
            var current = stack.Pop();

            if (current is BoundBlockStatement block)
                foreach (var s in block.Statements.Reverse())
                    stack.Push(s);

            else
                builder.Add(current);
        }

        return new BoundBlockStatement(builder.ToImmutable());
    }


    private BoundLabel GenerateLabel() {
        var name = $"label{++_labelCount}";
        return new BoundLabel(name);
    }

    protected override BoundStatement RewriteIfStatement(BoundIfStatement node) {
        if (node.ElseStatement == null) {
            var endLabel = GenerateLabel();

            var endLabelStatement = new BoundLabelStatement(endLabel);
            var gotoFalse = new BoundConditionalGotoStatement(endLabel, node.Condition, false);

            var result =
                new BoundBlockStatement(ImmutableArray.Create(gotoFalse, node.ThenStatement, endLabelStatement));

            return RewriteStatement(result);
        }

        else {
            var endLabel = GenerateLabel();
            var elseLabel = GenerateLabel();

            var gotoEndStatement = new BoundGotoStatement(endLabel);
            var elseLabelStatement = new BoundLabelStatement(elseLabel);
            var endLabelStatement = new BoundLabelStatement(endLabel);
            var gotoFalse = new BoundConditionalGotoStatement(elseLabel, node.Condition, false);

            var result =
                new BoundBlockStatement(ImmutableArray.Create(gotoFalse, node.ThenStatement, gotoEndStatement,
                    elseLabelStatement, node.ElseStatement, endLabelStatement));

            return RewriteStatement(result);
        }
    }

    protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node) {
        var bodyLabel = GenerateLabel();

        var gotoContinue = new BoundGotoStatement(node.ContinueLabel);
        var bodyLabelStatement = new BoundLabelStatement(bodyLabel);
        var continueLabelStatement = new BoundLabelStatement(node.ContinueLabel);
        var gotoTrue = new BoundConditionalGotoStatement(bodyLabel, node.Condition);
        var breakLabelStatement = new BoundLabelStatement(node.BreakLabel);

        var result =
            new BoundBlockStatement(ImmutableArray.Create(gotoContinue, bodyLabelStatement, node.Body,
                continueLabelStatement, gotoTrue, breakLabelStatement));

        return RewriteStatement(result);
    }

    protected override BoundStatement RewriteForStatement(BoundForStatement node) {
        var variableDeclaration = new BoundVariableDeclaration(node.Variable, node.LowerBound);
        var variableExpression = new BoundVariableExpression(node.Variable);
        var upperBoundSymbol = new LocalVariableSymbol("upperBound", true, TypeSymbol.Int);
        var upperBoundDeclaration = new BoundVariableDeclaration(upperBoundSymbol, node.UpperBound);
        var condition = new BoundBinaryExpression(
            variableExpression,
            BoundBinaryOperator.Bind(SyntaxKind.LessThanOrEqualsToken, TypeSymbol.Int, TypeSymbol.Int),
            new BoundVariableExpression(upperBoundSymbol)
        );

        var continueLabelStatement = new BoundLabelStatement(node.ContinueLabel);
        var increment = new BoundExpressionStatement(
            new BoundAssignmentExpression(
                node.Variable,
                new BoundBinaryExpression(
                    variableExpression,
                    BoundBinaryOperator.Bind(SyntaxKind.PlusToken, TypeSymbol.Int, TypeSymbol.Int),
                    new BoundLiteralExpression(1)
                )
            )
        );

        var whileBlock = new BoundBlockStatement(ImmutableArray.Create(node.Body, continueLabelStatement, increment));
        var whileStatement =
            new BoundWhileStatement(condition, whileBlock, node.BreakLabel, GenerateLabel());
        var result =
            new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(variableDeclaration, upperBoundDeclaration,
                whileStatement));

        return RewriteStatement(result);
    }
}