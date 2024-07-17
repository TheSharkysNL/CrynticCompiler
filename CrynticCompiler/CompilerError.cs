using System.Data.Common;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using CrynticCompiler.Extensions;
using System;
using System.Buffers;
using System.Reflection;
using System.Runtime.InteropServices;
using CrynticCompiler.Tokenizer.IO;

namespace CrynticCompiler;

public enum ExceptionSeverity
{
    Info,
    Warning,
    Error,
}

public enum ErrorLocation
{
    Tokenizer,
    Parser,
    Generator
}

/// <summary>
/// represents a error somewhere in the compiler
/// </summary>
/// <typeparam name="TData">the type of the data</typeparam>
[DebuggerDisplay("{Message, nq}")]
public class CompilerError<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    /// <summary>
    /// the line where the error occured
    /// </summary>
    public int Line { get; protected set; } = -1;
    /// <summary>
    /// the column where the error occured
    /// </summary>
    public int Column { get; protected set; } = -1;

    /// <summary>
    /// the line where the error stops
    /// if -1 then it is the same as the <see cref="Line"/>
    /// if -2 then it means it needs to show the whole line <see cref="WholeLine"/>
    /// </summary>
    public int TillLine { get; protected set; } = -1;
    /// <summary>
    /// the column where the error stops
    /// if -1 then it is the same as the <see cref="Column"/>
    /// </summary>
    public int TillColumn { get; protected set; } = -1;

    /// <summary>
    /// the error message
    /// </summary>
    public string Message { get; protected set; }

    /// <summary>
    /// the severity of the error
    /// </summary>
    public ExceptionSeverity Severity { get; protected set; }

    /// <summary>
    /// the location where the error occured
    /// </summary>
    public ErrorLocation Location { get; }

    /// <summary>
    /// the reader that was used when the error occured
    /// </summary>
    public Reader<TData>? Reader { get; }

    /// <summary>
    /// true if the whole line needs to be shown for the error else it will use the <see cref="CharacterAroundAmount"/> const
    /// </summary>
    public bool WholeLine => TillLine == -2;

    /// <summary>
    /// gets a set amount of characters around the error point
    /// </summary>
    protected const int CharacterAroundAmount = 6;

    /// <summary>
    /// gets the stacktrace for the error that occured
    /// </summary>
    public string? StackTrace { get; }

    // for converting the severity into the loglevel
    private static readonly LogLevel[] SevertityConverter = [LogLevel.Information, LogLevel.Warning, LogLevel.Error];
    // for converting the severity into a console color
    private static readonly ConsoleColor[] Colors = [ConsoleColor.White, ConsoleColor.Yellow, ConsoleColor.Red];

    /// <summary>
    /// Creates a compiler error with a given message and severity
    /// </summary>
    /// <param name="message">the message that you want to display</param>
    /// <param name="severity">the severity of the error</param>
    /// <param name="location">the location where the error occured</param>
    public CompilerError(string message, ErrorLocation location, ExceptionSeverity severity = default)
    {
        Message = message;
        Severity = severity;
        Location = location;
    }

    /// <summary>
    /// Creates a compiler error with a given message and severity at a line and column
    /// </summary>
    /// <param name="message">the message that you want to display</param>
    /// <param name="severity">the severity of the error</param>
    /// <param name="column">the column where the error occured</param>
    /// <param name="line">the line where the error occured</param>
    /// <param name="location">the location where the error occured</param>
    /// <param name="useStackTrace">true if the stacktrace of the c# function where the error occured should be included</param>
    public CompilerError(string message, int line, int column, ErrorLocation location, ExceptionSeverity severity = default, bool useStackTrace = false)
    {
        Message = message;
        Severity = severity;
        Line = line;
        Column = column;
        Location = location;
        if (useStackTrace)
        {
            StackTrace = GetStackTrace();
        }
    }

    /// <summary>
    /// Creates a compiler error with a given message. 
    /// It uses the data to support this message, 
    /// and shows part of the data at the given <paramref name="line"/> number (zero-based) and <paramref name="column"/> (zero-based).
    /// </summary>
    /// <param name="message">the message that you want to display</param>
    /// <param name="reader">the reader where the error occured</param>
    /// <param name="line">the line where the error occured (zero-based)</param>
    /// <param name="column">the column where the error occured (zero-based)</param>
    /// <param name="severity">the severity of the error</param>
    /// <param name="location">the location where the error occured</param>
    /// <param name="useStackTrace">true if the stacktrace of the c# function where the error occured should be included</param>
    public CompilerError(string message, Reader<TData> reader, int line, int column, ErrorLocation location, ExceptionSeverity severity = default, bool useStackTrace = false)
    {
        Message = message;
        Reader = reader;
        Line = line;
        Column = column;
        Severity = severity;
        Location = location;
        if (useStackTrace)
        {
            StackTrace = GetStackTrace();
        }
    }

    /// <summary>
    /// Creates a compiler error with a given message. 
    /// It uses the data to support this message, 
    /// and shows part of the data at the given <paramref name="line"/> number (zero-based) and <paramref name="column"/> (zero-based).
    /// it also allows you to select if the whole line from the <paramref name="data"/> is shown 
    /// </summary>
    /// <param name="message">the message that you want to display</param>
    /// <param name="reader">the reader where the error occured</param>
    /// <param name="line">the line where the error occured (zero-based)</param>
    /// <param name="column">the column where the error occured (zero-based)</param>
    /// <param name="wholeLine">true if the whole line needs to be shown for the error else it will use the <see cref="CharacterAroundAmount"/> const</param>
    /// <param name="severity">the severity of the error</param>
    /// <param name="location">the location where the error occured</param>
    public CompilerError(string message, Reader<TData> reader, int line, int column, bool wholeLine, ErrorLocation location, ExceptionSeverity severity = default)
    {
        Message = message;
        Reader = reader;
        Line = line;
        Severity = severity;
        Column = column;
        TillLine = -1 - Unsafe.As<bool, byte>(ref wholeLine);
        Location = location;
    }

    /// <summary>
    /// Creates a compiler error with a given message. 
    /// It uses the data to support this message, 
    /// and shows part of the data at the given <paramref name="line"/> number (zero-based) and <paramref name="column"/> (zero-based).
    /// it'll underline until the <paramref name="tillColumn"/> (zero-based) is reached 
    /// </summary>
    /// <param name="message">the message that you want to display</param>
    /// <param name="reader">the reader where the error occured</param>
    /// <param name="line">the line where the error occured (zero-based)</param>
    /// <param name="column">the column where the error occured (zero-based)</param>
    /// <param name="tillColumn">the column where the error section ends (zero-based)</param>
    /// <param name="severity">the severity of the error</param>
    /// <param name="location">the location where the error occured</param>
    public CompilerError(string message, Reader<TData> reader, int line, int column, int tillColumn, ErrorLocation location, ExceptionSeverity severity = default)
    {
        Message = message;
        Reader = reader;
        Line = line;
        Column = column;
        Severity = severity;
        TillColumn = tillColumn;
        Location = location;
    }

    /// <summary>
    /// Creates a compiler error with a given message. 
    /// It uses the data to support this message, 
    /// and shows part of the data at the given <paramref name="line"/> number (zero-based) and <paramref name="column"/> (zero-based).
    /// it'll underline until the <paramref name="tillColumn"/> (zero-based) is reached 
    /// it also allows you to select if the whole line from the <paramref name="data"/> is shown 
    /// </summary>
    /// <param name="message">the message that you want to display</param>
    /// <param name="reader">the reader where the error occured</param>
    /// <param name="line">the line where the error occured (zero-based)</param>
    /// <param name="column">the column where the error occured (zero-based)</param>
    /// <param name="wholeLine">true if the whole line needs to be shown for the error else it will use the <see cref="CharacterAroundAmount"/> const</param>
    /// <param name="tillColumn">the column where the error section ends (zero-based)</param>
    /// <param name="severity">the severity of the error</param>
    /// <param name="location">the location where the error occured</param>
    /// <param name="useStackTrace">true if the stacktrace of the c# function where the error occured should be included</param>
    public CompilerError(string message, Reader<TData> reader, int line, int column, int tillColumn, bool wholeLine, ErrorLocation location, ExceptionSeverity severity = default, bool useStackTrace = false)
    {
        Message = message;
        Reader = reader;
        Line = line;
        Column = column;
        Severity = severity;
        TillColumn = tillColumn;
        TillLine = -1 - Unsafe.As<bool, byte>(ref wholeLine);
        Location = location;
        if (useStackTrace)
        {
            StackTrace = GetStackTrace();
        }
    }

    /// <summary>
    /// Creates a compiler error with a given message. 
    /// It uses the data to support this message, 
    /// and shows part of the data at the given <paramref name="line"/> number (zero-based) and <paramref name="column"/> (zero-based).
    /// it'll underline until the <paramref name="tillColumn"/> (zero-based) is reached at the <paramref name="tillLine"/> (zero-based)
    /// </summary>
    /// <param name="message">the message that you want to display</param>
    /// <param name="reader">the reader where the error occured</param>
    /// <param name="line">the line where the error occured (zero-based)</param>
    /// <param name="column">the column where the error occured (zero-based)</param>
    /// <param name="tillColumn">the column where the error section ends (zero-based)</param>
    /// <param name="tillLine">the line where the error section ends (zero-based)</param>
    /// <param name="severity">the severity of the error</param>
    /// <param name="location">the location where the error occured</param>
    /// <param name="useStackTrace">true if the stacktrace of the c# function where the error occured should be included</param>
    public CompilerError(string message, Reader<TData> reader, int line, int column, int tillLine, int tillColumn, ErrorLocation location, ExceptionSeverity severity = default, bool useStackTrace = false)
    {
        Message = message;
        Reader = reader;
        Line = line;
        Column = column;
        Severity = severity;
        TillLine = tillLine;
        TillColumn = tillColumn;
        Location = location;
        if (useStackTrace)
        {
            StackTrace = GetStackTrace();
        }
    }
    
    /// <summary>
    /// Creates a compiler error with a given message. 
    /// and shows the given <paramref name="line"/> number (zero-based) and <paramref name="column"/> (zero-based).
    /// it'll underline until the <paramref name="tillColumn"/> (zero-based) is reached at the <paramref name="tillLine"/> (zero-based)
    /// </summary>
    /// <param name="message">the message that you want to display</param>
    /// <param name="line">the line where the error occured (zero-based)</param>
    /// <param name="column">the column where the error occured (zero-based)</param>
    /// <param name="tillColumn">the column where the error section ends (zero-based)</param>
    /// <param name="tillLine">the line where the error section ends (zero-based)</param>
    /// <param name="severity">the severity of the error</param>
    /// <param name="location">the location where the error occured</param>
    /// <param name="useStackTrace">true if the stacktrace of the c# function where the error occured should be included</param>
    public CompilerError(string message, int line, int column, int tillLine, int tillColumn, ErrorLocation location, ExceptionSeverity severity = default, bool useStackTrace = false)
    {
        Message = message;
        Line = line;
        Column = column;
        Severity = severity;
        TillLine = tillLine;
        TillColumn = tillColumn;
        Location = location;
        if (useStackTrace)
        {
            StackTrace = GetStackTrace();
        }
    }

    private static string GetStackTrace()
    {
        StackTrace trace = new();
        StackFrame[] frames = trace.GetFrames();
        StackFrame? frame = GetFrame(frames);
        if (frame is null)
        {
            return string.Empty;
        }

        ValueListBuilder<char> builder = new(stackalloc char[512]);
        builder.Append("C# Stacktrace: In ");
        
        MethodBase? method = frame.GetMethod();
        Debug.Assert(method is not null);

        if (method.DeclaringType is not null)
        {
            builder.Append(method.DeclaringType.FullName);
            builder.Append('.');
        }
        
        builder.Append(method.Name);
        builder.Append('(');

        ParameterInfo[] parameters = method.GetParameters();
        if (parameters.Length != 0)
        {
            ParameterInfo firstParameter = parameters[0];

            AppendParameter(ref builder, firstParameter);
            for (int i = 1; i < parameters.Length; i++)
            {
                builder.Append(", ");
                AppendParameter(ref builder, parameters[i]);
            }
        }
        
        builder.Append(");");

        int line = frame.GetFileLineNumber();
        if (line != 0)
        {
            builder.Append(" at line: ");
            AppendInt(ref builder, line);
            builder.Append(';');
        }

        int column = frame.GetFileColumnNumber();
        if (column != 0)
        {
            builder.Append(" at column: ");
            AppendInt(ref builder, column);
            builder.Append(';');
        }

        ReadOnlySpan<char> span = builder.AsSpan();
        builder.Dispose();
        return span.ToString();
    }

    private static StackFrame? GetFrame(StackFrame[] frames)
    {
        bool foundAddErrorFunction = false;
        for (int i = 0; i < frames.Length; i++)
        {
            StackFrame frame = frames[i];

            MethodBase? method = frame.GetMethod();
            if (method is null)
            {
                continue;
            }

            if (method.Name == "AddError")
            {
                foundAddErrorFunction = true;
            } 
            else if (foundAddErrorFunction || method.Module.Name != "CrynticCompiler.dll")
            {
                return frame;
            }
        }

        return null;
    }

    private static void AppendParameter(ref ValueListBuilder<char> builder, ParameterInfo parameter)
    {
        builder.Append(parameter.ParameterType.Name);
        builder.Append(' ');
        builder.Append(parameter.Name);
    }

    private static void AppendInt(ref ValueListBuilder<char> builder, int value)
    {
        Span<char> span = builder.AppendSpan(10);
        bool canFormat = value.TryFormat(span, out int charsWritten);
        Debug.Assert(canFormat);
        builder.Length -= span.Length - charsWritten;
    }

    /// <summary>
    /// writes an integer to builder
    /// </summary>
    /// <param name="builder">the builder to write to</param>
    /// <param name="value">the integer value</param>
    private static void WriteIntToBuilder(ref ValueListBuilder<char> builder, int value)
    {
        Span<char> span = builder.AppendSpan(10); // 10 is the max amount of digits that a int32 can be in base 10 
        value.TryFormat(span, out int charsWritten);
        builder.Length -= 10 - charsWritten;
    }

    private static bool TryGetLine(Reader<TData> reader, int line, out long position) =>
        TryGetLine(reader, 0, 0, line, out position);

    private static bool TryGetLine(Reader<TData> reader, long start, int startLine, int line, out long position)
    {
        if (startLine == line)
        {
            position = start;
            return true;
        }
        TData[] buffer = ArrayPool<TData>.Shared.Rent(1024);

        reader.Position = start + 1;
        int bytesRead;
        TData newLineChar = TData.CreateTruncating('\n');
        while ((bytesRead = reader.Read(buffer.AsSpan())) != 0)
        {
            int currentLine = startLine;
            startLine += buffer.AsSpan(0, bytesRead).Count(newLineChar);
            if (startLine >= line)
            {
                int index = -1;
                for (; currentLine < line; currentLine++)
                {
                    index = Array.IndexOf(buffer, newLineChar, index + 1);
                    Debug.Assert(index != -1);
                }

                ArrayPool<TData>.Shared.Return(buffer);
                position = reader.Position - (bytesRead - index);
                return true;
            }
        }

        ArrayPool<TData>.Shared.Return(buffer);
        position = 0;
        return false;
    }

    /// <summary>
    /// writes specific characters according to a <paramref name="predicate"/>
    /// </summary>
    /// <param name="builder">the builder to write to</param>
    /// <param name="index">the index to start at in the <see cref="Reader"/> array</param>
    /// <param name="length">the amount of characters</param>
    /// <param name="previousLineIndex">the position of the previous new line: '\n'</param>
    /// <param name="predicate">the characters to write</param>
    /// <returns>the start of the underlining part of the error</returns>
    private unsafe int WritePredicateCharactersToBuilder(ref ValueListBuilder<char> builder, long index, long length, long previousLineIndex, Predicate<char> predicate)
    {
        Debug.Assert(Reader is not null);

        Reader<TData> reader = Reader;

        long start;
        long end;
        if (!WholeLine)
        {
            start = long.Max(0, index - CharacterAroundAmount);
            end = index + length + CharacterAroundAmount;
        }
        else
        {
            start = previousLineIndex;

            end = !TryGetLine(reader, previousLineIndex, 0, 1, out long position) 
                ? reader.Position 
                : position;
        }

        int spanLength = (int)(end - start);
        Span<char> span = builder.AppendSpan(spanLength);
        int spanIndex = 0;

        byte[] arr = ArrayPool<byte>.Shared.Rent(spanLength * Unsafe.SizeOf<TData>());
        Span<TData> dataSpan = MemoryMarshal.Cast<byte, TData>(arr.AsSpan(0, spanLength * Unsafe.SizeOf<TData>()));

        reader.Position = start;
        end = reader.Read(dataSpan);

        ref TData dataRef = ref MemoryMarshal.GetReference(dataSpan);
        for (int i = 0; i < end; i++)
        {
            char c;
            if (typeof(TData) == typeof(byte))
                c = (char)Unsafe.As<TData, byte>(ref Unsafe.Add(ref dataRef, i));
            else
                c = Unsafe.As<TData, char>(ref Unsafe.Add(ref dataRef, i));

            if (predicate(c))
                span[spanIndex++] = c;
        }

        int charactersNotWritten = span.Length - spanIndex;
        builder.Length -= charactersNotWritten;

        return (int)(index - start) + 1;
    }

    /// <summary>
    /// writes the exception underlining to the builder
    /// </summary>
    /// <param name="builder">the builder that will be written to</param>
    /// <param name="extractLineLength"></param>
    /// <param name="start">the start of the underlining</param>
    /// <param name="length">the length of the underlining</param>
    private static void WriteUnderliningToBuilder(ref ValueListBuilder<char> builder, int start, long length)
    {
        builder.Append('\n');

        Span<char> whitespace = builder.AppendSpan(start);
        whitespace.Fill(' ');

        Span<char> underlining = builder.AppendSpan((int)length);
        underlining.Fill('~');
    }

    /// <summary>
    /// writes the extract or data part <see cref="Data"/> to the <paramref name="builder"/>
    /// </summary>
    /// <param name="builder">the builder that will be written to</param>
    /// <exception cref="InvalidOperationException"></exception>
    private void WriteExtractToBuilder(ref ValueListBuilder<char> builder)
    {
        if (Reader is null || Line == -1) return;
        
        if (!TryGetLine(Reader, Line, out long position))
            throw new InvalidOperationException($"cannot find line: {Line}, the given data doesn't have that many lines");

        long column = position;
        if (Column != -1)
            column += Column;

        long endColumn;
        if (TillLine > -1)
        {
            if (!TryGetLine(Reader, position, Line, TillLine, out long tillLinePosition))
                throw new InvalidOperationException($"cannot find line: {Line}, the given data doesn't have that many lines");
            endColumn = tillLinePosition;
            if (TillColumn != -1)
                endColumn += TillColumn;
        }
        else if (TillColumn != -1)
        {
            if (TillColumn < Column)
                throw new InvalidOperationException($"the TillColumn: {TillColumn} must come after the Column: {Column}");
            endColumn = position + TillColumn;
        }
        else
        {
            if (!TryGetLine(Reader, position, Line, Line + 1, out long nextLinePosition))
                endColumn = Reader.Position;
            else if (nextLinePosition <= Column)
                endColumn = position + Column + 1;
            else
                endColumn = nextLinePosition + position;
        }

        const string Extract = "Line extract: ";
        builder.Append('\n');
        builder.Append(Extract);
        builder.Append('\n');
        long length = long.Abs(endColumn - column);
        int start = WritePredicateCharactersToBuilder(ref builder, column, length, position, c => c != '\n' & c != '\r') - 1;
        WriteUnderliningToBuilder(ref builder, start + 1, length);
    }

    /// <summary>
    /// gets the full message that is contained within the error
    /// </summary>
    /// <returns>the message</returns>
    public string GetMessageString()
    {
        ValueListBuilder<char> builder = new(stackalloc char[512]);

        if (Severity == ExceptionSeverity.Error)
            builder.Append("An error occured: ");
        else if (Severity == ExceptionSeverity.Warning)
            builder.Append("Warning: ");
        else if (Severity == ExceptionSeverity.Info)
            builder.Append("Info: ");
        builder.Append(Message);

        if (Line != -1)
        {
            if (!Message.EndsWith('.'))
            {
                builder.Append(';');
            }
            builder.Append(" At line: ");

            WriteIntToBuilder(ref builder, Line + 1);

            if (Column != -1)
            {
                builder.Append(", At column: ");

                WriteIntToBuilder(ref builder, Column + 1);
            }
        }

        if (TillColumn != -1)
        {
            if (TillLine > -1)
            {
                builder.Append("; Till line: ");

                WriteIntToBuilder(ref builder, TillLine + 1);
            }
            else
            {
                builder.Append("; Till line: ");

                WriteIntToBuilder(ref builder, Line + 1); // stay on same line
            }

            builder.Append(", Till column: ");
            WriteIntToBuilder(ref builder, TillColumn + 1);
        }

        WriteExtractToBuilder(ref builder);

        if (StackTrace is not null)
        {
            builder.Append('\n');
            builder.Append(StackTrace);
        }

        ReadOnlySpan<char> chars = builder.AsSpan();
        builder.Dispose();
        return new(chars);
    }

    /// <summary>
    /// writes <see cref="GetMessageString"/> to the console using the severity as colors
    /// </summary>
    public virtual void WriteToConsole()
    {
        ConsoleColor original = Console.ForegroundColor;

        Console.ForegroundColor = Colors[(int)Severity];

        string message = GetMessageString();
        Console.Error.WriteLine(message);
        Console.ForegroundColor = original;
    }

    /// <summary>
    /// writes <see cref="GetMessageString"/> to a <see cref="ILogger"/>
    /// </summary>
    /// <param name="logger">the logger to write to</param>
    public virtual void WriteToLogger(ILogger logger)
    {
        LogLevel logLevel = SevertityConverter[(int)Severity];
        if (logger.IsEnabled(logLevel))
#pragma warning disable // GetMessageString should follow a template 
            logger.Log(logLevel, GetMessageString());
        else
            logger.LogDebug(GetMessageString());
#pragma warning enable
    }

    [Serializable]
    protected sealed class CompilerException : Exception
    {
        public CompilerException() { }

        public CompilerException(string message)
            : base(message) { }

        public CompilerException(string message, Exception inner)
            : base(message, inner) { }
    }

    /// <summary>
    /// throw the error as an exception
    /// </summary>
    /// <exception cref="CompilerException"></exception>
    public virtual void Throw() =>
        throw new CompilerException(GetMessageString());
}
