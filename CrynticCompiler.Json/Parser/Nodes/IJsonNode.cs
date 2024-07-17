using System.Text;
using CrynticCompiler.Parser.Nodes;

namespace CrynticCompiler.Json.Parser.Nodes;

public interface IJsonNode : INode
{
    public void AppendString(StringBuilder builder);

    public void AppendStringFormatted(StringBuilder builder, int indent = 0);
}