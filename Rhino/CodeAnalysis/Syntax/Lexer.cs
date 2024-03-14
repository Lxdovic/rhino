namespace Rhino.CodeAnalysis.Syntax;

internal sealed class Lexer {
    private readonly string _text;
    private int _position;

    public Lexer(string text) {
        _text = text;
    }

    public DiagnosticBag Diagnostics { get; } = new();

    private char Current => Peek(0);
    private char LookAhead => Peek(1);

    private void Next() {
        _position++;
    }

    private char Peek(int offset) {
        var index = _position + offset;
        if (index >= _text.Length) return '\0';

        return _text[index];
    }

    public SyntaxToken Lex() {
        if (_position >= _text.Length) return new SyntaxToken(SyntaxKind.EndOfFileToken, _position, "\0");

        var start = _position;

        if (char.IsDigit(Current)) {
            while (char.IsDigit(Current)) Next();

            var length = _position - start;
            var text = _text.Substring(start, length);
            if (!int.TryParse(text, out var value))
                Diagnostics.ReportInvalidNumber(new TextSpan(start, length), _text, typeof(int));

            return new SyntaxToken(SyntaxKind.NumberToken, start, text, value);
        }

        if (char.IsWhiteSpace(Current)) {
            while (char.IsWhiteSpace(Current)) Next();

            var length = _position - start;
            var text = _text.Substring(start, length);
            int.TryParse(text, out var value);
            return new SyntaxToken(SyntaxKind.WhiteSpaceToken, start, text, value);
        }

        if (char.IsLetter(Current)) {
            while (char.IsLetter(Current)) Next();

            var length = _position - start;
            var text = _text.Substring(start, length);
            var kind = SyntaxFacts.GetKeywordKind(text);
            return new SyntaxToken(kind, start, text);
        }

        switch (Current) {
            case '+': return new SyntaxToken(SyntaxKind.PlusToken, _position++, "+");
            case '-': return new SyntaxToken(SyntaxKind.MinusToken, _position++, "-");
            case '*': return new SyntaxToken(SyntaxKind.StarToken, _position++, "*");
            case '/': return new SyntaxToken(SyntaxKind.SlashToken, _position++, "/");
            case '(': return new SyntaxToken(SyntaxKind.OpenParenthesisToken, _position++, "(");
            case ')':
                return new SyntaxToken(SyntaxKind.CloseParenthesisToken, _position++, ")");
            case '&':
                if (LookAhead == '&') {
                    _position += 2;
                    return new SyntaxToken(SyntaxKind.AmpersandAmpersandToken, start, "&&");
                }

                _position++;
                return new SyntaxToken(SyntaxKind.AmpersandToken, start, "&");
            case '|':
                if (LookAhead == '|') {
                    _position += 2;
                    return new SyntaxToken(SyntaxKind.PipePipeToken, start, "||");
                }

                _position++;
                return new SyntaxToken(SyntaxKind.PipeToken, start, "|");
            case '=':
                if (LookAhead == '=') {
                    _position += 2;
                    return new SyntaxToken(SyntaxKind.EqualsEqualsToken, start, "==");
                }

                _position += 1;
                return new SyntaxToken(SyntaxKind.EqualsToken, start, "=");
            case '!':
                if (LookAhead == '=') {
                    _position += 2;
                    return new SyntaxToken(SyntaxKind.BangEqualsToken, start, "!=");
                }

                _position += 1;
                return new SyntaxToken(SyntaxKind.BangToken, start, "!");
            case '^':
                _position++;
                return new SyntaxToken(SyntaxKind.HatToken, start, "^");
            case '~':
                _position++;
                return new SyntaxToken(SyntaxKind.TildeToken, start, "~");
            case '<':
                if (LookAhead == '<') {
                    _position += 2;
                    return new SyntaxToken(SyntaxKind.LessThanLessThanToken, start, "<<");
                }

                if (LookAhead == '=') {
                    _position += 2;
                    return new SyntaxToken(SyntaxKind.LessThanOrEqualsToken, start, "<=");
                }

                break;
            case '>':
                if (LookAhead == '>') {
                    _position += 2;
                    return new SyntaxToken(SyntaxKind.GreaterThanGreaterThanToken, start, ">>");
                }

                if (LookAhead == '=') {
                    _position += 2;
                    return new SyntaxToken(SyntaxKind.GreaterThanOrEqualsToken, start, ">=");
                }

                break;
        }

        Diagnostics.ReportBadCharacter(_position, Current);
        return new SyntaxToken(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1));
    }
}