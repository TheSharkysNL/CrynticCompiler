using System.Numerics;
using System.Text;
using CrynticCompiler.Parser.Nodes;

namespace CrynticCompiler.Json.Parser.Nodes;

public class KeyValueMapNode<TData>(IReadOnlyList<KeyValuePairNode<TData>> pairs, INode? parent = null) : JsonCollectionNode(parent)
    where TData : unmanaged, IBinaryInteger<TData>
{
    public IReadOnlyList<KeyValuePairNode<TData>> Pairs
    {
        get => pairs;
        protected internal set => pairs = value;
    }
    public override IReadOnlyList<IJsonNode> Items => pairs;

    protected override char CollectionOpenChar => '{';
    protected override char CollectionCloseChar => '}';
    
    
}