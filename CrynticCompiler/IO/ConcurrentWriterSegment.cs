namespace CrynticCompiler.IO;

public sealed class ConcurrentWriterSegment : Stream
{
    internal long selfPosition;
    private readonly ConcurrentWriter writer;

	internal ConcurrentWriterSegment(ConcurrentWriter writer)
        : this(writer, 0)
    { }

    internal ConcurrentWriterSegment(ConcurrentWriter writer, long position)
    {
        this.writer = writer;
        selfPosition = position;
    }

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => writer.writeStream.Length;

    public override long Position
    {
        get => selfPosition;
        set => throw new NotImplementedException(); 
    }

    public override void Flush() =>
        writer.writeStream.Flush();

    public override Task FlushAsync(CancellationToken cancellationToken) =>
        writer.writeStream.FlushAsync(cancellationToken);

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException(); // could be implemented but could cost a lot of resources...
    }

    public override void SetLength(long value) =>
        writer.writeStream.SetLength(value);

    private long GetActualPosition()
    {
        long position = selfPosition;
        int writerCount = writer.Count;

        int i = 0;
        for (; i < writerCount; i++)
        {
            ConcurrentWriterSegment segment = writer[i];
            if (ReferenceEquals(segment, this))
                break;

            position += segment.selfPosition;
        }
        if (i == writerCount)
            throw new InvalidOperationException($"segment must be found within the {nameof(ConcurrentWriter)}");

        return position;
    }

    public override void Write(byte[] buffer, int offset, int count) =>
        Write(buffer.AsSpan(offset, count));

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        Stream stream = writer.writeStream;
        stream.Position = GetActualPosition();
        selfPosition += buffer.Length;
        stream.Write(buffer);
    }

    public override void WriteByte(byte value) =>
        writer.writeStream.WriteByte(value);

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        Stream stream = writer.writeStream;
        stream.Position = GetActualPosition();
        Interlocked.Add(ref selfPosition, buffer.Length);
        return stream.WriteAsync(buffer, cancellationToken);
    }

    public override void Close()
    {
        writer.Remove(this);
        base.Close();
    }
}

