using System.Numerics;
using CrynticCompiler.Tokenizer.IO;

namespace CrynticCompiler.Tokenizer;

public interface ITokenEnumerator<TData> : IEnumerator<Token<TData>>
	where TData : unmanaged, IBinaryInteger<TData>
{
    /// <summary>
    /// the reader that is currently being used to read the tokens
    /// </summary>
    public Reader<TData> Reader { get; }
    
    /// <summary>
    /// the current line where the tokenizer is at. this is at the end of the current token statement.
    /// </summary>
    public int Line { get; }
    /// <summary>
    /// the current column where the tokenizer is at. this is at the end of the current token statement.
    /// </summary>
    public int Column { get; }

    /// <summary>
    /// the line where the previous token was found. this is at the start of the current token statement.
    /// </summary>
    public int PreviousLine { get; }
    /// <summary>
    /// the column where the previous token was found. this is at the start of the current token statement.
    /// </summary>
    public int PreviousColumn { get; }

    /// <summary>
    /// the errors that occured within the tokenizer
    /// </summary>
    public IReadOnlyCollection<CompilerError<TData>> Errors { get; }

    /// <summary>
    /// positive if more scopes were added, negative if there were scopes removed.
    /// The value is the amount of scopes that were added or removes
    /// </summary>
    public int ScopeChange { get; }

    /// <summary>
    /// gets the current scope of the tokenizer, used the <see cref="ScopeChange"/> as reference
    /// </summary>
    public int Scope { get; }

    /// <summary>
    /// true if the tokenizer is tokenizing tokens within a comment
    /// </summary>
    public bool InComment { get; }

    /// <summary>
    /// peeks the next token within the tokenizer, a consumable can only be made once, to view the next token use <see cref="PeekAfter(Consumable{TData}, out Consumable{TData})"/>
    /// </summary>
    /// <param name="consumable">the consumable used to view and consume the token</param>
    /// <returns>true if the next token has been found</returns>
    public bool PeekNext(out Consumable<TData> consumable);

    /// <summary>
    /// consumes a <see cref="Consumable{TData}"/>
    /// </summary>
    /// <param name="consumable">the consumable to consume</param>
    public void Consume(Consumable<TData> consumable);

    /// <summary>
    /// adds a runtime token to the <see cref="ITokenEnumerator{TData}"/>
    /// this token should be the next output from the <see cref="IEnumerator{Token{TData}}.Current"/>
    /// if no other tokens were added before it
    /// </summary>
    /// <param name="token">the token to add to the enumerator</param>
    public void AddToken(Token<TData> token);
    
    /// <summary>
    /// adds multiple runtime tokens to the <see cref="ITokenEnumerator{TData}"/>
    /// these tokens should be the next output from the <see cref="IEnumerator{Token{TData}}.Current"/>
    /// </summary>
    /// <param name="tokens">the tokens that will be added to the enumerator</param>
    public void AddTokens(IReadOnlyCollection<Token<TData>> tokens);

    /// <summary>
    /// adds an error to the <see cref="ITokenEnumerator{TData}"/>
    /// </summary>
    /// <param name="error">the error to add</param>
    public void AddError(CompilerError<TData> error);
}

