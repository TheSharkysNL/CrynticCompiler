using System.Numerics;

namespace CrynticCompiler.Parser.Nodes;

public interface IExpression<TData> : INode
    where TData : unmanaged, IBinaryInteger<TData>
{
    // /// <summary>
    // /// returns the string representation of this <see cref="IExpression{TData}"/>
    // /// </summary>
    // /// <returns>the string representation of this <see cref="IExpression{TData}"/></returns>
    // public ReadOnlySpan<TData> GetRepresentation();
    // /// <summary>
    // /// returns the string representation of this <see cref="IExpression{TData}"/>
    // /// using specific formating options
    // /// </summary>
    // /// <param name="options">the formating options used</param>
    // /// <returns>the string representation of this <see cref="IExpression{TData}"/> with the formating applied</returns>
    // public ReadOnlySpan<TData> GetRepresentation(FormatOptions options);
    // /// <summary>
    // /// returns the string representation of this <see cref="IExpression{TData}"/>
    // /// using specific formating options
    // /// </summary>
    // /// <param name="options">the formating options used</param>
    // /// <param name="indentationLevel">the level of the current indentation</param>
    // /// <returns>the string representation of this <see cref="IExpression{TData}"/> with the formating applied</returns>
    // public ReadOnlySpan<TData> GetRepresentation(FormatOptions options, int indentationLevel);
    // /// <summary>
    // /// returns the string representation of this <see cref="IExpression{TData}"/>
    // /// using specific formating options
    // /// and a given format object
    // /// </summary>
    // /// <param name="options">the formating options used</param>
    // /// <param name="indentationLevel">the level of the current indentation</param>
    // /// <param name="formatObject">a format object used to determine extra formating options</param>
    // /// <returns>the string representation of this <see cref="IExpression{TData}"/> with the formating applied</returns>
    // public ReadOnlySpan<TData> GetRepresentation(FormatOptions options, int indentationLevel, object? formatObject);
}

