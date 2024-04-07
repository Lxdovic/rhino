using System.Collections;
using System.Collections.Immutable;

namespace Rhino.CodeAnalysis.Syntax;

public abstract class SeparatedSyntaxList {
    public abstract ImmutableArray<SyntaxNode> GetWithSeparators();
}

public sealed class SeparatedSyntaxList<T> : SeparatedSyntaxList, IEnumerable<T>
    where T : SyntaxNode {
    private readonly ImmutableArray<SyntaxNode> _nodesAndSeparators;

    public SeparatedSyntaxList(ImmutableArray<SyntaxNode> nodesAndSeparators) {
        _nodesAndSeparators = nodesAndSeparators;
    }

    public int Count => (_nodesAndSeparators.Length + 1) / 2;
    public T this[int index] => (T)_nodesAndSeparators[index * 2];

    public IEnumerator<T> GetEnumerator() {
        for (var i = 0; i < Count; i++) yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public override ImmutableArray<SyntaxNode> GetWithSeparators() {
        return _nodesAndSeparators;
    }

    public SyntaxToken GetSeparator(int index) {
        if (index == Count - 1) return null;

        return (SyntaxToken)_nodesAndSeparators[index * 2 + 1];
    }
}