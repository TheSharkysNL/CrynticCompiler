using System.Text;
using CrynticCompiler.Parser.Nodes;

namespace CrynticCompiler.Json.Parser.Nodes;

public abstract class JsonCollectionNode(INode? parent = null) : IJsonNode
{
    public abstract IReadOnlyList<IJsonNode> Items { get; }

    public int Count => Items.Count;
    
    public INode? Parent { get; } = parent;
    
    protected abstract char CollectionOpenChar { get; }
    protected abstract char CollectionCloseChar { get; }
    
    public void AppendString(StringBuilder builder)
    {
        builder.Append(CollectionOpenChar);
        
        if (Items.Count != 0)
        {
            Items[0].AppendString(builder);

            for (int i = 1; i < Items.Count; i++)
            {
                builder.Append(',');
                Items[i].AppendString(builder);
            }
        }

        builder.Append(CollectionCloseChar);
    }

    public void AppendStringFormatted(StringBuilder builder, int indent = 0)
    {
        builder.Append(' ', indent);
        builder.Append(CollectionOpenChar);
        builder.Append('\n');

        if (Items.Count != 0)
        {
            int newIndent = indent + 4;
            Items[0].AppendStringFormatted(builder, newIndent);

            for (int i = 1; i < Items.Count; i++)
            {
                builder.Append(",\n");
                Items[i].AppendStringFormatted(builder, newIndent);
            }
        }
        
        builder.Append('\n');
        builder.Append(' ', indent);
        builder.Append(CollectionCloseChar);
    }
}