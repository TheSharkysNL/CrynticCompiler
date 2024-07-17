using System.Numerics;
using CrynticCompiler.Collections;
using CrynticCompiler.Parser;
using CrynticCompiler.Parser.Nodes;
using CrynticCompiler.Tags.Parser.Nodes;
using CrynticCompiler.Tokenizer;
using ErrorType = CrynticCompiler.Parser.ErrorType;

namespace CrynticCompiler.Tags.Parser;

public abstract class TagParseTree<TData>(ITokenizer<TData> tokenizer) : ParseTree<TData>(tokenizer)
    where TData : unmanaged, IBinaryInteger<TData>
{
    /// <summary>
    /// Used to specify that tags can be short
    /// eg: <![CDATA[<img src="" />]]>
    /// </summary>
    protected virtual bool UseShortTags => true;
    
    #region Errors

    protected virtual string TagNameNotFoundMessage => "Was unable to parse the tag identifier.";

    protected virtual string TagNotClosedMessage => "Tag was not closed.";

    protected virtual string ExpectedAttributeValueMessage => "Expected attribute value.";

    protected virtual string ExpectedATagMessage => "Expected a tag after: ";

    protected virtual string OpeningAndClosingTagsDontMatchMessage => "Opening and closing tags, expected: ";
    
    protected override string GetErrorMessage<T>(int errorType, T? expected) 
        where T : default
    {
        if (errorType == TagErrorType.TagNameNotFound)
        {
            return TagNameNotFoundMessage;
        }

        if (errorType == TagErrorType.TagNotClosed)
        {
            return TagNotClosedMessage;
        }

        if (errorType == TagErrorType.ExpectedAttributeValue)
        {
            return ExpectedAttributeValueMessage;
        }

        if (errorType == TagErrorType.ExpectedATag)
        {
            return CreateErrorMessage(ExpectedATagMessage, expected, ".");
        }

        if (errorType == TagErrorType.OpeningAndClosingTagsDontMatch)
        {
            return OpeningAndClosingTagsDontMatchMessage;
        }
        return base.GetErrorMessage(errorType, expected);
    }

    #endregion
    
    protected virtual bool IsTagStartType(Token<TData> token) =>
        token.IsType(TokenType.LessThan);

    protected virtual bool IsTagEndType(Token<TData> token) =>
        token.IsType(TokenType.GreaterThan);

    protected virtual bool IsClosingTagType(Token<TData> token) =>
        token.IsType(TokenType.ForwardSlash);

    protected virtual bool IsShortTagType(TokenData<TData> name) => false;

    private bool IsTagEnd(Token<TData> token) =>
        (UseShortTags && IsClosingTagType(token)) ||
        IsTagEndType(token);

    protected virtual TokenData<TData> GetTagName(ITokenEnumerator<TData> enumerator)
    {
        if (!enumerator.Current.IsType(TokenType.Identifier))
        {
            AddError(TagErrorType.TagNameNotFound, ExceptionSeverity.Error);
            return default;
        }

        return enumerator.Current.Data;
    }

    protected virtual ILiteralExpression<TData>? GetAttributeName(ITokenEnumerator<TData> enumerator, INode? parent)
    {
        if (!enumerator.Current.IsType(TokenType.Identifier))
        {
            AddError(ErrorType.ExpectedIdentifier, ExceptionSeverity.Error);
            return null;
        }
        return new IdentifierLiteral<TData>(enumerator.Current.Data, parent);
    }

    protected virtual INode? GetAttributeValue(ITokenEnumerator<TData> enumerator, INode? parent)
    {
        Token<TData> current = enumerator.Current;
        if (IsNumber(current))
        {
            return new IntegerLiteral<TData>(current.Data, current.Type, parent);
        }

        if (IsString(current))
        {
            return new StringLiteral<TData>(current.Data, parent);
        }
        
        SurroundFormatter<TokenData<TData>> got = new(current.Data, "\"");
        AddError(TagErrorType.ExpectedAttributeValue, got, new EmptyFormatter(), ExceptionSeverity.Error);
        return null;
    }

    protected virtual IReadOnlyList<TagAttribute<TData>> GetAttributes(ITokenEnumerator<TData> enumerator, INode? parent)
    {
        if (!enumerator.MoveNext())
        {
            AddError(TagErrorType.TagNotClosed, ExceptionSeverity.Error);
            return Array.Empty<TagAttribute<TData>>();
        }

        Token<TData> current = enumerator.Current;
        if (IsTagEnd(current))
        {
            return Array.Empty<TagAttribute<TData>>();
        }

        List<TagAttribute<TData>> attributes = new(4);
        do
        {
            TagAttribute<TData> attribute = new(null!, null!);
            ILiteralExpression<TData>? name = GetAttributeName(enumerator, attribute);

            if (name is null)
            {
                break;
            }

            if (!enumerator.NextTokenIsType(TokenType.Equals))
            {
                SurroundFormatter<TokenData<TData>> got = new(enumerator.Current.Data, "\"");
                SurroundFormatter<char> expected = new('=', "\'");
                AddError(ErrorType.ExpectedValue, got, expected, ExceptionSeverity.Error);
                break;
            }

            if (!enumerator.MoveNext())
            {
                AddError(TagErrorType.ExpectedAttributeValue, ExceptionSeverity.Error);
                break;
            }

            INode? value = GetAttributeValue(enumerator, attribute);
            if (value is null)
            {
                break;
            }

            if (!enumerator.MoveNext())
            {
                AddError(TagErrorType.TagNotClosed, ExceptionSeverity.Error);
                break;
            }

            attribute.Name = name;
            attribute.Value = value;
            attributes.Add(attribute);
        } while (!IsTagEnd(enumerator.Current));

        return attributes;
    }
    
    protected IReadOnlyList<Token<TData>> GetTextTokens(ITokenEnumerator<TData> enumerator)
    {
        Token<TData> current = enumerator.Current;
        if (IsTagStartType(current) || 
            IsClosingTagType(current) ||
            !enumerator.MoveNext())
        {
            return Array.Empty<Token<TData>>();
        }

        return GetTextTokensInternal(enumerator, current);
    }
    
    private IReadOnlyList<Token<TData>> GetTextTokensInternal(ITokenEnumerator<TData> enumerator, Token<TData> current)
    {
        if (IsTagStartType(enumerator.Current) || 
            IsClosingTagType(enumerator.Current))
        {
            return new SingleItemList<Token<TData>>(current);
        }
        
        List<Token<TData>> tokens = new(8) { current };

        do
        {
            tokens.Add(enumerator.Current);
            if (!enumerator.MoveNext())
            {
                break;
            }
        } while (!IsTagStartType(enumerator.Current));

        return tokens;
    }

    protected IReadOnlyList<Tag<TData>> GetChildren(ITokenEnumerator<TData> enumerator, INode? parent)
    {
        Token<TData> token = enumerator.Current;
        if (!enumerator.MoveNext() ||
            IsClosingTagType(enumerator.Current))
        {
            return Array.Empty<Tag<TData>>();
        }

        List<Tag<TData>> children = new(8);

        do
        {
            if (!IsTagStartType(token))
            {
                IReadOnlyList<Token<TData>> tokens = GetTextTokensInternal(enumerator, token);
                children.Add(new TextTag<TData>(tokens));
            }
            else
            {
                Tag<TData>? tag = GetTagInternal(enumerator, parent);
                if (tag is null)
                {
                    break;
                }
                children.Add(tag);

                if (!enumerator.MoveNext())
                {
                    break;
                }
            }
            
            Token<TData> currentToken = enumerator.Current;
            if (!enumerator.MoveNext())
            {
                AddError(TagErrorType.ExpectedATag, new EmptyFormatter(), currentToken.Data, ExceptionSeverity.Error);
                break;
            }

            if (IsClosingTagType(enumerator.Current))
            {
                break;
            }

            token = currentToken;
        } while (true);

        return children;
    }

    private Tag<TData>? GetTagInternal(ITokenEnumerator<TData> enumerator, INode? parent)
    {
        TokenData<TData> name = GetTagName(enumerator);

        Tag<TData> tag = new(name, null!, Array.Empty<Tag<TData>>(), parent);
        IReadOnlyList<TagAttribute<TData>> attributes = GetAttributes(enumerator, tag);
        tag.Attributes = attributes;

        if ((UseShortTags & IsClosingTagType(enumerator.Current)) || IsShortTagType(name))
        {
            if (!enumerator.MoveNext())
            {
                AddError(TagErrorType.TagNotClosed, ExceptionSeverity.Error);
                return null;
            }

            if (IsTagEndType(enumerator.Current))
            {
                return tag;
            }
            AddError(TagErrorType.TagNotClosed, ExceptionSeverity.Error);
            return tag;
        }

        if (!enumerator.MoveNext())
        {
            AddError(TagErrorType.TagNotClosed, ExceptionSeverity.Error);
            return tag;
        }
        
        IReadOnlyList<Tag<TData>> children = GetChildren(enumerator, tag);
        tag.Children = children;

        if (!IsClosingTagType(enumerator.Current) || 
            !enumerator.MoveNext())
        {
            AddError(TagErrorType.TagNotClosed, ExceptionSeverity.Error);
            return null;
        }

        TokenData<TData> endTagName = GetTagName(enumerator);
        if (!name.Span.SequenceEqual(endTagName.Span))
        {
            AddError(TagErrorType.OpeningAndClosingTagsDontMatch, endTagName, name, ExceptionSeverity.Error);
            return null;
        }

        if (!enumerator.MoveNext() ||
            !IsTagEndType(enumerator.Current))
        {
            AddError(TagErrorType.TagNotClosed, ExceptionSeverity.Error);
            return null;
        }

        return tag;
    }

    protected Tag<TData>? GetTag(ITokenEnumerator<TData> enumerator)
    {
        Token<TData> currentToken = enumerator.Current;
        if (!enumerator.MoveNext())
        {
            AddError(TagErrorType.ExpectedATag, new EmptyFormatter(), currentToken.Data, ExceptionSeverity.Error);
            return null;
        }

        return GetTagInternal(enumerator, null);
    }

    protected virtual INode? TokenNotFound(Token<TData> token, ITokenEnumerator<TData> enumerator) => null;
    protected override INode? GetNode(Token<TData> token, ITokenEnumerator<TData> enumerator)
    {
        if (IsTagStartType(token))
        {
            return GetTag(enumerator);
        }

        return TokenNotFound(token, enumerator);
    }

    protected override ISemanticModel<TData> CreateSemanticModel() => 
        new TagSemanticModel<TData>(this);
}