using System;
using System.Numerics;
using CrynticCompiler.Parser;

namespace CrynticCompiler.Generator;

public interface IGenerator<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    /// <summary>
    /// The type of generator this is
    /// </summary>
    public GeneratorType Type { get; }

    /// <summary>
    /// the parser for the generator
    /// </summary>
    public IParseTree<TData> Parser { get; }


    /// <summary>
    /// generates data that was parsed using the <see cref="Parser"/>
    /// </summary>
    /// <returns>the generated data</returns>
    public ReadOnlySpan<TData> Generate();
    /// <summary>
    /// generates data using the <see cref="Parser"/> and writes that into a given <paramref name="stream"/>
    /// </summary>
    /// <param name="stream">the stream to write into</param>
    public void Generate(Stream stream);
    /// <summary>
    /// generates data using the <see cref="Parser"/> and writes that into a given <paramref name="stream"/> asynchronously
    /// </summary>
    /// <param name="stream">the stream to write into</param>
    /// <param name="token">a <see cref="CancellationToken"/></param>
    public Task GenerateAsync(Stream stream, CancellationToken token = default);
}

