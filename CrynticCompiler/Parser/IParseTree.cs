using System;
using CrynticCompiler.Tokenizer;
using System.Numerics;
using CrynticCompiler.Parser.Nodes;

namespace CrynticCompiler.Parser;

public interface IParseTree<TData> : IReadOnlyList<Node>, IDisposable
    where TData : unmanaged, IBinaryInteger<TData>
{
    /// <summary>
    /// the tokenizer that is being used to get the tokens that will be parsed 
    /// </summary>
    public ITokenizer<TData> Tokenizer { get; }

    public IReadOnlyCollection<CompilerError<TData>> Errors { get; }

    /// <summary>
    /// gets the <see cref="ISemanticModel{TData}"/> for the enumerator
    /// </summary>
    /// <returns></returns>
    public ISemanticModel<TData> SemanticModel { get; }
}

