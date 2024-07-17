using System;
using System.Numerics;

namespace CrynticCompiler.Tokenizer;

public interface ITokenizer<TData> : IEnumerable<Token<TData>>
    where TData : unmanaged, IBinaryInteger<TData>
{
    /// <summary>
    /// gets the <see cref="Token{TData}"/> enumerator
    /// </summary>
    /// <returns>a <see cref="Token{TData}"/> enumerator</returns>
    public new ITokenEnumerator<TData> GetEnumerator();
}

