using Rhino.CodeAnalysis.Text;

namespace Rhino;

public class TextSpanComparer : IComparer<TextSpan> {
    public int Compare(TextSpan x, TextSpan y) {
        var cmp = x.Start - y.Start;
        if (cmp == 0) cmp = x.Length - y.Length;

        return cmp;
    }
}