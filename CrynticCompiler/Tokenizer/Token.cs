using System.Diagnostics;
using System.Numerics;
using CrynticCompiler.Extensions;
using System.Reflection;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace CrynticCompiler.Tokenizer;

[DebuggerDisplay("{DebuggerDisplay, nq}")]
public readonly struct Token<TData>
	where TData : unmanaged, IBinaryInteger<TData>
{
	/// <summary>
	/// the data of the token
	/// </summary>
	public readonly TokenData<TData> Data;

	/// <summary>
	/// the type of the token using the <see cref="TokenType"/> static variables
	/// </summary>
	public readonly int Type;

	public Token(ReadOnlyMemory<TData> data, int type)
	{
		Data = new(data);
		Type = type;
	}

	public Token(TokenData<TData> data, int type)
	{
		Data = data;
		Type = type;
	}

	public Token(TData data, int type)
	{
		Data = new(data);
		Type = type;
	}

	public Token(int type)
	{
		Data = default;
		Type = type;
	}

	private string DebuggerDisplay
	{
		get
		{
			if (Data.Length > 0)
				return Data.Span.ConvertToCharacters();

			return TokenizerHelpers.TryGetTokenTypeName(Type, out string? name) ? name : "No Display";
		}
	}

	public override string ToString() =>
		DebuggerDisplay;
}

