using System.Numerics;
using CrynticCompiler.Extensions;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Tags.Parser.Nodes;

public class ShortTag<TData>
    : Tag<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    public override IReadOnlyList<Tag<TData>> Children => Array.Empty<Tag<TData>>();

    public ShortTag(TokenData<TData> name, IReadOnlyList<TagAttribute<TData>> attributes)
        : base(name, attributes)
    {  }

    public override string ToString()
    {
        string tagName = "<" + Name;
        if (Attributes.Count > 0)
        {
            tagName += " " + string.Join(' ', Attributes);
        }

        return tagName + " />";
    }

    protected override string GetStringIndented(int indent)
    {
        string indentString = new string(' ', indent);
        string tagName = indentString + "<" + Name;
        if (Attributes.Count > 0)
        {
            tagName += " " + string.Join(' ', Attributes);
        }

        return tagName + " />";
    }
}