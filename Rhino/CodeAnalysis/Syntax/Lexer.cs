using System.Text;
using Rhino.CodeAnalysis.Symbols;
using Rhino.CodeAnalysis.Text;

namespace Rhino.CodeAnalysis.Syntax;

internal sealed class Lexer {
    private readonly SyntaxTree _syntaxTree;
    private readonly SourceText _text;

    private SyntaxKind _kind;
    private int _position;
    private int _start;
    private object _value;

    public Lexer(SyntaxTree syntaxTree) {
        _syntaxTree = syntaxTree;
        _text = syntaxTree.Text;
    }

    public DiagnosticBag Diagnostics { get; } = new();

    private char Current => Peek(0);
    private char LookAhead => Peek(1);

    private char Peek(int offset) {
        var index = _position + offset;
        if (index >= _text.Length) return '\0';

        return _text[index];
    }

    public SyntaxToken Lex() {
        _start = _position;
        _kind = SyntaxKind.BadToken;
        _value = null;

        switch (Current) {
            case '\0':
                _kind = SyntaxKind.EndOfFileToken;

                break;
            case '+':
                _kind = SyntaxKind.PlusToken;
                _position++;

                break;
            case '%':
                _kind = SyntaxKind.ModuloToken;
                _position++;

                break;
            case '-':
                _kind = SyntaxKind.MinusToken;
                _position++;

                break;
            case '*':
                _kind = SyntaxKind.StarToken;
                _position++;

                break;
            case '/':
                _kind = SyntaxKind.SlashToken;
                _position++;

                break;
            case '(':
                _kind = SyntaxKind.OpenParenthesisToken;
                _position++;

                break;
            case ')':
                _kind = SyntaxKind.CloseParenthesisToken;
                _position++;

                break;
            case '{':
                _kind = SyntaxKind.OpenBraceToken;
                _position++;

                break;
            case '}':
                _kind = SyntaxKind.CloseBraceToken;
                _position++;

                break;
            case '&':
                _position++;
                if (Current != '&') {
                    _kind = SyntaxKind.AmpersandToken;
                }

                else {
                    _position++;
                    _kind = SyntaxKind.AmpersandAmpersandToken;
                }

                break;
            case '|':
                _position++;
                if (Current != '|') {
                    _kind = SyntaxKind.PipeToken;
                }

                else {
                    _position++;
                    _kind = SyntaxKind.PipePipeToken;
                }

                break;
            case '=':
                _position++;
                if (Current != '=') {
                    _kind = SyntaxKind.EqualsToken;
                }

                else {
                    _kind = SyntaxKind.EqualsEqualsToken;
                    _position++;
                }

                break;
            case '!':
                _position++;
                if (Current != '=') {
                    _kind = SyntaxKind.BangToken;
                }

                else {
                    _kind = SyntaxKind.BangEqualsToken;
                    _position++;
                }

                break;
            case '^':
                _position++;
                _kind = SyntaxKind.HatToken;

                break;
            case '~':
                _position++;
                _kind = SyntaxKind.TildeToken;

                break;
            case '<':
                _position++;

                switch (Current) {
                    case '=':
                        _kind = SyntaxKind.LessThanOrEqualsToken;
                        _position++;

                        break;
                    case '<':
                        _kind = SyntaxKind.LessThanLessThanToken;
                        _position++;

                        break;
                    default:
                        _kind = SyntaxKind.LessToken;

                        break;
                }

                break;
            case '>':
                _position++;

                switch (Current) {
                    case '=':
                        _kind = SyntaxKind.GreaterThanOrEqualsToken;
                        _position++;

                        break;
                    case '>':
                        _kind = SyntaxKind.GreaterThanGreaterThanToken;
                        _position++;

                        break;
                    default:
                        _kind = SyntaxKind.GreaterToken;

                        break;
                }

                break;
            case ':':
                _position++;
                _kind = SyntaxKind.ColonToken;
                break;
            case '\"':
                ReadString();
                break;
            case ',':
                _position++;
                _kind = SyntaxKind.CommaToken;
                break;
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                ReadNumberToken();

                break;

            case ' ':
            case '\t':
            case '\n':
            case '\r':
                ReadWhiteSpace();

                break;
            default:
                if (char.IsLetter(Current)) {
                    ReadIdentifierOrKeyword();
                }
                else if (char.IsWhiteSpace(Current)) {
                    ReadWhiteSpace();
                }

                else {
                    var span = new TextSpan(_position, 1);
                    var location = new TextLocation(_text, span);
                    Diagnostics.ReportBadCharacter(location, Current);
                    _position++;
                }

                break;
        }

        var length = _position - _start;
        var text = SyntaxFacts.GetText(_kind);

        if (text == null) text = _text.ToString(_start, length);

        return new SyntaxToken(_syntaxTree, _kind, _start, text, _value);
    }

    private void ReadString() {
        _position++;

        var sb = new StringBuilder();
        var isDone = false;

        while (!isDone)
            switch (Current) {
                case '\0':
                case '\r':
                case '\n':
                    var span = new TextSpan(_start, 1);
                    var location = new TextLocation(_text, span);
                    Diagnostics.ReportUnterminatedString(location);
                    isDone = true;
                    break;
                case '"':
                    if (LookAhead == '"') {
                        sb.Append(Current);
                        _position += 2;
                    }

                    else {
                        _position++;
                        isDone = true;
                    }

                    break;
                default:
                    sb.Append(Current);
                    _position++;
                    break;
            }

        _kind = SyntaxKind.StringToken;
        _value = sb.ToString();
    }

    private void ReadIdentifierOrKeyword() {
        while (char.IsLetter(Current)) _position++;

        var length = _position - _start;
        var text = _text.ToString(_start, length);
        _kind = SyntaxFacts.GetKeywordKind(text);
    }

    private void ReadWhiteSpace() {
        while (char.IsWhiteSpace(Current)) _position++;

        _kind = SyntaxKind.WhiteSpaceToken;
    }

    private void ReadNumberToken() {
        var numberOfDots = 0;

        while (char.IsDigit(Current) || Current == '.') {
            if (Current == '.') numberOfDots++;

            _position++;
        }

        var length = _position - _start;
        var text = _text.ToString(_start, length);

        if (numberOfDots > 0) {
            if (Current == 'f') {
                _position++;

                if (!float.TryParse(text, out var value)) {
                    var span = new TextSpan(_start, length);
                    var location = new TextLocation(_text, span);
                    Diagnostics.ReportInvalidNumber(location, text, TypeSymbol.Float);
                }

                _value = value;
            }

            else {
                if (!double.TryParse(text, out var value)) {
                    var span = new TextSpan(_start, length);
                    var location = new TextLocation(_text, span);
                    Diagnostics.ReportInvalidNumber(location, text, TypeSymbol.Double);
                }

                _value = value;
            }
        }

        if (numberOfDots == 0) {
            if (!int.TryParse(text, out var value)) {
                var span = new TextSpan(_start, length);
                var location = new TextLocation(_text, span);
                Diagnostics.ReportInvalidNumber(location, text, TypeSymbol.Int);
            }

            _value = value;
        }

        _kind = SyntaxKind.NumberToken;
    }
}