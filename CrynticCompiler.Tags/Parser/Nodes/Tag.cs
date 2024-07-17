using System.Numerics;
using CrynticCompiler.Extensions;
using CrynticCompiler.Parser.Nodes;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Tags.Parser.Nodes;

public class Tag<TData>(
    TokenData<TData> name,
    IReadOnlyList<TagAttribute<TData>> attributes,
    IReadOnlyList<Tag<TData>> children,
    INode? parent = null) : INode
    where TData : unmanaged, IBinaryInteger<TData>
{
    public virtual TokenData<TData> Name { get; } = name;

    public virtual IReadOnlyList<TagAttribute<TData>> Attributes
    {
        get => attributes;
        protected internal set => attributes = value;
    }

    public virtual IReadOnlyList<Tag<TData>> Children
    {
        get => children;
        protected internal set => children = value;
    }

    public INode? Parent { get; } = parent;
    
    public Tag(TokenData<TData> name) 
        : this(name, Array.Empty<TagAttribute<TData>>(), Array.Empty<Tag<TData>>())
    {}
    
    public Tag(TokenData<TData> name, IReadOnlyList<TagAttribute<TData>> attributes) 
        : this(name, attributes, Array.Empty<Tag<TData>>())
    {}
    
    public Tag(TokenData<TData> name, IReadOnlyList<Tag<TData>> children) 
        : this(name, Array.Empty<TagAttribute<TData>>(), children)
    {}

    public override string ToString()
    {
        string tagName = "<" + Name;
        if (Attributes.Count > 0)
        {
            tagName += " " + string.Join(' ', Attributes);
        }

        return tagName + ">" + string.Join("", Children) + $"</{Name}>";
    }

    protected virtual string GetStringIndented(int indent)
    {
        string indentString = new string(' ', indent);
        string tagName = indentString + "<" + Name;
        if (Attributes.Count > 0)
        {
            tagName += " " + string.Join(' ', Attributes);
        }

        tagName += ">\n";

        tagName += string.Join('\n', Children.Select(child => child.GetStringIndented(indent + 4)));

        return $"{tagName}\n{indentString}</{Name}>";
    }

    public string ToStringFormatted() =>
        GetStringIndented(0);
}