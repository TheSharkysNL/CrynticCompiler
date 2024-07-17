namespace CrynticCompiler.Tokenizer;

/// <summary>
/// contains the types that a <see cref="Token{TData}"/> can be
/// </summary>
public class TokenType : ExtendableEnum
{
    /// <summary>
    /// Given to a token with no known type
    /// </summary>
    public static readonly int None = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is an identifier
    /// </summary>
    public static readonly int Identifier = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a base 10 number
    /// </summary>
    public static readonly int Number = AutoIncrement();
    /// <summary>
    /// Given to a token to specify that it is a base 16 number, eg: hexadecimal
    /// </summary>
    public static readonly int Hexadecimal = AutoIncrement();
    /// <summary>
    /// Given to a token to specify that it is a base 2 number, eg: binary
    /// </summary>
    public static readonly int Binary = AutoIncrement();
    /// <summary>
    /// Given to a token to specify that it is a base 8 number, eg: octal
    /// </summary>
    public static readonly int Octal = AutoIncrement();
    /// <summary>
    /// Given to a token to specify that it is a floating point number, eg: decimal
    /// </summary>
    public static readonly int Float = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '+'
    /// </summary>
    public static readonly int Plus = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '-'
    /// </summary>
    public static readonly int Minus = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '/'
    /// </summary>
    public static readonly int ForwardSlash = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '*'
    /// </summary>
    public static readonly int Star = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '%'
    /// </summary>
    public static readonly int Percentage = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that the next tokens are one scope in compared to the previous tokens.
    /// The type <see cref="ScopeOut"/> specifies the opposite.
    /// </summary>
    public static readonly int ScopeIn = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that the next tokens are one scope out compared to the previous tokens.
    /// The type <see cref="ScopeIn"/> specifies the opposite.
    /// </summary>
    public static readonly int ScopeOut = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a whitespace character, eg: ' \t\n\r'
    /// </summary>
    public static readonly int WhiteSpace  = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '&'
    /// </summary>
    public static readonly int Ampersand = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '|'
    /// </summary>
    public static readonly int VerticalBar = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '^'
    /// </summary>
    public static readonly int Caret = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '~'
    /// </summary>
    public static readonly int Tilde = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '('
    /// </summary>
    public static readonly int LeftParenthesis = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a ')'
    /// </summary>
    public static readonly int RightParenthesis = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a ','
    /// </summary>
    public static readonly int Comma = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '='
    /// </summary>
    public new static readonly int Equals = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a string of characters,
    /// which are surrounded by string identifiers, eg: '"'
    /// </summary>
    public static readonly int String = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '!'
    /// </summary>
    public static readonly int ExclamationMark = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '{'
    /// </summary>
    public static readonly int LeftBrace = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '}'
    /// </summary>
    public static readonly int RightBrace = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '['
    /// </summary>
    public static readonly int LeftBracket = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a ']'
    /// </summary>
    public static readonly int RightBracket = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a ';'
    /// </summary>
    public static readonly int SemiColon = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '.'
    /// </summary>
    public static readonly int Dot = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '?'
    /// </summary>
    public static readonly int QuestionMark = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '<![CDATA[<]]>'
    /// </summary>
    public static readonly int LessThan = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '>'
    /// </summary>
    public static readonly int GreaterThan = AutoIncrement();
    
    /// <summary>
    /// Given to a token to specify that it is a '$'
    /// </summary>
    public static readonly int DolarSign = AutoIncrement();
    
    /// <summary>
    /// Given to a token to specify that it is a ':'
    /// </summary>
    public static readonly int Colon = AutoIncrement();
    
    /// <summary>
    /// Given to a token to specify that it is a '@'
    /// </summary>
    public static readonly int At = AutoIncrement();

    /// <summary>
    /// Identifies the start of a array of tokens which are within a comment
    /// </summary>
    public static readonly int CommentStart = AutoIncrement();

    /// <summary>
    /// Identifies the end of a comment must come after <see cref="CommentStart"/>
    /// </summary>
    public static readonly int CommentEnd = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '\n'
    /// </summary>
    public static readonly int NewLine = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '"'
    /// </summary>
    public static readonly int DoubleQuotes = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '''
    /// </summary>
    public static readonly int SingleQuote = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a '#'
    /// </summary>
    public static readonly int Hashtag = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a character
    /// </summary>
    public static readonly int Character = AutoIncrement();

    /// <summary>
    /// Given to a token to specify that it is a multiline string.
    /// This is the same as a <see cref="String"/> except it includes new lines, eg: '\n'
    /// </summary>
    public static readonly int MultiLineString = AutoIncrement();
}

