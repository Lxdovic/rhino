using System.CodeDom.Compiler;
using Rhino.CodeAnalysis.Symbols;
using Rhino.CodeAnalysis.Syntax;
using Rhino.IO;

namespace Rhino.CodeAnalysis.Binding;

internal static class BoundNodePrinter {
    public static void WriteTo(this BoundNode node, TextWriter writer) {
        if (writer is IndentedTextWriter iw)
            WriteTo(node, iw);
        else
            WriteTo(node, new IndentedTextWriter(writer));
    }

    private static void WriteNestedStatement(this IndentedTextWriter writer, BoundStatement node) {
        var needsIndentation = !(node is BoundBlockStatement);

        if (needsIndentation) writer.Indent++;

        node.WriteTo(writer);

        if (needsIndentation) writer.Indent--;
    }

    private static void WriteNestedExpression(this IndentedTextWriter writer, int parentPrecedence,
        BoundExpression expression) {
        if (expression is BoundUnaryExpression unary)
            writer.WriteNestedExpression(parentPrecedence, unary.Op.SyntaxKind.GetUnaryOperatorPrecedence(), unary);

        else if (expression is BoundBinaryExpression binary)
            writer.WriteNestedExpression(parentPrecedence, binary.Op.SyntaxKind.GetBinaryOperatorPrecedence(), binary);

        else
            expression.WriteTo(writer);
    }

    private static void WriteNestedExpression(this IndentedTextWriter writer, int parentPrecedence,
        int currentPrecedence, BoundExpression expression) {
        var needsParenthesis = parentPrecedence >= currentPrecedence;

        if (needsParenthesis) writer.WritePonctuation("(");

        expression.WriteTo(writer);

        if (needsParenthesis) writer.WritePonctuation(")");
    }


    public static void WriteTo(this BoundNode node, IndentedTextWriter writer) {
        switch (node.Kind) {
            case BoundNodeKind.UnaryExpression:
                WriteUnaryExpression((BoundUnaryExpression)node, writer);
                break;
            case BoundNodeKind.LiteralExpression:
                WriteLiteralExpression((BoundLiteralExpression)node, writer);
                break;
            case BoundNodeKind.BinaryExpression:
                WriteBinaryExpression((BoundBinaryExpression)node, writer);
                break;
            case BoundNodeKind.VariableExpression:
                WriteVariableExpression((BoundVariableExpression)node, writer);
                break;
            case BoundNodeKind.AssignmentExpression:
                WriteAssignmentExpression((BoundAssignmentExpression)node, writer);
                break;
            case BoundNodeKind.ErrorExpression:
                WriteErrorExpression((BoundErrorExpression)node, writer);
                break;
            case BoundNodeKind.CallExpression:
                WriteCallExpression((BoundCallExpression)node, writer);
                break;
            case BoundNodeKind.ConversionExpression:
                WriteConversionExpression((BoundConversionExpression)node, writer);
                break;

            case BoundNodeKind.BlockStatement:
                WriteBlockStatement((BoundBlockStatement)node, writer);
                break;
            case BoundNodeKind.ExpressionStatement:
                WriteExpressionStatement((BoundExpressionStatement)node, writer);
                break;
            case BoundNodeKind.VariableDeclaration:
                WriteVariableDeclaration((BoundVariableDeclaration)node, writer);
                break;
            case BoundNodeKind.IfStatement:
                WriteIfStatement((BoundIfStatement)node, writer);
                break;
            case BoundNodeKind.WhileStatement:
                WriteWhileStatement((BoundWhileStatement)node, writer);
                break;
            case BoundNodeKind.ForStatement:
                WriteForStatement((BoundForStatement)node, writer);
                break;
            case BoundNodeKind.GotoStatement:
                WriteGotoStatement((BoundGotoStatement)node, writer);
                break;
            case BoundNodeKind.LabelStatement:
                WriteLabelStatement((BoundLabelStatement)node, writer);
                break;
            case BoundNodeKind.ConditionalGotoStatement:
                WriteConditionalGotoStatement((BoundConditionalGotoStatement)node, writer);
                break;

            default:
                throw new Exception($"Unexpected node {node.Kind}");
        }
    }

    private static void WriteLabelStatement(BoundLabelStatement node, IndentedTextWriter writer) {
        var unindent = writer.Indent > 0;

        if (unindent) writer.Indent--;

        writer.WritePonctuation(node.Label.Name);
        writer.WritePonctuation(":");
        writer.WriteLine();

        if (unindent) writer.Indent++;
    }

    private static void WriteConditionalGotoStatement(BoundConditionalGotoStatement node, IndentedTextWriter writer) {
        writer.WriteKeyword("goto ");
        writer.WriteIdentifier(node.Label.Name);
        writer.WriteKeyword(node.JumpIfTrue ? " if " : " unless ");

        node.Condition.WriteTo(writer);
        writer.WriteLine();
    }

