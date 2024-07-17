using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CrynticCompiler.Extensions;

namespace CrynticCompiler.Tokenizer.IO;

/// <summary>
/// A not very efficient reader for reading chars
/// </summary>
public class CharReader : Reader<char>
{
    private readonly StreamReader reader;
    
    public override long Position
    {
        get => reader.GetPosition();
        set => reader.SetPosition(value);
    }

    public CharReader(Stream stream, Encoding? encoding = null)
    {
        if (!stream.CanRead || !stream.CanSeek)
        {
            throw new ArgumentException("stream must be seekable and readable", nameof(stream));
        }

        bool detectEncoding = encoding is null;
        reader = new(stream, encoding, detectEncoding);
    }

    ~CharReader()
    {
        Dispose();
    }

    public override char Read(out bool eof)
    {
        int character = reader.Read();
        eof = character == -1;
        return (char)character;
    }

    public override int Read(Span<char> span) =>
        reader.Read(span);

    public override void Close() =>
        reader.Close();

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        reader.Dispose();
    }
    
}

// /// <summary>
// /// currently doesn't work 
// /// </summary>
// public class CharReader : Reader<char>
// {
//     private readonly Stream stream;
//
//     private const int DefaultBufferSize = 1024;
//     
//     private byte[]? buffer;
//     private readonly int bufferSize;
//
//     private Span<byte> ByteBuffer => buffer.AsSpan(0, bufferSize);
//     private Span<char> CharBuffer => MemoryMarshal.Cast<byte, char>(buffer.AsSpan(bufferSize));
//     
//
//     private int byteBufferPos;
//     private int byteBufferLength;
//     
//     private int charBufferPos;
//     private int charBufferLength;
//     
//     private readonly int maxCharsPerBuffer;
//
//     private Encoding encoding;
//     private readonly Decoder decoder;
//     
//     public override long Position
//     {
//         get => stream.Position - maxCharsPerBuffer + charBufferPos;
//         set
//         {
//             stream.Position = value;
//             charBufferLength = 0;
//         }
//     }
//
//     /// <summary>
//     /// creates a new instance of the <see cref="CharReader"/> class using a stream
//     /// </summary>
//     /// <param name="stream">the stream that will be used to read from</param>
//     /// <exception cref="ArgumentException">throws when reader cannot be read from or is not seekable</exception>
//     public CharReader(Stream stream)
//     {
//         if (!stream.CanRead || !stream.CanSeek)
//         {
//             throw new ArgumentException("stream must be seekable and readable", nameof(stream));
//         }
//
//         this.stream = stream;
//
//         bufferSize = DefaultBufferSize;
//         
//         encoding = Encoding.UTF8;
//         decoder = encoding.GetDecoder();
//         maxCharsPerBuffer = encoding.GetMaxCharCount(bufferSize);
//         
//         
//     }
//
//     ~CharReader()
//     {
//         Dispose();
//     }
//
//     public override int Read(Span<char> buffer)
//     {
//         EnsureBufferAllocated(bufferSize);
//         
//         int charsRead = 0;
//         bool readToUserBuffer = false;
//         int count = buffer.Length;
//         while (count > 0)
//         {
//             int n = charBufferLength - charBufferPos;
//             if (n <= 0)
//             {
//                 n = ReadBuffer(buffer.Slice(charsRead), out readToUserBuffer);
//             }
//             if (n == 0)
//             {
//                 break; 
//             }
//             if (n > count)
//             {
//                 n = count;
//             }
//             if (!readToUserBuffer)
//             {
//                 ref char charBufferRef = ref MemoryMarshal.GetReference(CharBuffer);
//                 charBufferRef = ref Unsafe.Add(ref charBufferRef, charBufferPos);
//                 Span<char> bufferSpan = MemoryMarshal.CreateSpan(ref charBufferRef, n);
//                 bufferSpan.CopyTo(buffer.Slice(charsRead));
//                 charBufferPos += n;
//             }
//  
//             charsRead += n;
//             count -= n;
//         }
//  
//         return charsRead;
//     }
//
//     private int ReadBuffer(Span<char> userBuffer, out bool readToUserBuffer)
//     {
//         charBufferLength = 0;
//         charBufferPos = 0;
//
//         bool eofReached = false;
//         int charsRead = 0;
//         
//         readToUserBuffer = userBuffer.Length >= maxCharsPerBuffer;
//
//         do
//         {
//             byteBufferLength = stream.Read(ByteBuffer);
//
//             if (byteBufferLength == 0)
//             {
//                 eofReached = true;
//                 break;
//             }
//             
//             if (readToUserBuffer)
//             {
//                 charsRead = decoder.GetChars(buffer.AsSpan(0, byteBufferLength), userBuffer,
//                     flush: false);
//             }
//             else
//             {
//                 charsRead = decoder.GetChars(ByteBuffer, CharBuffer, flush: false);
//                 charBufferLength = charsRead;
//             }
//         } while (charsRead == 0);
//
//         if (eofReached)
//         {
//             if (readToUserBuffer)
//             {
//                 charsRead = decoder.GetChars(buffer.AsSpan(0, byteBufferLength), userBuffer, flush: true);
//             }
//             else
//             {
//                 charsRead = decoder.GetChars(buffer.AsSpan(0, byteBufferLength), CharBuffer, flush: true);
//                 charBufferLength = charsRead; 
//             }
//
//             byteBufferPos = 0;
//             byteBufferLength = 0;
//         }
//
//         return charsRead;
//     }
//
//     public override void Close() =>
//         stream.Close();
//
//     public override void Dispose()
//     {
//         GC.SuppressFinalize(this);
//         stream.Dispose();
//         buffer = null;
//     }
//     
//     [MemberNotNull(nameof(buffer))]
//     private void EnsureBufferAllocated(int size)
//     {
//         if (buffer is null)
//         {
//             AllocateBuffer(size);
//         }
//     }
//
//
//     /// <summary>
//     /// see <see cref="BufferedFileStreamStrategy.AllocateBuffer"/>
//     /// </summary>
//     /// <param name="size"></param>
//     [MemberNotNull(nameof(buffer))]
//     [MethodImpl(MethodImplOptions.NoInlining)]
//     private void AllocateBuffer(int size)
//     {
//         Interlocked.CompareExchange(ref buffer, GC.AllocateUninitializedArray<byte>(size + maxCharsPerBuffer * Unsafe.SizeOf<char>()), null);
//     }
//}