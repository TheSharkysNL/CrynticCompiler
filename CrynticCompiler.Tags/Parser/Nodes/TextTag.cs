using System.Numerics;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Tags.Parser.Nodes;

public class TextTag<TData>(IReadOnlyList<Token<TData>> tokens) : Tag<TData>(default)
    where TData : unmanaged, IBinaryInteger<TData>
{
    public readonly IReadOnlyList<Token<TData>> Tokens = tokens;

    public override IReadOnlyList<TagAttribute<TData>> Attributes => Array.Empty<TagAttribute<TData>>();
    public override IReadOnlyList<Tag<TData>> Children => Array.Empty<Tag<TData>>();
    public override TokenData<TData> Name => default;

    public override string ToString() =>
        string.Join(' ', Tokens);

    protected override string GetStringIndented(int indent) =>
        new string(' ', indent) + string.Join(' ', Tokens);
}