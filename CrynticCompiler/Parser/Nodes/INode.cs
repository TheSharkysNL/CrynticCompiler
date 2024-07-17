using System;
namespace CrynticCompiler.Parser.Nodes;

public interface INode
{
    public INode? Parent { get; }
}

public readonly struct Node(int lineStart, int lineEnd, int columnStart, int columnEnd, INode? node)
{
    public readonly int LineStart = lineStart;
    public readonly int ColumnStart = columnStart;

    public readonly int LineEnd = lineEnd;
    public readonly int ColumnEnd = columnEnd;

    /// <summary>
    /// When using the <see cref="Parser{TData}"/> a <see cref="INode"/> can only be null if the <see cref="Parser{TData}.IncludeNullNodes"/> is set to true
    /// </summary>
    public readonly INode? Value = node;
}
