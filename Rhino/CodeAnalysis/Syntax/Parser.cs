using System.Collections.Immutable;
using Rhino.CodeAnalysis.Text;

namespace Rhino.CodeAnalysis.Syntax;

internal sealed class Parser {
    private readonly SourceText _text;
    private readonly ImmutableArray<SyntaxToken> _tokens;
    private int _position;

    public Parser(SourceText text) {
        var tokens = new List<SyntaxToken>();
        var lexer = new Lexer(text);
        SyntaxToken token;

        do {
            token = lexer.Lex();

            if (token.Kind != SyntaxKind.WhiteSpaceToken && token.Kind != SyntaxKind.BadToken) tokens.Add(token);
        } while (token.Kind != SyntaxKind.EndOfFileToken);

        _text = text;
        _tokens = tokens.ToImmutableArray();
        Diagnostics.AddRange(lexer.Diagnostics);
    }

    public DiagnosticBag Diagnostics { get; } = new();

    private SyntaxToken Current => Peek(0);

    private SyntaxToken Peek(int offset) {
        var index = _position + offset;
        if (index >= _tokens.Length) return _tokens[_tokens.Length - 1];
        return _tokens[index];
    }

    private ExpressionSyntax ParseExpression() {
        return ParseAssignmentExpression();
    }

    private ExpressionSyntax ParseAssignmentExpression() {
        if (Peek(0).Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.EqualsToken) {
            var identifierToken = NextToken();
            var operatorToken = NextToken();
            var right = ParseAssignmentExpression();
            return new AssignmentExpressionSyntax(identifierToken, operatorToken, right);
        }

        return ParseBinaryExpression();
    }

    private ExpressionSyntax ParseBinaryExpression(int parentPrecedence = 0) {
        ExpressionSyntax left;
        var unaryOperatorPrecedence = Current.Kind.GetUnaryOperatorPrecedence();

        if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence) {
            var operatorToken = NextToken();
            var operand = ParseBinaryExpression(unaryOperatorPrecedence);
            left = new UnaryExpressionSyntax(operatorToken, operand);
        }
        else {
            left = ParsePrimaryExpression();
        }

        while (true) {
            var precedence = Current.Kind.GetBinaryOperatorPrecedence();
            if (precedence == 0 || precedence <= parentPrecedence) break;

            var operatorToken = NextToken();
            var right = ParseBinaryExpression(precedence);
            left = new BinaryExpressionSyntax(left, operatorToken, right);
        }