    private static void WriteGotoStatement(BoundGotoStatement node, IndentedTextWriter writer) {
        writer.WriteKeyword("goto ");
        writer.WriteIdentifier(node.Label.Name);
        writer.WriteLine();
    }

    private static void WriteForStatement(BoundForStatement node, IndentedTextWriter writer) {
        writer.WriteKeyword("for ");
        writer.WriteIdentifier(node.Variable.Name);
        writer.WritePonctuation(" = ");

        node.LowerBound.WriteTo(writer);

        writer.WritePonctuation(" to ");

        node.UpperBound.WriteTo(writer);

        writer.WriteLine();
        writer.WriteNestedStatement(node.Body);
    }

    private static void WriteWhileStatement(BoundWhileStatement node, IndentedTextWriter writer) {
        writer.WriteKeyword("while ");
        node.Condition.WriteTo(writer);
        writer.WriteLine();
        writer.WriteNestedStatement(node.Body);
    }

    private static void WriteIfStatement(BoundIfStatement node, IndentedTextWriter writer) {
        writer.WriteKeyword("if");
        node.Condition.WriteTo(writer);
        writer.WriteLine();
        writer.WriteNestedStatement(node.ThenStatement);
        // WriteBlockStatement((BoundBlockStatement)node.ThenStatement, writer);

        if (node.ElseStatement != null) {
            writer.WriteKeyword("else");
            writer.WriteLine();
            writer.WriteNestedStatement(node.ElseStatement);
        }
    }

    private static void WriteVariableDeclaration(BoundVariableDeclaration node, IndentedTextWriter writer) {
        writer.WriteKeyword(node.Variable.IsReadOnly ? "let " : "var ");
        writer.WriteIdentifier(node.Variable.Name);
        writer.WritePonctuation(" = ");
        node.Initializer.WriteTo(writer);
        writer.WriteLine();
    }

    private static void WriteExpressionStatement(BoundExpressionStatement node, IndentedTextWriter writer) {
        node.Expression.WriteTo(writer);
        writer.WriteLine();
    }

    private static void WriteBlockStatement(BoundBlockStatement node, IndentedTextWriter writer) {
        writer.WritePonctuation("{");
        writer.WriteLine();
        writer.Indent++;

        foreach (var statement in node.Statements) statement.WriteTo(writer);

        writer.Indent--;
        writer.WritePonctuation("}");
        writer.WriteLine();
    }

    private static void WriteConversionExpression(BoundConversionExpression node, IndentedTextWriter writer) {
        writer.WriteIdentifier(node.Type.Name);
        writer.WritePonctuation("(");

        node.Expression.WriteTo(writer);

        writer.WritePonctuation(")");
    }

    private static void WriteVariableExpression(BoundVariableExpression node, IndentedTextWriter writer) {
        writer.WriteIdentifier(node.Variable.Name);
    }

    private static void WriteCallExpression(BoundCallExpression node, IndentedTextWriter writer) {
        writer.WriteIdentifier(node.Function.Name);
        writer.WritePonctuation("(");

        var isFirst = true;

        foreach (var argument in node.Arguments) {
            if (isFirst) isFirst = false;
            else writer.WritePonctuation(", ");

            argument.WriteTo(writer);
        }

        writer.WritePonctuation(")");
    }

    private static void WriteErrorExpression(BoundErrorExpression node, IndentedTextWriter writer) {
        writer.WriteKeyword("?");
    }

    private static void WriteAssignmentExpression(BoundAssignmentExpression node, IndentedTextWriter writer) {
        writer.WriteIdentifier(node.Variable.Name);
        writer.WritePonctuation(" = ");

        node.Expression.WriteTo(writer);
    }

    private static void WriteBinaryExpression(BoundBinaryExpression node, IndentedTextWriter writer) {
        var op = SyntaxFacts.GetText(node.Op.SyntaxKind);
        var precedence = node.Op.SyntaxKind.GetBinaryOperatorPrecedence();


        writer.WriteNestedExpression(precedence, node.Left);
        writer.Write(" ");
        writer.WritePonctuation(op);
        writer.Write(" ");
        writer.WriteNestedExpression(precedence, node.Right);
    }

    private static void WriteLiteralExpression(BoundLiteralExpression node, IndentedTextWriter writer) {
        var value = node.Value;

        if (node.Type == TypeSymbol.Bool) writer.WriteKeyword(((bool)value).ToString());
        else if (node.Type == TypeSymbol.Int) writer.WriteNumber(((int)value).ToString());
        else if (node.Type == TypeSymbol.String)
            writer.WriteString("\"" + ((string)value).Replace("\"", "\"\"") + "\"");

        else throw new Exception($"Unexpected type {node.Type}");
    }

    private static void WriteUnaryExpression(BoundUnaryExpression node, IndentedTextWriter writer) {
        var op = SyntaxFacts.GetText(node.Op.SyntaxKind);
        var precedence = node.Op.SyntaxKind.GetUnaryOperatorPrecedence();

        writer.WritePonctuation(op);
        writer.WriteNestedExpression(precedence, node.Operand);
    }
}