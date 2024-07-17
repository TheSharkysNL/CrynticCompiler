using System.Diagnostics;
using System.Numerics;
using CrynticCompiler.Collections;
using CrynticCompiler.Json.Parser.Nodes;
using CrynticCompiler.Parser;
using CrynticCompiler.Parser.Nodes;
using CrynticCompiler.Parser.Symbols;
using CrynticCompiler.Tokenizer;
using ErrorType = CrynticCompiler.Parser.ErrorType;

namespace CrynticCompiler.Json.Parser;

public abstract class JsonParseTree<TData>(ITokenizer<TData> tokenizer) : ParseTree<TData>(tokenizer)
    where TData : unmanaged, IBinaryInteger<TData>
{
    #region Errors

    protected virtual string TrailingSeparatorNotAllowedMessage => "Trailing separators are not allowed.";
    
    protected virtual string ListNeverClosedMessage => "Json list was never closed";
    
    protected virtual string ObjectNeverClosedMessage => "Json object was never closed";
    
    protected override string GetErrorMessage<T>(int errorType, T? expected) where T : default
    {
        if (errorType == JsonErrorType.TrailingSeparatorNotAllowed)
        {
            return TrailingSeparatorNotAllowedMessage;
        }

        if (errorType == JsonErrorType.ListNeverClosed)
        {
            return ListNeverClosedMessage;
        }

        if (errorType == JsonErrorType.ObjectNeverClosed)
        {
            return ObjectNeverClosedMessage;
        }
        return base.GetErrorMessage(errorType, expected);
    }

    #endregion

    protected virtual bool IsListStartType(Token<TData> token) =>
        token.IsType(TokenType.LeftBracket);
    
    protected virtual bool IsListEndType(Token<TData> token) =>
        token.IsType(TokenType.RightBracket);
    
    protected virtual bool IsObjectStartType(Token<TData> token) =>
        token.IsType(TokenType.LeftBrace);
    
    protected virtual bool IsObjectEndType(Token<TData> token) =>
        token.IsType(TokenType.RightBrace);

    protected virtual bool IsSeparator(Token<TData> token) => 
        token.IsType(TokenType.Comma);

    protected bool IsBooleanLiteral(Token<TData> token, out bool truthy)
    {
        if (token.Data.Span.SequenceEqual(JsonBooleanLiteral<TData>.TrueLiteral.Span))
        {
            truthy = true;
            return true;
        }

        if (token.Data.Span.SequenceEqual(JsonBooleanLiteral<TData>.FalseLiteral.Span))
        {
            truthy = false;
            return true;
        }

        truthy = default;
        return false;
    }

    protected virtual string Separator => ",";
    protected virtual string ListEndType => "]";
    protected virtual string ObjectEndType => "}";

    protected virtual bool AllowTrailingSeparator => false;
    
    protected virtual IJsonNode? GetExpression(ITokenEnumerator<TData> enumerator, JsonCollectionNode parent)
    {
        if (IsString(enumerator.Current))
        {
            return new JsonStringLiteral<TData>(enumerator.Current.Data, parent);
        }
        if (IsNumber(enumerator.Current))
        {
            return new JsonIntegerLiteral<TData>(enumerator.Current.Data, enumerator.Current.Type, parent);
        }
        if (IsBooleanLiteral(enumerator.Current, out bool truthy))
        {
            return new JsonBooleanLiteral<TData>(truthy, parent);
        }
        if (enumerator.Current.Data.Span.SequenceEqual(JsonNullLiteral<TData>.NullLiteral.Span))
        {
            return new JsonNullLiteral<TData>(parent);
        }
        
        return GetValue(enumerator.Current, enumerator, parent);
    }

    protected Token<TData> GetObjectKey(ITokenEnumerator<TData> enumerator)
    {
        if (IsString(enumerator.Current))
        {
            return enumerator.Current;
        }
        
        AddError(ErrorType.ExpectedString, ExceptionSeverity.Error);
        return default;
    }

    protected IReadOnlyList<IJsonNode> GetList(ITokenEnumerator<TData> enumerator, JsonCollectionNode parent)
    {
        Debug.Assert(IsListStartType(enumerator.Current));
        if (!enumerator.MoveNext())
        {
            AddError(JsonErrorType.ListNeverClosed, ExceptionSeverity.Error);
            return Array.Empty<IJsonNode>();
        }

        if (IsListEndType(enumerator.Current))
        {
            return Array.Empty<IJsonNode>();
        }

        int index = 0;
        IJsonNode[] nodes = new IJsonNode[4];

        do
        {
            IJsonNode? expression = GetExpression(enumerator, parent);
            if (expression is null)
            {
                break;
            }

            if (index >= nodes.Length)
            {
                IJsonNode[] temp = new IJsonNode[index * 2];
                nodes.CopyTo(temp, 0);
                nodes = temp;
            }
            nodes[index++] = expression;

            if (!enumerator.MoveNext() ||
                (!IsListEndType(enumerator.Current) &&
                !IsSeparator(enumerator.Current)))
            {
                AddError(ErrorType.ExpectedValue, enumerator.Current.Data,
                    new ArrayFormatter<StringFormatter>([ListEndType, Separator], " or ", "\'"), ExceptionSeverity.Error);
                break;
            }

            if (IsListEndType(enumerator.Current))
            {
                break;
            }
            
            Debug.Assert(IsSeparator(enumerator.Current));
            if (!AllowTrailingSeparator && 
                (!enumerator.MoveNext() ||
                IsListEndType(enumerator.Current)))
            {
                AddError(JsonErrorType.TrailingSeparatorNotAllowed, ExceptionSeverity.Error);
                break;
            }
        } while (true);

        return new ReadOnlyArray<IJsonNode>(nodes, index);
    }

    protected IReadOnlyList<KeyValuePairNode<TData>> GetKeyValuePairs(ITokenEnumerator<TData> enumerator, JsonCollectionNode parent)
    {
        Debug.Assert(IsObjectStartType(enumerator.Current));
        if (!enumerator.MoveNext())
        {
            AddError(JsonErrorType.ObjectNeverClosed, ExceptionSeverity.Error);
            return Array.Empty<KeyValuePairNode<TData>>();
        }

        if (IsObjectEndType(enumerator.Current))
        {
            return Array.Empty<KeyValuePairNode<TData>>();
        }

        int index = 0;
        KeyValuePairNode<TData>[] pairs = new KeyValuePairNode<TData>[4];

        do
        {
            Token<TData> key = GetObjectKey(enumerator);

            if (!enumerator.NextTokenIsType(TokenType.Colon))
            {
                AddError(ErrorType.ExpectedValue, enumerator.Current.Data, new SurroundFormatter<char>(':', "\'"), ExceptionSeverity.Error);
                break;
            }

            if (!enumerator.MoveNext())
            {
                AddError(ErrorType.ExpectedExpressionAfter, new EmptyFormatter(), new SurroundFormatter<char>(':', "\'"), ExceptionSeverity.Error);
                break;
            }

            KeyValuePairNode<TData> pair = new(key, null!);
            IJsonNode? value = GetExpression(enumerator, parent);

            if (value is null)
            {
                break;
            }
            
            pair.Value = value;
            
            if (index >= pairs.Length)
            {
                KeyValuePairNode<TData>[] temp = new KeyValuePairNode<TData>[index * 2];
                pairs.CopyTo(temp, 0);
                pairs = temp;
            }

            pairs[index++] = pair;

            if (!enumerator.MoveNext() ||
                (!IsObjectEndType(enumerator.Current) &&
                !IsSeparator(enumerator.Current)))
            {
                AddError(ErrorType.ExpectedValue, enumerator.Current.Data,
                    new ArrayFormatter<StringFormatter>([Separator, ObjectEndType], " or ", "\'"), ExceptionSeverity.Error);
                break;
            }

            if (IsObjectEndType(enumerator.Current))
            {
                break;
            }
            
            Debug.Assert(IsSeparator(enumerator.Current));
            if (!AllowTrailingSeparator && (!enumerator.MoveNext() ||
                IsListEndType(enumerator.Current)))
            {
                AddError(JsonErrorType.TrailingSeparatorNotAllowed, ExceptionSeverity.Error);
                break;
            }
        } while (true);
        
        return new ReadOnlyArray<KeyValuePairNode<TData>>(pairs, index);
    }

    protected IJsonNode? GetValue(Token<TData> token, ITokenEnumerator<TData> enumerator, IJsonNode? parent = null)
    {
        if (IsListStartType(token))
        {
            ListNode list = new(null, parent);
            IReadOnlyList<IJsonNode> values = GetList(enumerator, list);
            list.Values = values;
            return list;
        }
        if (IsObjectStartType(token))
        {
            KeyValueMapNode<TData> map = new(null!, parent);
            IReadOnlyList<KeyValuePairNode<TData>> pairs = GetKeyValuePairs(enumerator, map);
            map.Pairs = pairs;
            return map;
        }

        return null;
    }

    protected virtual INode? TokenNotFound(Token<TData> token, ITokenEnumerator<TData> enumerator) => null;
    
    protected override INode? GetNode(Token<TData> token, ITokenEnumerator<TData> enumerator)
    {
        if (IsListStartType(token) || IsObjectStartType(token))
        {
            return GetValue(token, enumerator);
        }

        return TokenNotFound(token, enumerator);
    }

    protected override ISemanticModel<TData> CreateSemanticModel() =>
        new JsonSemanticModel<TData>(this);
}