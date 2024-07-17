using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace CrynticCompiler.Tokenizer.IO;

public static class Reader
{
    /// <summary>
    /// creates a <see cref="Reader{TData}"/> from piece of <paramref name="memory"/>
    /// </summary>
    /// <param name="memory">the piece of memory that the reader will be created from</param>
    /// <typeparam name="TData">the data type of the piece of memory</typeparam>
    /// <returns>a new instance of an <see cref="Reader{TData}"/></returns>
    public static Reader<TData> Create<TData>(ReadOnlyMemory<TData> memory)
        where TData : unmanaged, IBinaryInteger<TData> =>
        new MemoryReader<TData>(memory);
    
    /// <summary>
    /// creates a reader from a <paramref name="path"/> to a file
    /// </summary>
    /// <param name="path">the path to the file</param>
    /// <param name="mode">the mode in which to open the file</param>
    /// <param name="encoding">the encoding of the file not necessary for bytes</param>
    /// <typeparam name="TData">the data type that the file will be opened in must be either a byte or char</typeparam>
    /// <returns>a new instance of an <see cref="Reader{TData}"/></returns>
    /// <exception cref="ArgumentException">thrown when <typeparamref name="TData"/> is not of type byte or char</exception>
    public static Reader<TData> Create<TData>(string path, FileMode mode = FileMode.Open,  Encoding? encoding = null)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (typeof(TData) != typeof(byte) &&
            typeof(TData) != typeof(char))
        {
            throw new ArgumentException("Can only create a reader with a path of type byte or char", nameof(TData));
        }

        FileReader reader = new FileReader(path, mode);
        if (typeof(TData) == typeof(byte))
        {
            Reader<byte> byteReader = new FileByteReader(reader);
            return Unsafe.As<Reader<TData>>(byteReader);
        }
        
        Reader<char> charReader = new CharReader(reader, encoding);
        return Unsafe.As<Reader<TData>>(charReader);
    }
}

/// <summary>
/// a helper interface used for the <see cref="Tokenizer{TData}"/>.
/// Can be created using the <see cref="Reader"/> class
/// </summary>
/// <typeparam name="TData">The data type that will be read</typeparam>
public abstract class Reader<TData> : IDisposable
    where TData : unmanaged, IBinaryInteger<TData>
{
    /// <summary>
    /// gets or sets the current position of the reader
    /// </summary>
    public abstract long Position { get; set; }

    /// <summary>
    /// reads a single <typeparamref name="TData"/> value from the reader
    /// </summary>
    /// <param name="eof"><see langword="true"/> if the end of file has been reached else false</param>
    /// <returns>the <typeparamref name="TData"/> value from the reader</returns>
    public virtual TData Read(out bool eof)
    {
        Unsafe.SkipInit(out TData value);

        Span<TData> span = new(ref value);
        int readAmount = Read(span);
        eof = readAmount == 0;
        return value;
    }

    /// <summary>
    /// reads <typeparamref name="TData"/> values from the reader and copies them into the <paramref name="span"/>
    /// </summary>
    /// <param name="span">the <see cref="Span{TData}"/> that the data will be copied into</param>
    /// <returns>the amount of <typeparamref name="TData"/> values read</returns>
    public abstract int Read(Span<TData> span);

    /// <summary>
    /// reads <typeparamref name="TData"/> values from the reader and copies them into the <paramref name="buffer"/>
    /// at the <paramref name="start"/> until the <paramref name="length"/> is reached 
    /// </summary>
    /// <param name="buffer">the array that the data will be copied into</param>
    /// <param name="start">the start position of the buffer</param>
    /// <param name="length">the length of the buffer</param>
    /// <returns>the amount of <typeparamref name="TData"/> values read</returns>
    public int Read(TData[] buffer, int start, int length) =>
        Read(buffer.AsSpan(start, length));

    /// <summary>
    /// closes the reader
    /// </summary>
    public abstract void Close();

    public abstract void Dispose();
}