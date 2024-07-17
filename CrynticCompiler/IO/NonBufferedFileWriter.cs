using System;
using Microsoft.Win32.SafeHandles;

namespace CrynticCompiler.IO;

internal sealed class NonBufferedFileWriter : Stream
{
	private readonly SafeFileHandle handle;
    private long position;

    public override bool CanRead => false;

    public override bool CanSeek => true;

    public override bool CanWrite => true;

    public override long Length => RandomAccess.GetLength(handle);

    public override long Position { get => position; set => Seek(value, SeekOrigin.Begin); }

    public NonBufferedFileWriter(string path, FileMode mode, bool isAsync)
	{
		FileOptions options = isAsync ? FileOptions.WriteThrough | FileOptions.Asynchronous : FileOptions.WriteThrough;
		handle = File.OpenHandle(path, mode, FileAccess.Write, FileShare.Write, options); // write through because there is no buffer
	}

    public override void Flush() // nothing to flush
    {
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        long newPosition;
        if (origin == SeekOrigin.Begin)
            newPosition = offset;
        else if (origin == SeekOrigin.Current)
            newPosition = position + offset;
        else
            newPosition = position - offset;

        if (newPosition < 0)
            throw new ArgumentException("invalid offset", nameof(offset));

        position = newPosition;
        return newPosition;
    }

    public override void SetLength(long value) =>
        RandomAccess.SetLength(handle, value);

    public override void Write(byte[] buffer, int offset, int count) =>
        Write(buffer.AsSpan(offset, count));

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        long writeOffset = position;
        position += buffer.Length;
        RandomAccess.Write(handle, buffer, writeOffset);
    }

    public override void WriteByte(byte value) =>
        Write(new(value));

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        int bufferLength = buffer.Length;
        long writeOffset = Interlocked.Add(ref position, bufferLength) - bufferLength;
        return RandomAccess.WriteAsync(handle, buffer, writeOffset, cancellationToken);
    }

    public override void Close()
    {
        handle.Close();
        base.Close();
    }
}