        return left;
    }

    private SyntaxToken NextToken() {
        var current = Current;
        _position++;
        return current;
    }

    private SyntaxToken MatchToken(SyntaxKind kind) {
        if (Current.Kind == kind) return NextToken();

        Diagnostics.ReportUnexpectedToken(Current.Span, Current.Kind, kind);
        return new SyntaxToken(kind, Current.Position);
    }

    private ExpressionSyntax ParsePrimaryExpression() {
        switch (Current.Kind) {
            case SyntaxKind.OpenParenthesisToken: {
                return ParseParenthesizedExpression();
            }
            case SyntaxKind.TrueKeyword:
            case SyntaxKind.FalseKeyword: {
                return ParseBooleanLiteral();
            }
            case SyntaxKind.NumberToken: {
                return ParseNumberLiteral();
            }

            case SyntaxKind.StringToken: {
                return ParseStringLiteral();
            }

            case SyntaxKind.IdentifierToken:
            default:
                return ParseNameOrCallExpression();
        }
    }

    private ExpressionSyntax ParseNumberLiteral() {
        var numberToken = MatchToken(SyntaxKind.NumberToken);

        return new LiteralExpressionSyntax(numberToken);
    }

    private ExpressionSyntax ParseParenthesizedExpression() {
        var left = MatchToken(SyntaxKind.OpenParenthesisToken);
        var expression = ParseExpression();
        var right = MatchToken(SyntaxKind.CloseParenthesisToken);
        return new ParenthesizedExpressionSyntax(left, expression, right);
    }

    private ExpressionSyntax ParseBooleanLiteral() {
        var isTrue = Current.Kind == SyntaxKind.TrueKeyword;
        var keywordToken = isTrue ? MatchToken(SyntaxKind.TrueKeyword) : MatchToken(SyntaxKind.FalseKeyword);

        return new LiteralExpressionSyntax(keywordToken, isTrue);
    }

    private ExpressionSyntax ParseStringLiteral() {
        var stringToken = MatchToken(SyntaxKind.StringToken);

        return new LiteralExpressionSyntax(stringToken);
    }

    private ExpressionSyntax ParseNameOrCallExpression() {
        if (Peek(0).Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.OpenParenthesisToken)
            return ParseCallExpression();

        return ParseNameExpression();
    }

    private ExpressionSyntax ParseCallExpression() {
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var openParenthesisToken = MatchToken(SyntaxKind.OpenParenthesisToken);
        var arguments = ParseArguments();
        var closeParenthesisToken = MatchToken(SyntaxKind.CloseParenthesisToken);

        return new CallExpressionSyntax(identifier, openParenthesisToken, arguments, closeParenthesisToken);
    }

    private SeparatedSyntaxList<ExpressionSyntax> ParseArguments() {
        var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
        var parseNextArgument = true;

        while (parseNextArgument &&
               Current.Kind != SyntaxKind.CloseParenthesisToken && Current.Kind != SyntaxKind.EndOfFileToken) {
            var expression = ParseExpression();
            nodesAndSeparators.Add(expression);

            if (Current.Kind == SyntaxKind.CommaToken) {
                var comma = MatchToken(SyntaxKind.CommaToken);
                nodesAndSeparators.Add(comma);
            }

            else {
                parseNextArgument = false;
            }
        }

        return new SeparatedSyntaxList<ExpressionSyntax>(nodesAndSeparators.ToImmutable());
    }

    private ExpressionSyntax ParseNameExpression() {
        var identifierToken = MatchToken(SyntaxKind.IdentifierToken);
        return new NameExpressionSyntax(identifierToken);
    }

    public CompilationUnitSyntax ParseCompilationUnit() {
        var members = ParseMembers();
        var endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);

        return new CompilationUnitSyntax(members, endOfFileToken);
    }

    private ImmutableArray<MemberSyntax> ParseMembers() {
        var members = ImmutableArray.CreateBuilder<MemberSyntax>();

        while (Current.Kind != SyntaxKind.EndOfFileToken) {
            var startToken = Current;
            var member = ParseMember();

            members.Add(member);

            // if the statement was not parsed correctly, we skip to the next token
            if (Current == startToken) NextToken();
        }

        return members.ToImmutable();
    }

    private MemberSyntax ParseMember() {
        if (Current.Kind == SyntaxKind.FunctionKeyword) return ParseFunctionDeclaration();
        return ParseGlobalStatement();
    }

    private MemberSyntax ParseFunctionDeclaration() {
        var functionKeyword = MatchToken(SyntaxKind.FunctionKeyword);
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var openParenthesisToken = MatchToken(SyntaxKind.OpenParenthesisToken);
        var parameters = ParseParameterList();
        var closeParenthesisToken = MatchToken(SyntaxKind.CloseParenthesisToken);
        var typeClause = ParseOptionalTypeClause();
        var body = ParseBlockStatement();

        return new FunctionDeclarationSyntax(functionKeyword, identifier, openParenthesisToken, parameters,
            closeParenthesisToken, typeClause, body);
    }

    private SeparatedSyntaxList<ParameterSyntax> ParseParameterList() {
        var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
        var parseNextParameter = true;

        while (parseNextParameter && Current.Kind != SyntaxKind.CloseParenthesisToken &&
               Current.Kind != SyntaxKind.EndOfFileToken) {
            var parameter = ParseParameter();
            nodesAndSeparators.Add(parameter);

            if (Current.Kind == SyntaxKind.CommaToken) {
                var comma = MatchToken(SyntaxKind.CommaToken);
                nodesAndSeparators.Add(comma);
            }

            else {
                parseNextParameter = false;
            }
        }

        return new SeparatedSyntaxList<ParameterSyntax>(nodesAndSeparators.ToImmutable());
    }

    private ParameterSyntax ParseParameter() {
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var typeClause = ParseTypeClause();

        return new ParameterSyntax(identifier, typeClause);
    }

    private MemberSyntax ParseGlobalStatement() {
        var statement = ParseStatement();

        return new GlobalStatementSyntax(statement);
    }

    private StatementSyntax ParseStatement() {
        switch (Current.Kind) {
            case SyntaxKind.OpenBraceToken:
                return ParseBlockStatement();
            case SyntaxKind.LetKeyword:
            case SyntaxKind.VarKeyword:
                return ParseVariableDeclaration();
            case SyntaxKind.IfKeyword:
                return ParseIfStatement();
            case SyntaxKind.WhileKeyword:
                return ParseWhileStatement();
            case SyntaxKind.ForKeyword:
                return ParseForStatement();
            case SyntaxKind.BreakKeyword:
                return ParseBreakStatement();
            case SyntaxKind.ContinueKeyword:
                return ParseContinueStatement();
            default:
                return ParseExpressionStatement();
        }
    }

    private StatementSyntax ParseContinueStatement() {
        var keyword = MatchToken(SyntaxKind.ContinueKeyword);

        return new ContinueStatementSyntax(keyword);
    }

    private StatementSyntax ParseBreakStatement() {
        var keyword = MatchToken(SyntaxKind.BreakKeyword);

        return new BreakStatementSyntax(keyword);
    }

    private StatementSyntax ParseForStatement() {
        var keyword = MatchToken(SyntaxKind.ForKeyword);
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var equalsToken = MatchToken(SyntaxKind.EqualsToken);
        var lowerBound = ParseExpression();
        var toKeyword = MatchToken(SyntaxKind.ToKeyword);
        var upperBound = ParseExpression();
        var body = ParseStatement();

        return new ForStatementSyntax(keyword, identifier, equalsToken, lowerBound, toKeyword, upperBound, body);
    }

    private StatementSyntax ParseWhileStatement() {
        var keyword = MatchToken(SyntaxKind.WhileKeyword);
        var condition = ParseExpression();
        var body = ParseStatement();

        return new WhileStatementSyntax(keyword, condition, body);
    }

    private ElseClauseSyntax ParseElseClause() {
        if (Current.Kind != SyntaxKind.ElseKeyword) return null;

        var keyword = NextToken();
        var statement = ParseStatement();

        return new ElseClauseSyntax(keyword, statement);
    }

    private StatementSyntax ParseIfStatement() {
        var keyword = MatchToken(SyntaxKind.IfKeyword);
        var condition = ParseExpression();
        var statement = ParseStatement();
        var elseClause = ParseElseClause();

        return new IfStatementSyntax(keyword, condition, statement, elseClause);
    }

    private StatementSyntax ParseVariableDeclaration() {
        var expected = Current.Kind == SyntaxKind.LetKeyword ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword;
        var keyword = MatchToken(expected);
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var typeClause = ParseOptionalTypeClause();
        var equals = MatchToken(SyntaxKind.EqualsToken);
        var initializer = ParseExpression();

        return new VariableDeclarationSyntax(keyword, identifier, typeClause, equals, initializer);
    }

    private TypeClauseSyntax ParseOptionalTypeClause() {
        if (Current.Kind != SyntaxKind.ColonToken) return null;

        return ParseTypeClause();
    }

    private TypeClauseSyntax ParseTypeClause() {
        var colonToken = MatchToken(SyntaxKind.ColonToken);
        var identifier = MatchToken(SyntaxKind.IdentifierToken);

        return new TypeClauseSyntax(colonToken, identifier);
    }

    private ExpressionStatementSyntax ParseExpressionStatement() {
        var expression = ParseExpression();

        return new ExpressionStatementSyntax(expression);
    }

    private BlockStatementSyntax ParseBlockStatement() {
        var statements = ImmutableArray.CreateBuilder<StatementSyntax>();
        var openBraceToken = MatchToken(SyntaxKind.OpenBraceToken);

        while (Current.Kind != SyntaxKind.EndOfFileToken && Current.Kind != SyntaxKind.CloseBraceToken) {
            var startToken = Current;
            var statement = ParseStatement();

            statements.Add(statement);

            // if the statement was not parsed correctly, we skip to the next token
            if (Current == startToken) NextToken();
        }

        var closeBraceToken = MatchToken(SyntaxKind.CloseBraceToken);

        return new BlockStatementSyntax(openBraceToken, statements.ToImmutable(), closeBraceToken);
    }
}