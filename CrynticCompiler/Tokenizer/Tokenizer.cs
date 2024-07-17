using System;
using System.Buffers;
using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CrynticCompiler.Collections;
using CrynticCompiler.Extensions;
using CrynticCompiler.Tokenizer.IO;
using CrynticCompiler.Extensions;

namespace CrynticCompiler.Tokenizer;

/// <summary>
/// a premade tokenizer implementing the <see cref="ITokenizer{TData}"/>
/// also has a premade <see cref="ITokenEnumerator{TData}"/> by calling the <see cref="ITokenizer{TData}.GetEnumerator"/>
/// </summary>
/// <typeparam name="TData">the type of data that will be tokenized</typeparam>
public abstract class Tokenizer<TData> : ITokenizer<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    /// <summary>
    /// the reader that the tokenizer uses
    /// </summary>
    public Reader<TData> Reader { get; set; }

    /// <summary>
    /// the keywords within the tokenizer
    /// </summary>
    public IReadOnlyDictionary<ReadOnlyMemory<TData>, int>? Keywords { get; }

    /// <summary>
    /// true if the tokenizer will tokenize whitespace else false
    /// </summary>
    protected virtual bool TokenizeWhiteSpace => false;
    /// <summary>
    /// true if the tokenizer will tokenize comments else false
    /// </summary>
    protected virtual bool TokenizeComments => false;
    /// <summary>
    /// true if the tokenizer will tokenizer new lines else false. if <see cref="TokenizeWhiteSpace"/> is true then it will also tokenize whitespace
    /// </summary>
    protected virtual bool TokenizeNewLines => false;

    /// <summary>
    /// true if tokens using the type <see cref="TokenType.None"/> should be tokenized else false
    /// </summary>
    protected virtual bool TokenizeNoneTokens => false;

    /// <summary>
    /// the current mode the compiler is in. Is used to show part of the stacktrace when an error occurs whilst in <see cref="CompilationMode.Debug"/>
    /// </summary>
    protected abstract CompilationMode Mode { get; }

    /// <summary>
    /// Creates a tokenizer using a reader to read the data.
    /// Will also add keywords to the tokenizer
    /// </summary>
    /// <param name="reader">the data that will be tokenized</param>
    /// <param name="keywords">the keywords that will be tokenized </param>
    protected Tokenizer(Reader<TData> reader, IReadOnlyDictionary<ReadOnlyMemory<TData>, int>? keywords = null)
    {
        Reader = reader;
        Keywords = keywords;
    }

    /// <summary>
    /// checks if a character is the start of an identifier
    /// </summary>
    /// <param name="value">the character to check</param>
    /// <param name="multiLine">true if multiline else false</param>
    /// <returns>true if the <paramref name="value"/> is the start of an identifier else false</returns>
    protected virtual bool IsIdentifierStartCharacter(TData value, out bool multiLine)
    {
        multiLine = false;
        return TokenizerHelpers.IsLetter(value) | TokenizerHelpers.IsCharacter(value, '_');
    }

    /// <summary>
    /// checks if a character is an identifier character
    /// </summary>
    /// <param name="value">the character to check</param>
    /// <returns>true if the <paramref name="value"/> is the an identifier character else false</returns>
    protected virtual bool IsIdentifierCharacter(TData value) =>
        TokenizerHelpers.IsLetter(value) || TokenizerHelpers.IsDigit(value) | TokenizerHelpers.IsCharacter(value, '_');

    /// <summary>
    /// uses the <see cref="Keywords"/> dictionary to find if a identifier is a keyword
    /// can be overriden to use other means to find a keyword
    /// </summary>
    /// <param name="identifier">the identifier to check for keywords</param>
    /// <param name="keywordType">the type of keyword that is found</param>
    /// <returns>true if the identifier is a keyword else false</returns>
    protected virtual bool IsKeyword(ReadOnlyMemory<TData> identifier, out int keywordType)
    {
        if (Keywords is null)
        {
            Unsafe.SkipInit(out keywordType);
            return false;
        }

        return Keywords.TryGetValue(identifier, out keywordType);
    }

    /// <summary>
    /// checks if a character is the start of a digit
    /// </summary>
    /// <param name="value">the character to check</param>
    /// <param name="multiLine">true if multiline else false</param>
    /// <returns>true if the <paramref name="value"/> is the start of a digit else false</returns>
    protected virtual bool IsDigitStart(TData value, out bool multiLine)
    {
        multiLine = false;
        
        return TokenizerHelpers.IsDigit(value);
    }

    /// <summary>
    /// checks if a character is a digit character
    /// </summary>
    /// <param name="value">the character to check</param>
    /// <returns>true if the <paramref name="value"/> is the a digit character else false</returns>
    protected virtual bool IsDigitCharacter(TData value) =>
        TokenizerHelpers.IsDigit(value) | TokenizerHelpers.IsCharacter(value, 'x') || TokenizerHelpers.IsCharacter(value, 'b') | TokenizerHelpers.IsCharacter(value, 'o');

    /// <summary>
    /// checks if a <paramref name="span"/> is a number and
    /// gives the <paramref name="type"/> of number that it is
    /// eg: hexadecimal, decimal...
    /// assumptions can be made that the <paramref name="span"/> contains only values from <see cref="IsDigitCharacter(TData)"/>
    /// </summary>
    /// <param name="span">the span to check in</param>
    /// <param name="type">the type of number the <paramref name="span"/> represents, eg: hexadecimal, decimal...</param>
    /// <returns>true if the given <paramref name="span"/> is a number else false</returns>
    protected virtual bool IsNumber(ReadOnlySpan<TData> span, out int type)
    {
        if (span.Length <= 1)
        {
            type = TokenType.Number;
            return true;
        }

        // first character won't get checked as it should be checked by the parser
        TData secondCharacter = span[1]; 
        if (TokenizerHelpers.IsCharacter(secondCharacter, 'x'))
        {
            type = TokenType.Hexadecimal;
            return true;
        }

        if (TokenizerHelpers.IsCharacter(secondCharacter, 'b'))
        {
            type = TokenType.Binary;
            return true;
        }

        if (TokenizerHelpers.IsCharacter(secondCharacter, 'o'))
        {
            type = TokenType.Octal;
            return true;
        }

        type = TokenType.Number;
        return true;
    }

    /// <summary>
    /// checks if the value is a scope in character
    /// </summary>
    /// <param name="value">the character to check</param>
    /// <param name="scopeChange">the amount the scope should change negative numbers for going out of a scope and positive numbers for going into a scope</param>
    /// <returns>true if the <paramref name="value"/> is the a scope in character else false</returns>
    protected virtual bool IsScopeIn(TData value, out int scopeChange)
    {
        scopeChange = 1;
        return TokenizerHelpers.IsCharacter(value, '{');
    }


    /// <summary>
    /// checks if the value is a scope out character
    /// </summary>
    /// <param name="value">the character to check</param>
    /// <param name="scopeChange">the amount the scope should change negative numbers for going out of a scope and positive numbers for going into a scope</param>
    /// <returns>true if the <paramref name="value"/> is the a scope out character else false</returns>
    protected virtual bool IsScopeOut(TData value, out int scopeChange)
    {
        scopeChange = -1;
        return TokenizerHelpers.IsCharacter(value, '}');
    }

    /// <summary>
    /// checks if the value is a comment character
    /// </summary>
    /// <param name="value">the character to check</param>
    /// <param name="reader">a reader that allows you to read the next characters if needed</param>
    /// <returns>true if the <paramref name="value"/> is a comment character else false</returns>
    protected virtual bool IsComment(TData value, ref CharacterReader reader)
    {
        if (!TokenizerHelpers.IsCharacter(value, '/'))
            return false;

        TData character = reader.Read(out _);
        return TokenizerHelpers.IsCharacter(character, '/');
    }

    /// <summary>
    /// checks if the value is the start of a multiline comment character
    /// </summary>
    /// <param name="value">the character to check</param>
    /// <param name="reader">a reader that allows you to read the next characters if needed</param>
    /// <returns>true if the <paramref name="value"/> is the start of a multiline comment character else false</returns>
    protected virtual bool IsMultiLineCommentStart(TData value, ref CharacterReader reader)
    {
        if (!TokenizerHelpers.IsCharacter(value, '/'))
            return false;

        TData character = reader.Read(out _);
        return TokenizerHelpers.IsCharacter(character, '*');
    }

    /// <summary>
    /// checks if the value is the end of a multiline comment character
    /// </summary>
    /// <param name="value">the character to check</param>
    /// <param name="reader">a reader that allows you to read the next characters if needed</param>
    /// <returns>true if the <paramref name="value"/> is the end of a multiline comment character else false</returns>
    protected virtual bool IsMultiLineCommentEnd(TData value, ref CharacterReader reader)
    {
        if (!TokenizerHelpers.IsCharacter(value, '*'))
            return false;

        TData character = reader.Read(out _);
        return TokenizerHelpers.IsCharacter(character, '/');
    }

    /// <summary>
    /// checks if the value is the start of a string
    /// </summary>
    /// <param name="value">the character to check</param>
    /// <returns>true if the <paramref name="value"/> is the start of a string else false</returns>
    protected virtual bool IsStringStart(TData value) =>
        TokenizerHelpers.IsCharacter(value, '"');

    /// <summary>
    /// checks if the value is the end of a string
    /// </summary>
    /// <param name="value">the character to check</param>
    /// <returns>true if the <paramref name="value"/> is the end of a string else false</returns>
    protected virtual bool IsStringEnd(TData value) =>
        TokenizerHelpers.IsCharacter(value, '"');

    /// <summary>
    /// checks if the value is the start of a character string
    /// </summary>
    /// <param name="value">the character to check</param>
    /// <returns>true if the <paramref name="value"/> is the start of a character string else false</returns>
    protected virtual bool IsCharacterStringStart(TData value) =>
        TokenizerHelpers.IsCharacter(value, '\'');

    /// <summary>
    /// checks if the value is the end of a character string
    /// </summary>
    /// <param name="value">the character to check</param>
    /// <returns>true if the <paramref name="value"/> is the end of a character string else false</returns>
    protected virtual bool IsCharacterStringEnd(TData value) =>
        TokenizerHelpers.IsCharacter(value, '\'');

    /// <summary>
    /// checks if the value is the start of a multiline string
    /// </summary>
    /// <param name="value">the character to check</param>
    /// <param name="reader">a reader that allows you to read the next characters if needed</param>
    /// <returns>true if the <paramref name="value"/> is the start of a multiline string else false</returns>
    protected virtual bool IsMultiLineStringStart(TData value, ref CharacterReader reader)
    {
        if (!TokenizerHelpers.IsCharacter(value, '"'))
            return false;

        TData character = reader.Read(out _);
        if (!TokenizerHelpers.IsCharacter(character, '"'))
            return false;

        TData finalCharacter = reader.Read(out _);
        return TokenizerHelpers.IsCharacter(finalCharacter, '"');
    }

    /// <summary>
    /// checks if the value is the end of a multiline string
    /// </summary>
    /// <param name="value">the character to check</param>
    /// <param name="reader">a reader that allows you to read the next characters if needed</param>
    /// <returns>true if the <paramref name="value"/> is the end of a multiline string else false</returns>
    protected virtual bool IsMultiLineStringEnd(TData value, ref CharacterReader reader)
    {
        if (!TokenizerHelpers.IsCharacter(value, '"'))
            return false;

        TData character = reader.Read(out _);
        if (!TokenizerHelpers.IsCharacter(character, '"'))
            return false;

        TData finalCharacter = reader.Read(out _);
        return TokenizerHelpers.IsCharacter(finalCharacter, '"');
    }
    
    /// <summary>
    /// called when an error occures
    /// </summary>
    /// <param name="errorType">the type of error</param>
    /// <param name="value">the value on which the error occured</param>
    /// <returns>the error message</returns>
    protected virtual string OnError(ErrorType errorType, TData value) =>
        errorType switch
        {
            ErrorType.ValueNotFound => $"The character: {(char)short.CreateTruncating(value)} could not be tokenized",
            ErrorType.StringNeverClosed => "String was never closed",
            ErrorType.CharacterNeverClosed => "Character was never closed",
            ErrorType.MultiLineStringNeverClosed => "Multi line string was never closed",
            _ => "Error message not found"
        };

    /// <summary>
    /// adds an error to the <paramref name="enumerator"/> using a <paramref name="message"/>
    /// </summary>
    /// <param name="message">the message to include in the error</param>
    /// <param name="enumerator">the <see cref="ITokenEnumerator{TData}"/> to add the message to</param>
    protected void AddError(string message, ITokenEnumerator<TData> enumerator, ExceptionSeverity severity = default)
    {
        int prevLine = enumerator.PreviousLine;
        int prevColumn = enumerator.PreviousColumn;
        int line = enumerator.Line;
        int column = enumerator.Column;
        
        enumerator.AddError(new(message, enumerator.Reader, prevLine, prevColumn, line, column, ErrorLocation.Tokenizer, severity, Mode == CompilationMode.Debug));
    }
    
    /// <summary>
    /// when a value is not found this function will be called
    /// </summary>
    /// <param name="value">the value that was not found</param>
    /// <param name="reader">a reader used to read the next characters from the tokenizer</param>
    /// <param name="enumerator">the token enumerator</param>
    /// <param name="token">the token that is found, if none are found then default(Token)</param>
    /// <returns>true if a new token is found and it should be tokenized else false</returns>
    protected virtual bool OnValueNotFound(TData value, ref CharacterReader reader, ITokenEnumerator<TData> enumerator, out Token<TData> token)
    {
        token = default;
        
        string message = OnError(ErrorType.ValueNotFound, value);
        AddError(message, enumerator, ExceptionSeverity.Error);
    
        return false;
    }

    /// <summary>
    /// when a keyword is found this function will be called
    /// </summary>
    /// <param name="keyword">the keyword that was found</param>
    /// <param name="type">the type of the keyword that was found</param>
    /// <returns>the keyword <see cref="Token{TData}"/> that is found</returns>
    protected virtual Token<TData> OnKeywordFound(ReadOnlyMemory<TData> keyword, int type) =>
        new(keyword, type);

    /// <summary>
    /// when a token is found this function will be called
    /// </summary>
    /// <param name="token">the token that has been found</param>
    /// <param name="reader">a reader used to read the next characters from the tokenizer</param>
    /// <returns>true if the token should be added to the tokenizer else false</returns>
    protected virtual bool OnTokenFound(ref Token<TData> token, ref CharacterReader reader) =>
        true;

    /// <summary>
    /// when a newline is found this function will be called
    /// </summary>
    protected virtual void OnNewLineFound() { }

    public virtual ITokenEnumerator<TData> GetEnumerator() =>
        new TokenEnumerator(Reader, this);

    IEnumerator<Token<TData>> IEnumerable<Token<TData>>.GetEnumerator() =>
        GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    protected struct CharacterReader
    {
        private readonly TokenEnumerator enumerator;
        internal int linesAdded;
        internal long lastNewLineIndex;

        internal CharacterReader(TokenEnumerator enumerator, long lastNewLineIndex)
        {
            this.enumerator = enumerator; 
            this.lastNewLineIndex = lastNewLineIndex;
        }

        /// <inheritdoc cref="Reader{TData}.Read(out bool)"/>
        public TData Read(out bool eof)
        {
            TData character = enumerator.Read(out eof);
            
            if (TokenizerHelpers.IsNewLine(character))
            {
                linesAdded++;
                lastNewLineIndex = enumerator.Position;
            }
            return character;
        }
    }

    [DebuggerDisplay("Token = {Current, nq}")]
    protected sealed class TokenEnumerator : ITokenEnumerator<TData>
    {
        public Reader<TData> Reader => reader;
        
        private int line;
        public int Line => line;

        private long lastNewLineIndex;
        public int Column => (int)(Position - lastNewLineIndex);

        private int previousLine;
        public int PreviousLine => previousLine;

        private int previousColumn;
        public int PreviousColumn => previousColumn;

        private Token<TData> current;
        public Token<TData> Current => current;

        object IEnumerator.Current => current;

        private LinkedList<CompilerError<TData>> errors = new();
        public IReadOnlyCollection<CompilerError<TData>> Errors => errors;

        private int scopeChange;
        public int ScopeChange => scopeChange;

        private int scope;
        public int Scope => scope;

        private byte dataByte;
        public bool InComment => dataByte != 0;
        private bool InSingleLineComment
        {
            get => dataByte.GetBit(0) != 0;
            set => dataByte = dataByte.SetBit(value, 0);
        }
        private bool InMultiLineComment
        {
            get => dataByte.GetBit(1) != 0;
            set => dataByte = dataByte.SetBit(value, 1);
        }

        private Reader<TData> reader;

        private readonly Tokenizer<TData> tokenizer;

        private readonly FastQueue<Token<TData>> tokenBackLog = new(16);

        private ConsumableData consumableData;
        
        private readonly CharacterReaderFunction isMultiLineStringStart;
        private readonly CharacterReaderFunction isMultiLineStringEnd;

        private readonly CharacterReaderFunction isComment;

        private readonly CharacterReaderFunction isMultiLineCommentStart;
        private readonly CharacterReaderFunction isMultiLineCommentEnd;

        private readonly Predicate<TData> isIdentifierCharacter;
        private readonly Predicate<TData> isDigitCharacter;

        private readonly TData[] buffer;
        private int bufferLength;
        private int bufferPos;

        // internal readonly ArenaAllocator<TData> allocator;

        public TokenEnumerator(Reader<TData> reader, Tokenizer<TData> tokenizer)
        {
            this.reader = reader;
            this.tokenizer = tokenizer;
            isMultiLineStringStart = tokenizer.IsMultiLineStringStart;
            isMultiLineStringEnd = tokenizer.IsMultiLineStringEnd;

            isComment = tokenizer.IsComment;

            isMultiLineCommentStart = tokenizer.IsMultiLineCommentStart;
            isMultiLineCommentEnd = tokenizer.IsMultiLineCommentEnd;

            isIdentifierCharacter = tokenizer.IsIdentifierCharacter;
            isDigitCharacter = tokenizer.IsDigitCharacter;

            buffer = new TData[2048];
            bufferLength = reader.Read(buffer);

            // allocator = new(8192, 8192);
        }

        public void Dispose()
        {
            errors = null!;
            reader.Dispose();
            reader = null!;
            current = default;
        }

        internal TData Read(out bool eof)
        {
            eof = false;
            if (bufferPos >= bufferLength)
            {
                bufferLength = reader.Read(buffer);
                eof = bufferLength == 0;
                bufferPos = 0;
            }
            
            return buffer[bufferPos++];
        }

        internal long Position
        {
            get => reader.Position - bufferLength + bufferPos;
            set
            {
                Debug.Assert(value >= 0);
                long position = Position;

                long difference = value - position;
                int newBufferPos = bufferPos + (int)difference;
                if ((uint)newBufferPos < (uint)bufferLength)
                {
                    bufferPos = newBufferPos;
                }
                else
                {
                    reader.Position = value;
                    bufferLength = reader.Read(buffer);
                    bufferPos = 0;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 1024)]
        private readonly struct Size1024;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlyMemory<TData> GetWhileTrueMultiLine(TData initialValue, Predicate<TData> predicate, out int line, out int column)
        {
            line = 0;
            long lastNewLineIndex = 0;
        
            Unsafe.SkipInit(out Size1024 size1024);
            Span<TData> initialSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<Size1024, TData>(ref size1024),
                Unsafe.SizeOf<Size1024>() / Unsafe.SizeOf<TData>());
            initialSpan[0] = initialValue;
            ValueListBuilder<TData> builder = new(initialSpan);
            builder.Length++;

            TData newLine = TData.CreateTruncating('\n');
            
            TData[] buffer = this.buffer;

            do
            {
                int startBufferPos = bufferPos;
                for (; bufferPos < bufferLength; bufferPos++)
                {
                    TData value = buffer[bufferPos];
                    
                    if (value == newLine)
                    {
                        line++;
                        lastNewLineIndex = Position;
                    }

                    if (!predicate(value))
                    {
                        builder.Append(buffer.AsSpan(startBufferPos, bufferPos - startBufferPos));
                        goto End;
                    }
                }
                
                builder.Append(buffer.AsSpan(startBufferPos, bufferLength - startBufferPos));

                bufferLength = reader.Read(buffer);
                bufferPos = 0;
            } while (bufferLength != 0);
            
            End:

            column = (int)(Position - lastNewLineIndex);
        
            ReadOnlySpan<TData> span = builder.AsSpan();
            builder.Dispose();
            return span.ToArray();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlyMemory<TData> GetWhileTrue(TData initialValue, Predicate<TData> predicate)
        {
            Unsafe.SkipInit(out Size1024 size1024);
            Span<TData> initialSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<Size1024, TData>(ref size1024),
                Unsafe.SizeOf<Size1024>() / Unsafe.SizeOf<TData>());
            initialSpan[0] = initialValue;
            ValueListBuilder<TData> builder = new(initialSpan);
            builder.Length++;
            
            TData[] buffer = this.buffer;

            do
            {
                int startBufferPos = bufferPos;
                for (; bufferPos < bufferLength; bufferPos++)
                {
                    TData value = buffer[bufferPos];

                    if (!predicate(value))
                    {
                        builder.Append(buffer.AsSpan(startBufferPos, bufferPos - startBufferPos));
                        goto End;
                    }
                }

                builder.Append(buffer.AsSpan(startBufferPos, bufferLength - startBufferPos));

                bufferLength = reader.Read(buffer);
                bufferPos = 0;
            } while (bufferLength != 0);
        
            End:
            
            ReadOnlySpan<TData> span = builder.AsSpan();
            builder.Dispose();
            return span.ToArray();
        }

        private ReadOnlyMemory<TData> GetWhileTrue(TData value, bool multiLine, Predicate<TData> predicate, ref int line, ref long lastNewLineIndex)
        {
            if (!multiLine)
            {
                ReadOnlyMemory<TData> val = GetWhileTrue(value, predicate);
                return val;
            }
            else
            {
                ReadOnlyMemory<TData> val = GetWhileTrueMultiLine(value, predicate, out int addedLines, out int column);
                lastNewLineIndex = Position - column;
                line += addedLines;
                return val;
            }
        }

        private delegate bool StringFunc(TData value);

        private static TData GetEscapedCharacter(TData value)
        {
            byte byteValue = Unsafe.As<TData, byte>(ref value);
            return byteValue switch
            {
                (byte)'n' => TData.CreateTruncating('\n'),
                (byte)'t' => TData.CreateTruncating('\t'),
                (byte)'r' => TData.CreateTruncating('\r'),
                (byte)'b' => TData.CreateTruncating('\b'),
                (byte)'f' => TData.CreateTruncating('\f'),
                _ => value,
            };
        }
        
        private ReadOnlyMemory<TData> GetString(ErrorType error, StringFunc func)
        {
            Unsafe.SkipInit(out Size1024 size1024);
            Span<TData> initialSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<Size1024, TData>(ref size1024),
                Unsafe.SizeOf<Size1024>() / Unsafe.SizeOf<TData>());
            ValueListBuilder<TData> builder = new(initialSpan);
            
            TData[] buffer = this.buffer;

            TData newLine = TData.CreateTruncating('\n');
            TData backSlash = TData.CreateTruncating('\\');
            
            while (true)
            {
                int startBufferPos = bufferPos;
                for (; bufferPos < bufferLength; bufferPos++)
                {
                    TData value = buffer[bufferPos];
                    
                    if (value == newLine)
                    {
                        AddError(error, value);
                        break;
                    }
                    if (value == backSlash)
                    {
                        TData nextValue;
                        if (bufferPos + 1 >= bufferLength)
                        {
                            builder.Append(buffer.AsSpan(startBufferPos, bufferLength - startBufferPos));
                            bufferLength = reader.Read(buffer);

                            if (bufferLength == 0) break;

                            nextValue = buffer[0];
                            bufferPos = 0;
                        }
                        else
                        {
                            builder.Append(buffer.AsSpan(startBufferPos, bufferPos - startBufferPos));
                            nextValue = buffer[bufferPos + 1];
                        }

                        startBufferPos = bufferPos + 2;
                        bufferPos++;
                        TData escapedCharacter = GetEscapedCharacter(nextValue);
                        builder.Append(escapedCharacter);
                        continue;
                    }
                    
                    if (func(value))
                    {
                        builder.Append(buffer.AsSpan(startBufferPos, bufferPos - startBufferPos));
                        goto End;
                    }
                }
                
                builder.Append(buffer.AsSpan(startBufferPos, bufferLength - startBufferPos));

                bufferLength = reader.Read(buffer);

                if (bufferLength == 0)
                {
                    AddError(error, TData.Zero);
                    break;
                }

                bufferPos = 0;
            }
            
            End:

            bufferPos++;

            ReadOnlySpan<TData> span = builder.AsSpan();
            builder.Dispose();
            return span.ToArray();
        }

        private ReadOnlyMemory<TData> GetStringMultiline(ref int line, ref long lastNewLineIndex)
        {
            Unsafe.SkipInit(out Size1024 size1024);
            Span<TData> initialSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<Size1024, TData>(ref size1024),
                Unsafe.SizeOf<Size1024>() / Unsafe.SizeOf<TData>());
            ValueListBuilder<TData> builder = new(initialSpan);

            CharacterReader charReader = new(this, lastNewLineIndex);
            TData[] buffer = this.buffer;
            
            TData newLine = TData.CreateTruncating('\n');
            TData backSlash = TData.CreateTruncating('\\');
            while (true)
            {
                int startBufferPos = bufferPos;
                for (; bufferPos < bufferLength; bufferPos++)
                {
                    TData value = buffer[bufferPos];
                    
                    if (value == newLine)
                    {
                        line++;
                        lastNewLineIndex = Position;
                    }
                    else if (value == backSlash)
                    {
                        TData nextValue;
                        if (bufferPos + 1 >= bufferLength)
                        {
                            builder.Append(buffer.AsSpan(startBufferPos, bufferLength - startBufferPos));
                            bufferLength = reader.Read(buffer);

                            if (bufferLength == 0) break;

                            nextValue = buffer[0];
                            bufferPos = 0;
                        }
                        else
                        {
                            builder.Append(buffer.AsSpan(startBufferPos, bufferPos - startBufferPos));
                            nextValue = buffer[bufferPos + 1];
                        }

                        startBufferPos = bufferPos + 2;
                        bufferPos++;
                        TData escapedCharacter = GetEscapedCharacter(nextValue);
                        builder.Append(escapedCharacter);
                        continue;
                    }

                    int startPos = bufferPos;
                    charReader.linesAdded = 0;
                    if (isMultiLineStringEnd(value, ref charReader))
                    {
                        line += charReader.linesAdded;
                        lastNewLineIndex = charReader.lastNewLineIndex;
                        builder.Append(buffer.AsSpan(startBufferPos, bufferPos - startBufferPos));
                        goto End;
                    }

                    bufferPos = startPos;
                }
                
                builder.Append(buffer.AsSpan(startBufferPos, bufferLength - startBufferPos));

                bufferLength = reader.Read(buffer);

                if (bufferLength == 0)
                {
                    AddError(ErrorType.MultiLineStringNeverClosed, TData.Zero);
                    break;
                }

                bufferPos = 0;
            }
            
            End:

            bufferPos++;

            ReadOnlySpan<TData> span = builder.AsSpan();
            builder.Dispose();
            return span.ToArray();
        }

        private delegate bool CharacterReaderFunction(TData value, ref CharacterReader reader);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool RunWithCharacterReader(TData value, CharacterReaderFunction function, ref int line, ref long lastNewLineIndex)
        {
            long startPosition = Position;
            CharacterReader characterReader = new(this, lastNewLineIndex);
            bool b = function(value, ref characterReader);

            if (!b)
            {
                Position = startPosition;
                return false;
            }
            line += characterReader.linesAdded;
            lastNewLineIndex = characterReader.lastNewLineIndex;
            return true;
        }

        private void SkipOverMultilineComment(ref int line, ref long lastNewLineIndex)
        {
            while (true)
            {
                TData val = Read(out bool eof);
                if (TokenizerHelpers.IsNewLine(val))
                {
                    line++;
                    lastNewLineIndex = Position;
                }
                if (eof | RunWithCharacterReader(val, isMultiLineCommentEnd, ref line, ref lastNewLineIndex))
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsCharacter(TData value, out int type)
        {
            if (value > TData.CreateSaturating(char.MaxValue))
            {
                type = default;
                return false;
            }

            char val = (char)ushort.CreateTruncating(value); // value should fit inside of a char here
            Debug.Assert(TData.CreateTruncating(val) == value);

            type = val switch
            {
                '=' => TokenType.Equals,
                '+' => TokenType.Plus,
                '-' => TokenType.Minus,
                '/' => TokenType.ForwardSlash,
                '*' => TokenType.Star,
                '%' => TokenType.Percentage,
                '&' => TokenType.Ampersand,
                '|' => TokenType.VerticalBar,
                '^' => TokenType.Caret,
                '~' => TokenType.Tilde,
                '(' => TokenType.LeftParenthesis,
                ')' => TokenType.RightParenthesis,
                ',' => TokenType.Comma,
                '!' => TokenType.ExclamationMark,
                '{' => TokenType.LeftBrace,
                '}' => TokenType.RightBrace,
                '[' => TokenType.LeftBracket,
                ']' => TokenType.RightBracket,
                ';' => TokenType.SemiColon,
                '.' => TokenType.Dot,
                '<' => TokenType.LessThan,
                '>' => TokenType.GreaterThan,
                '"' => TokenType.DoubleQuotes,
                '\'' => TokenType.SingleQuote,
                '#' => TokenType.Hashtag,
                '?' => TokenType.QuestionMark,
                '$' => TokenType.DolarSign,
                ':' => TokenType.Colon,
                '@' => TokenType.At,
                _ => TokenType.None
            };

            return type != TokenType.None;
        }

        private void AddError(ErrorType error, TData value)
        {
            string message = tokenizer.OnError(error, value);
            AddError(
                new CompilerError<TData>(message, reader, PreviousLine, PreviousColumn, Line, Column, ErrorLocation.Tokenizer, ExceptionSeverity.Error,  tokenizer.Mode == CompilationMode.Debug)
                );

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool GetNext(out int line, out long lastNewLineIndex, out bool dequeued, out int tokensAdded, out Token<TData> current, bool isPeeking)
        {
            tokenBackLog.RemoveLast(consumableData.TokensAdded);
            
            line = this.line;
            lastNewLineIndex = this.lastNewLineIndex;
            dequeued = false;
            current = default;
            tokensAdded = 0;

            int startingBacklogCount = tokenBackLog.Count;

            do
            {
                if (!isPeeking)
                {
                    if (tokenBackLog.TryDequeue(out Token<TData> queuedToken))
                    {
                        current = queuedToken;

                        //tokenizer.OnTokenFound(queuedToken, ref index, ref line, ref lastNewLineIndex, this);

                        return true;
                    }
                }
                else
                {
                    if (tokenBackLog.TryPeek(out Token<TData> queuedToken))
                    {
                        current = queuedToken;

                        //tokenizer.OnTokenFound(queuedToken, ref index, ref line, ref lastNewLineIndex, this);
                        dequeued = true;

                        return true;
                    }
                }

                TData value = Read(out bool eof);
                if (eof)
                {
                    return false;
                }

                if (TokenizerHelpers.IsNewLine(value))
                {
                    lastNewLineIndex = Position;
                    line++;

                    tokenizer.OnNewLineFound();

                    if (tokenizer.TokenizeComments & InSingleLineComment)
                    {
                        current = new(TokenType.CommentEnd);
                        InSingleLineComment = false;
                        if (tokenizer.TokenizeNewLines)
                            AddToken(new(TData.CreateTruncating('\n'), TokenType.NewLine));
                        break;
                    }

                    if (tokenizer.TokenizeNewLines)
                    {
                        current = new(TData.CreateTruncating('\n'), TokenType.NewLine);
                        break;
                    }
                    
                    continue;
                }
                if (tokenizer.IsIdentifierStartCharacter(value, out bool multiLine))
                {
                    ReadOnlyMemory<TData> array = GetWhileTrue(value, multiLine, isIdentifierCharacter, ref line, ref lastNewLineIndex);
                    Token<TData> token;
                    if (tokenizer.IsKeyword(array, out int type))
                        token = tokenizer.OnKeywordFound(array, type);
                    else
                        token = new(array, TokenType.Identifier);

                    current = token;
                }
                else if (tokenizer.IsDigitStart(value, out multiLine))
                {
                    ReadOnlyMemory<TData> memory = GetWhileTrue(value, multiLine, isDigitCharacter, ref line, ref lastNewLineIndex);

                    if (tokenizer.IsNumber(memory.Span, out int type))
                        current = new(memory, type);
                    else
                        continue;
                }
                else if (RunWithCharacterReader(value, isComment, ref line, ref lastNewLineIndex))
                {
                    if (tokenizer.TokenizeComments)
                    {
                        InSingleLineComment = true;
                        current = new(TokenType.CommentStart);
                        break;
                    }
                    
                    while (!TokenizerHelpers.IsNewLine(Read(out _)));
                    Position--;
                    continue;
                }
                else if (RunWithCharacterReader(value, isMultiLineCommentStart, ref line, ref lastNewLineIndex))
                {
                    if (tokenizer.TokenizeComments)
                    {
                        InMultiLineComment = true;
                        current = new(TokenType.CommentStart);
                        break;
                    }

                    SkipOverMultilineComment(ref line, ref lastNewLineIndex);
                    continue;
                }
                else if (InMultiLineComment && RunWithCharacterReader(value, isMultiLineCommentEnd, ref line, ref lastNewLineIndex))
                {
                    InMultiLineComment = false;
                    current = new(TokenType.CommentEnd);
                    break;
                }
                else if (tokenizer.IsStringStart(value))
                {
                    ReadOnlyMemory<TData> @string = GetString(ErrorType.StringNeverClosed, tokenizer.IsStringEnd);
                    current = new(@string, TokenType.String);
                }
                else if (tokenizer.IsCharacterStringStart(value))
                {
                    ReadOnlyMemory<TData> character = GetString(ErrorType.CharacterNeverClosed, tokenizer.IsCharacterStringEnd);
                    current = new(character, TokenType.Character);
                }
                else if (RunWithCharacterReader(value, isMultiLineStringStart, ref line, ref lastNewLineIndex))
                {
                    ReadOnlyMemory<TData> multiLineString = GetStringMultiline(ref line, ref lastNewLineIndex);
                    current = new(multiLineString, TokenType.MultiLineString);
                }
                else if (tokenizer.IsScopeIn(value, out scopeChange))
                {
                    scope += scopeChange;
                    current = new(TokenType.ScopeIn);
                }
                else if (tokenizer.IsScopeOut(value, out scopeChange))
                {
                    scope += scopeChange;
                    current = new(TokenType.ScopeOut);
                }
                else if (IsCharacter(value, out int type))
                {
                    current = new(value, type);
                }
                else if (TokenizerHelpers.IsWhitespace(value))
                {
                    if (tokenizer.TokenizeWhiteSpace)
                        current = new(value, TokenType.WhiteSpace);
                    else
                        continue;
                }
                else
                {
                    CharacterReader notFoundReader = new(this, lastNewLineIndex);
                    bool tokenFound = tokenizer.OnValueNotFound(value, ref notFoundReader, this, out current);
                    if (!tokenFound)
                        continue;
                    lastNewLineIndex = notFoundReader.lastNewLineIndex;
                    line += notFoundReader.linesAdded;
                }

                CharacterReader characterReader = new(this, lastNewLineIndex);
                if (!tokenizer.OnTokenFound(ref current, ref characterReader))
                    continue;
                lastNewLineIndex = characterReader.lastNewLineIndex;
                line += characterReader.linesAdded;
                
                if (current.Type == TokenType.None && !tokenizer.TokenizeNoneTokens)
                    continue;
                break;
            } while (true);

            tokensAdded = tokenBackLog.Count - startingBacklogCount;

            return true;
        }

        public bool MoveNext()
        {
            previousLine = line;
            previousColumn = Column;
            return GetNext(out line, out lastNewLineIndex, out _, out _, out current, false);
        }

        public void Reset()
        {
            Position = 0;
            errors = new();
        }

        public bool PeekNext(out Consumable<TData> consumable)
        {
            long startingPosition = Position;
            bool gotNext = GetNext(out int line, out long lastNewLineIndex, out bool dequeued, out int tokensAdded, out Token<TData> current, true);
            long endPosition = Position;
            Position = startingPosition;
            
            consumableData = new(endPosition, line, lastNewLineIndex, dequeued, tokensAdded);
            consumable = new(this, current, line, (int)(endPosition - lastNewLineIndex));
            return gotNext;
        }

        public void Consume(Consumable<TData> consumable)
        {
            previousLine = line;
            previousColumn = Column;
            (Position, line, lastNewLineIndex, bool dequeued) = consumableData;

            if (dequeued)
            {
                Debug.Assert(tokenBackLog.Count > 0);
                tokenBackLog.Dequeue();
            }
            consumableData = default;
            current = consumable.Token;
        }

        public void AddError(CompilerError<TData> error) =>
            errors.AddLast(error);


        public void AddToken(Token<TData> token) =>
            tokenBackLog.Enqueue(token);

        public void AddTokens(IReadOnlyCollection<Token<TData>> tokens) =>
            tokenBackLog.Enqueue(tokens);

        private readonly struct ConsumableData
        {
            public readonly long Position;
            public readonly int Line;
            public readonly long LastNewLineIndex;
            public readonly bool Dequeued;
            public readonly int TokensAdded;

            public ConsumableData(long position, int line, long lastNewLineIndex, bool dequeued, int tokensAdded)
            {
                Position = position;
                Line = line;
                LastNewLineIndex = lastNewLineIndex;
                Dequeued = dequeued;
                TokensAdded = tokensAdded;
            }

            public void Deconstruct(out long position, out int line, out long lastNewLineIndex, out bool dequeued)
            {
                position = Position;
                line = Line;
                lastNewLineIndex = LastNewLineIndex;
                dequeued = Dequeued;
            }
        }
    }
}