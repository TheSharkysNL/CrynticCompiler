using System;
using System.Numerics;

namespace CrynticCompiler.Tokenizer;

/// <summary>
/// a structure that allows a user to peek the next token
/// only 1 can be made at a time and must be consumed before peeking another token
/// </summary>
/// <typeparam name="TData">the type of data that will be consumed</typeparam>
public readonly ref struct Consumable<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    public readonly ITokenEnumerator<TData> Enumerator;
    public readonly Token<TData> Token;

    public readonly int Line;
    public readonly int Column;

    public readonly int PreviousLine => Enumerator.Line;
    public readonly int PreviousColumn => Enumerator.Column;

    public Consumable(ITokenEnumerator<TData> enumerator, Token<TData> token, int line, int column)
    {
        Enumerator = enumerator;
        Token = token;
        Line = line;
        Column = column;
    }

    /// <summary>
    /// consumes this token
    /// </summary>
    public void Consume() =>
        Enumerator.Consume(this);
}

