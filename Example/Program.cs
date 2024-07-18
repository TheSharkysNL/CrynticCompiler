using System.Diagnostics;
using System.Numerics;
using System.Text;
using CrynticCompiler.Json.Parser;
using CrynticCompiler.Json.Parser.Nodes;
using CrynticCompiler.Json.Parser.Symbols;
using CrynticCompiler.Json.Tokenizer;
using CrynticCompiler.Parser;
using CrynticCompiler.Parser.Symbols;
using CrynticCompiler.Tokenizer;
using CrynticCompiler.Tokenizer.IO;
using System.Collections;
using CrynticCompiler;

namespace Example;

public static class Program
{
    private static readonly string pathToFile = Path.GetFullPath($"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}") + "large-file.json";
    public const CompilationMode Mode = CompilationMode.Release;

    public static void Main(string[] args)
    {
        RunCompiler<byte>(pathToFile);
    }

    private static void RunCompiler<T>(string path)
        where T : unmanaged, IBinaryInteger<T>
    {
        MyTokenizer<T> tokenizer = new(Reader.Create<T>(path));
        MyParser<T> parseTree = new(tokenizer);
        
        foreach (var error in parseTree.Errors)
        {
            error.WriteToConsole();
        }

        if (parseTree.Errors.Count == 0)
        {
            IJsonNode node = (IJsonNode)parseTree[0].Value!; // json only has top level statements
            
            ISemanticModel<T> model = parseTree.SemanticModel; // get semantic model
            ISymbol<T>? symbol = model.GetSymbol(node); //  get symbol for top level statement
            JsonCollectionSymbol<T> collection = (JsonCollectionSymbol<T>)symbol!; // statement will always be of a collection type
            
            ICollection objects = collection.GetCollection(); // get the collection 
            IEnumerable<Dictionary<string, object?>> enumerable = objects.Cast<Dictionary<string, object?>>(); // cast to dictionaries
            IEnumerable<object?> obj = enumerable
                .Where(dict => dict.ContainsKey("id"))
                .Select(dict => dict["id"]); // get all ids

            foreach (object? id in obj)
            {
                Console.WriteLine(id); // write all ids
            }
        }
    }
}

sealed class MyTokenizer<TData> : JsonTokenizer<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    public MyTokenizer(Reader<TData> reader) : base(reader)
    {
    }

    public MyTokenizer(Reader<TData> reader, IReadOnlyDictionary<ReadOnlyMemory<TData>, int> keywords) : base(reader, keywords)
    {
    }

    protected override CompilationMode Mode => Program.Mode;
}

sealed class MyParser<TData>(ITokenizer<TData> tokenizer) : JsonParseTree<TData>(tokenizer) where TData : unmanaged, IBinaryInteger<TData>
{
    public override CompilationMode Mode => Program.Mode;
}