using System.Numerics;
using CrynticCompiler.Tokenizer;
using CrynticCompiler.Tokenizer.IO;

namespace CrynticCompiler.Json.Tokenizer;

public abstract class JsonTokenizer<TData> : Tokenizer<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    protected JsonTokenizer(Reader<TData> reader) : base(reader)
    {
    }

    protected JsonTokenizer(Reader<TData> reader, IReadOnlyDictionary<ReadOnlyMemory<TData>, int> keywords) : base(reader,
        keywords)
    {
    }

    protected override bool IsScopeIn(TData value, out int scopeChange)
    {
        scopeChange = 0;
        return false;
    }

    protected override bool IsScopeOut(TData value, out int scopeChange)
    {
        scopeChange = 0;
        return false;
    }

    protected override bool IsStringStart(TData value) => false;
    protected override bool IsStringEnd(TData value) => false;

    protected override bool IsMultiLineStringStart(TData value, ref CharacterReader reader) =>
        value == TData.CreateTruncating('\"');

    protected override bool IsMultiLineStringEnd(TData value, ref CharacterReader reader) =>
        value == TData.CreateTruncating('\"');

    protected override bool IsCharacterStringStart(TData value) => false;
    protected override bool IsCharacterStringEnd(TData value) => false;

    protected override bool IsComment(TData value, ref CharacterReader reader) => false;
    protected override bool IsMultiLineCommentStart(TData value, ref CharacterReader reader) => false;
    protected override bool IsMultiLineCommentEnd(TData value, ref CharacterReader reader) => false;

    protected override bool IsDigitCharacter(TData value) =>
        TokenizerHelpers.IsDigit(value);

    protected override bool IsNumber(ReadOnlySpan<TData> span, out int type)
    {
        type = TokenType.Number;
        return true;
    }

    protected override bool IsIdentifierCharacter(TData value) =>
        TokenizerHelpers.IsLetter(value);

    protected override bool IsIdentifierStartCharacter(TData value, out bool multiLine)
    {
        multiLine = false;
        return TokenizerHelpers.IsLetter(value);
    }
}