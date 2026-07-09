using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using Xunit;

namespace LuaInterop.Tests.CompilerTests;

/// <summary>
/// Contains baseline tests which indicates that the source generator does not produce output when not expected.
/// </summary>
public class NoGeneratorOutputTests
{
    [Fact]
    public async Task Compile_EmptySource_EntryPointNotExists()
    {
        // Arrange
        string csharp = string.Empty;

        // Act
        IAssemblySymbol assembly = CompilationHelper.Compile(csharp, out ImmutableArray<Diagnostic> diagnostics);

        // Assert
        Assert.Empty(diagnostics);
        Assert.Empty(assembly.GetAttributes());
        Assert.Null(assembly.GetTypeByMetadataName(TestConstants.LuaEntryPointClassName));
    }

    [Fact]
    public async Task Compile_NoLuaLibraryAttribute_EntryPointNotExists()
    {
        // Arrange
        string csharp = """
            namespace Test
            {
                public static class Class
                {

                }
            }
            """;

        // Act
        IAssemblySymbol assembly = CompilationHelper.Compile(csharp, out ImmutableArray<Diagnostic> diagnostics);

        // Assert
        Assert.Empty(diagnostics);
        Assert.Empty(assembly.GetAttributes());
        Assert.Null(assembly.GetTypeByMetadataName(TestConstants.LuaEntryPointClassName));
    }
}
