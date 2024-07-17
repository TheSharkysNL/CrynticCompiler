using System;
using System.Diagnostics;
using System.Numerics;
using CrynticCompiler.Extensions;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Parser.Nodes;

public class StringLiteral<TData>(TokenData<TData> @string, INode? parent = null) : ILiteralExpression<TData>
	where TData : unmanaged, IBinaryInteger<TData>
{
    public TokenData<TData> Literal { get; } = @string;
    
    public INode? Parent { get; } = parent;

    // public readonly static ReadOnlyMemory<TData> coloredSpan = "<span class=\"text-[#79A978]\">".AsSpan().Convert<char, TData>();
    // public readonly static ReadOnlyMemory<TData> spanEnd = "</span>".AsSpan().Convert<char, TData>();

    //
    // public ReadOnlySpan<TData> GetRepresentation() =>
    //     GetRepresentation(FormatOptions.None);
    //
    // public ReadOnlySpan<TData> GetRepresentation(FormatOptions options) =>
    //     GetRepresentation(options, 0);
    //
    // public ReadOnlySpan<TData> GetRepresentation(FormatOptions options, int indentationLevel) =>
    //     GetRepresentation(options, indentationLevel, null);
    //
    // public ReadOnlySpan<TData> GetRepresentation(FormatOptions options, int indentationLevel, object? formatObject)
    // {
    //     ValueListBuilder<TData> builder;
    //
    //     bool useColor = false;
    //     if (formatObject is Dictionary<string, bool> dict && dict.ContainsKey("colored"))
    //     {
    //         useColor = true;
    //         builder = new(Literal.Length + 2 + coloredSpan.Length + spanEnd.Length);
    //         builder.Append(coloredSpan.Span);
    //     }
    //     else
    //         builder = new(Literal.Length + 2);
    //
    //     builder.Append(TData.CreateTruncating('"'));
    //     builder.Append(Literal.Span);
    //     builder.Append(TData.CreateTruncating('"'));
    //
    //     if (useColor)
    //         builder.Append(spanEnd.Span);
    //
    //     builder.Dispose();
    //     return builder.AsSpan();
    // }

    public override string ToString() =>
		'"' + Literal.Span.ConvertToCharacters() + '"';
}

