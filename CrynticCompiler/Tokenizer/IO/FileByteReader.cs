using System.Numerics;

namespace CrynticCompiler.Tokenizer.IO;

internal class FileByteReader(FileReader reader) : Reader<byte>
{
    public override long Position
    {
        get => reader.position;
        set => reader.Seek(value, SeekOrigin.Begin);
    }

    ~FileByteReader()
    {
        Dispose();
    }

    public override byte Read(out bool eof)
    {
        int read = reader.ReadByte();
        eof = read == -1;
        return (byte)read;
    }

    public override int Read(Span<byte> span) =>
        reader.Read(span);

    public override void Close() =>
        reader.Close();

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        reader.Dispose();
    }
}