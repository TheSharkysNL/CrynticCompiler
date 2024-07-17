using System.Text;
using CrynticCompiler.Parser.Nodes;

namespace CrynticCompiler.Json.Parser.Nodes;

public class ListNode(IReadOnlyList<IJsonNode> items, INode? parent = null) : JsonCollectionNode(parent)
{
    public override IReadOnlyList<IJsonNode> Items => items;
    
    protected internal IReadOnlyList<IJsonNode> Values
    {
        get => items;
        set => items = value;
    }

    protected override char CollectionOpenChar => '[';
    protected override char CollectionCloseChar => ']';
}