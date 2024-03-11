namespace Rhino.CodeAnalysis;

public struct TextSpan {
    public TextSpan(int start, int length) {
        Start = start;
        Length = length;
    }

    public int Start { get; }
    public int Length { get; }
    public int End => Start + Length;
    public bool OverlapsWith(TextSpan span) => Start < span.End && End > span.Start;
    public bool Contains(TextSpan span) => Start <= span.Start && End >= span.End;
}