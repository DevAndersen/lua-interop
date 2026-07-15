using System.Diagnostics.CodeAnalysis;

namespace LuaInterop.Tests;

/// <summary>
/// Helper class to provide syntax highlight for strings of C# source code.
/// </summary>
internal class CSharpString
{
    private readonly string _source;

    public CSharpString([StringSyntax("csharp")] string source)
    {
        _source = source;
    }

    public static implicit operator string(CSharpString source) => source._source;
}
