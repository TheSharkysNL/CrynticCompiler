## Table of contents

- [How to use](#how-to-use)
  - [Tokenization](#tokenization)
    - [TokenType](#tokentype)  
  - [Parser](#parser)
    - [Semantic model](#semantic-model)
  - [Generation](#generation)
- [Example](#example)

# How to use

First download the [latest release](https://github.com/TheSharkysNL/CrynticCompiler/releases/) from the github page. 
Once you have done that you can now include the code as a reference.

```csproj
<ItemGroup>
    <Reference Include="{PathToDll}CrynticCompiler.dll" />
</ItemGroup>
```

## Tokenization

Once you have included the cryntic compiler. 
You can now inherit from the `Tokenizer<TData>` class which resides in the `CrynticCompiler.Tokenizer` namespace. 
You can create a tokenizer which uses generic byte sizes like so:

```c#
public partial class MyTokenizer<TData> : Tokenizer<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    public MyTokenizer(Reader<TData> reader) : base(reader)
    { }

    public MyTokenizer(Reader<TData> reader, IReadOnlyDictionary<ReadOnlyMemory<TData>, int> keywords) : base(reader, keywords)
    { }

    protected override CompilationMode Mode => CompilationMode.Debug;
}
```

This can be a little hard to manage so you can also create one using byte (for utf-8), char (for utf-16) or int/uint (for utf-32) like so:

```c#
public partial class MyTokenizer : Tokenizer<byte>
{
    public MyTokenizer(Reader<byte> reader) : base(reader)
    { }

    public MyTokenizer(Reader<byte> reader, IReadOnlyDictionary<ReadOnlyMemory<byte>, int> keywords) : base(reader, keywords)
    { }

    protected override CompilationMode Mode => CompilationMode.Debug;
}
```

With the `Tokenizer<TData>` class comes some methods which can be overriden.
To customize the tokenizer to your own needs.

```c#
public partial class MyTokenizer<TData> : Tokenizer<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    /*
        This will override the string tokenization implementation.
        Strings will now look like '{text}' or "{text}" 
        instead of only "{text}"
    */
    private TData previousStringChar;
    
    protected override bool IsStringStart(TData value)
    {
        if (TokenizerHelpers.IsCharacter(value, '\"') || TokenizerHelpers.IsCharacter(value, '\''))
        {
            previousStringChar = value;
            return true;
        }
        return false;
    }
    
    protected override bool IsStringEnd(TData value) =>
        value == previousStringChar;
}
```

### TokenType

The tokenizer has a `GetEnumerator()` function which will enumerate over tokens (`Token<TData>`).
These tokens have token types which can be found in the `TokenType` class.
To add your own types you can inherit from this class like so:

```c#
using CrynticCompiler.Tokenizer;

public class MyTypes : TokenType
{
    public static readonly int MyType = AutoIncrement();
}
// this can now be accessed using 
int type = MyTypes.MyType;
```

You can also create your own tokenizer by implementing the `ITokenizer<TData>` interface.

## Parser

You can create a parser by overriding the `ParseTree<TData>` class within the `CrynticCompiler.Parser` namespace.
A parse tree is a `IReadOnlyList<Node<TData>>`.
You can implement your own parser like so:

```c#
using CrynticCompiler.Parser;

public class MyParser<TData> : ParseTree<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    public override CompilationMode Mode => CompilationMode.Debug;
    
    public MyParser(ITokenizer<TData> tokenizer)
        : base(tokenizer)
    { }
    
    /*
        You can enumerate over the tokens by using enumerator.MoveNext().
        The `token` is the starting token in the enumeration.
        Some nodes are already implemented in the CrynticCompiler.Parser.Nodes namespace
    */
    protected override INode? GetNode(Token<TData> token, ITokenEnumerator<TData> enumerator)
    {
        throw new NotImplementedException();
    }

    // see the paragraph about semantic models
    protected override ISemanticModel<TData> CreateSemanticModel()
    {
        throw new NotImplementedException();
    }
}
```

Or you can use a pre-made parser which can be included as a project reference, like so:

```csproj
<ItemGroup>
    <ProjectReference Include="{PathToFolder}/CrynticCompiler.Json/CrynticCompiler.Json.csproj" />
</ItemGroup>
```

Some of the pre-made parsers can also have their own tokenizers.

You can also create your own parser by implementing the `IParseTree<TData>` interface.

### Semantic model

For your own parser you can use a semantic model, which is already partially implemented when using the `SemanticModel<TData>` class.
Or you can create your own by implementing the `ISemanticModel<TData>` interface

## Generation

There is currently no generation class only an `IGenerator<TData>` interface which is not used.

At the moment you have to make your own generator to get your desired output.

# Example

Find the example on my [github page](https://github.com/TheSharkysNL/CrynticCompiler/blob/master/Example/Program.cs)

