using System.Numerics;
using System.Runtime.CompilerServices;

namespace CrynticCompiler.Tokenizer.IO;

public class MemoryReader<TData> : Reader<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    private readonly ReadOnlyMemory<TData> memory;
    private int position;
    
    public override long Position
    {
        get => position;
        set => position = checked((int)value);
    }

    public long Length => memory.Length;

    public MemoryReader(ReadOnlyMemory<TData> memory)
    {
        this.memory = memory;
    }
    
    public override int Read(Span<TData> span)
    {
        if (span.Length == 0)
        {
            return 0;
        }

        int position = this.position;
        ReadOnlyMemory<TData> memory = this.memory;
        ReadOnlySpan<TData> positionSpan = memory.Span[position..];

        int lengthLeft = memory.Span.Length - position;
        int readLength = lengthLeft < span.Length ? lengthLeft : span.Length;
        if (readLength == 0)
        {
            return 0;
        }
        
        positionSpan[..readLength].CopyTo(span);
        this.position += readLength;
        return readLength;
    }

    public override void Close()
    {
    }

    public override void Dispose()
    {
    }
}