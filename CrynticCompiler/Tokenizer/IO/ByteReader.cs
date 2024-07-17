using System.Runtime.CompilerServices;

namespace CrynticCompiler.Tokenizer.IO;

public class ByteReader : Reader<byte>
{
    private readonly Stream stream;
    
    public override long Position
    {
        get => stream.Position;
        set => stream.Position = value;
    }

    public long Length => stream.Length;

    /// <summary>
    /// creates a new instance of the <see cref="ByteReader"/> class using a stream
    /// </summary>
    /// <param name="stream">the stream that will be used to read from</param>
    /// <exception cref="ArgumentException">throws when stream cannot be read from or is not seekable</exception>
    public ByteReader(Stream stream)
    {
        if (!stream.CanRead || !stream.CanSeek)
        {
            throw new ArgumentException("stream must be seekable and readable", nameof(stream));
        }
        this.stream = stream;
    }

    ~ByteReader()
    {
        Dispose();
    }
    

    public override int Read(Span<byte> span) =>
        stream.Read(span);

    public override void Close() =>
        stream.Close();

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        stream.Dispose();
    }
}