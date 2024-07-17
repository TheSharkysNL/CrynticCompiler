using System;
namespace CrynticCompiler.Tokenizer;

/// <summary>
/// represents a error within the tokenizer
/// </summary>
public enum ErrorType
{
    ValueNotFound,
    StringNeverClosed,
    CharacterNeverClosed,
    MultiLineStringNeverClosed
}

