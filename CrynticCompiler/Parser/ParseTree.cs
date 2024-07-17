using System.Collections;
using System.Linq.Expressions;
using System.Net;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CrynticCompiler.Tokenizer;
using CrynticCompiler.Collections;
using CrynticCompiler.Parser.Nodes;
using CrynticCompiler.Tokenizer.IO;

namespace CrynticCompiler.Parser;

public abstract class ParseTree<TData> : IParseTree<TData>
	where TData : unmanaged, IBinaryInteger<TData>
{
    public ITokenizer<TData> Tokenizer { get; }
    private readonly ITokenEnumerator<TData> enumerator;
    
    protected readonly LinkedList<CompilerError<TData>> errors;
    public IReadOnlyCollection<CompilerError<TData>> Errors
    {
        get
        {
            if (enumerator.Errors.Count == 0)
            {
                return errors;
            }

            if (errors.Count == 0)
            {
                return enumerator.Errors;
            }

            return new CollectionLinker<CompilerError<TData>>
            {
                (IReadOnlyCollection<CompilerError<TData>>)errors,
                enumerator.Errors
            };
        }
    }

    private readonly Node[] nodes;

    private readonly int count;
    public int Count => count;

    /// <summary>
    /// the current mode the compiler is in. Is used to show part of the stacktrace when an error occurs whilst in <see cref="CompilationMode.Debug"/>
    /// </summary>
    public abstract CompilationMode Mode { get; }

    protected virtual bool IncludeNullNodes => false;

    private ISemanticModel<TData>? model;

    public ParseTree(ITokenizer<TData> tokenizer)
    {
        Tokenizer = tokenizer;
        enumerator = tokenizer.GetEnumerator();
        errors = new();

        nodes = GetNodes(out count);
    }

    ~ParseTree() =>
        Dispose();

    public Node this[int index] => nodes[index];

    protected abstract INode? GetNode(Token<TData> token, ITokenEnumerator<TData> enumerator);

    public virtual ISemanticModel<TData> SemanticModel =>
        model ??= CreateSemanticModel();

    protected abstract ISemanticModel<TData> CreateSemanticModel();

    private Node GetParsedNode(ITokenEnumerator<TData> tokenEnumerator)
    {
        int startColumn = tokenEnumerator.Column;
        int startLine = tokenEnumerator.Line;
            
        Token<TData> token = tokenEnumerator.Current;

        INode? node = GetNode(token, tokenEnumerator);

        int endColumn = tokenEnumerator.Column;
        int endLine = tokenEnumerator.Line;

        return new(startLine, endLine, startColumn, endColumn, node);
    }

    private Node[] GetNodes(out int count)
    {
        Node[] nodes = new Node[16];

        int nodeIndex = 0;
        ITokenEnumerator<TData> tokenEnumerator = enumerator;
        while (tokenEnumerator.MoveNext())
        {
            Node parsedNode = GetParsedNode(tokenEnumerator);
            if (parsedNode.Value is null & !IncludeNullNodes)
            {
                continue;
            }

            if (nodeIndex >= nodes.Length)
            {
                int newSize = nodeIndex * 2;
                Node[] newArray = new Node[newSize];
                nodes.CopyTo(newArray, 0);
                nodes = newArray;
            }

            nodes[nodeIndex] = parsedNode;
            nodeIndex++;
        }

        count = nodeIndex;
        return nodes;
    }

    protected virtual bool IsNumber(Token<TData> token) =>
        token.IsTypes(TokenType.Number, TokenType.Hexadecimal, TokenType.Binary, TokenType.Octal, TokenType.Float);

    protected virtual bool IsString(Token<TData> token) => 
        token.IsTypes(TokenType.String, TokenType.MultiLineString);
    
    #region Errors
    
    protected void AddError(string message, ExceptionSeverity severity = default) =>
        AddError(message, enumerator.PreviousLine, enumerator.PreviousColumn, enumerator.Line, enumerator.Column, severity);

    protected void AddError(string message, Consumable<TData> consumable, ExceptionSeverity severity = default) =>
        AddError(message, consumable.PreviousLine, consumable.PreviousColumn, consumable.Line, consumable.Column, severity);

    protected void AddError(string message, int previousLine, int previousColumn, int line, int column,
        ExceptionSeverity severity = default)
    {
        // Console.WriteLine(((Tokenizer<TData>.TokenEnumerator)enumerator).Position);
        errors.AddLast(
            new CompilerError<TData>(message, enumerator.Reader, previousLine, previousColumn, line, column, ErrorLocation.Parser, severity, Mode == CompilationMode.Debug)
        );
    }

    protected void AddError(int errorType, ExceptionSeverity severity = default) =>
        AddError(errorType, enumerator.PreviousLine, enumerator.PreviousColumn, enumerator.Line, enumerator.Column,
            severity);
    
    protected void AddError(int errorType, Consumable<TData> consumable, ExceptionSeverity severity = default) =>
        AddError(errorType, consumable.PreviousLine, consumable.PreviousColumn, consumable.Line, consumable.Column,
            severity);

    protected void AddError(int errorType, int previousLine, int previousColumn, int line, int column,
        ExceptionSeverity severity = default)
    {
        string message = GetErrorMessage<IPAddress>(errorType, null); // using ipadress because it's the only reference type I could find with ISpanFormattable
        AddError(message, previousLine, previousColumn, line, column, severity);
    }
    
    protected void AddError<T, T2>(int errorType, T? got, T2? expected, ExceptionSeverity severity = default)
        where T : ISpanFormattable
        where T2 : ISpanFormattable =>
        AddError(errorType, got, expected, enumerator.PreviousLine, enumerator.PreviousColumn, enumerator.Line, enumerator.Column,
            severity);
    
    protected void AddError<T, T2>(int errorType, T? got, T2? expected, Consumable<TData> consumable, ExceptionSeverity severity = default) 
        where T : ISpanFormattable
        where T2 : ISpanFormattable =>
        AddError(errorType, got, expected, consumable.PreviousLine, consumable.PreviousColumn, consumable.Line, consumable.Column,
            severity);

    protected void AddError<T, T2>(int errorType, T? got, T2? expected, int previousLine, int previousColumn, int line, int column,
        ExceptionSeverity severity = default)
        where T : ISpanFormattable
        where T2 : ISpanFormattable
    {
        string message = GetErrorMessage(errorType, expected);
        if (got is not null && got is not EmptyFormatter)
        {
            message = CreateErrorMessage(message + " Got: ", got);
        }
        AddError(message, previousLine, previousColumn, line, column, severity);
    }
    
    protected virtual string ExpectedIdentifierMessage => "Expected an identifier.";

    protected virtual string ExpectedValueMessage => "Expected: ";

    protected virtual string ExpectedStringMessage => "Expected a string.";

    protected virtual string ExpectedExpressionMessage => "Expected an expression.";

    protected virtual string ExpectedExpressionAfterMessage => ExpectedExpressionMessage + " After: ";
    
    protected virtual string GetErrorMessage<T>(int errorType, T? expected)
        where T : ISpanFormattable
    {
        if (errorType == ErrorType.ExpectedIdentifier)
        {
            return ExpectedIdentifierMessage;
        }

        if (errorType == ErrorType.ExpectedValue)
        {
            return CreateErrorMessage(ExpectedValueMessage, expected, ".");
        }

        if (errorType == ErrorType.ExpectedString)
        {
            return ExpectedStringMessage;
        }

        if (errorType == ErrorType.ExpectedExpression)
        {
            return ExpectedExpressionMessage;
        }

        if (errorType == ErrorType.ExpectedExpressionAfter)
        {
            return CreateErrorMessage(ExpectedExpressionAfterMessage, expected, ".");
        }
        return "An error occured";
    }

    protected string CreateErrorMessage<T>(string message, T? value, ReadOnlySpan<char> postFix = default)
        where T : ISpanFormattable
    {
        Span<char> span = stackalloc char[512];
        
        int valueEndIndex = message.Length;
        if (value is not null)
        {
            Span<char> valueSlice = span[valueEndIndex..];
            if (!value.TryFormat(valueSlice, out int charsWritten, default, null))
            {
                return message + value + postFix.ToString();
            }

            valueEndIndex += charsWritten;
        }

        if (valueEndIndex + postFix.Length >= span.Length) // very rare but can happen
        {
            string str = new('\0', valueEndIndex + postFix.Length);
            ref char charRef = ref Unsafe.AsRef(in str.GetPinnableReference());
            Span<char> strSpan = MemoryMarshal.CreateSpan(ref charRef, str.Length);
            
            message.AsSpan().CopyTo(strSpan);
            span[message.Length..valueEndIndex].CopyTo(strSpan[message.Length..]);
            postFix.CopyTo(strSpan[valueEndIndex..]);

            return str;
        }
        
        message.AsSpan().CopyTo(span);
        postFix.CopyTo(span[valueEndIndex..]);
        valueEndIndex += postFix.Length;

        return span[..valueEndIndex].ToString();
    }
    
    #endregion

    public IEnumerator<Node> GetEnumerator()
    {
        for (int i = 0; i < count; i++)
        {
            yield return nodes[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        enumerator.Dispose();
    }
}

