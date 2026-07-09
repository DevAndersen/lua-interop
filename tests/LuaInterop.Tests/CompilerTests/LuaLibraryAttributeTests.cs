using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Xunit;

namespace LuaInterop.Tests.CompilerTests;

public class LuaLibraryAttributeTests
{
    [Fact]
    public async Task Compile_AssemblyWithLuaLibraryAttribute_EntryPointExists()
    {
        // Arrange
        string csharp = """
            [assembly: global::LuaInterop.Attributes.LuaLibrary]
            """;

        // Act
        IAssemblySymbol assembly = CompilationHelper.Compile(csharp, out ImmutableArray<Diagnostic> diagnostics);

        // Assert
        Assert.Empty(diagnostics);
        Assert.NotNull(assembly.GetTypeByMetadataName(TestConstants.LuaEntryPointClassName));
    }
}
