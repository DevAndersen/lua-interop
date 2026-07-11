namespace LuaInterop.Tests.CompilerTests;

public class LuaLibraryAttributeTests
{
    [Fact]
    public void Compile_AssemblyWithLuaLibraryAttribute_EntryPointExists()
    {
        // Arrange
        CSharpString csharp = new CSharpString("""
            [assembly: global::LuaInterop.Attributes.LuaLibrary]
            """);

        // Act
        IAssemblySymbol assembly = CompilationHelper.Compile(csharp, out ImmutableArray<Diagnostic> diagnostics);

        // Assert
        Assert.Empty(diagnostics);
        Assert.NotNull(assembly.GetTypeByMetadataName(TestConstants.LuaEntryPointClassName));
    }

    [Fact]
    public void Compile_LuaFunction_GeneratesWrapperMethod()
    {
        // Arrange
        CSharpString csharp = new CSharpString("""
            using LuaInterop.Attributes;

            [assembly: LuaLibrary]

            namespace Test;

            public static class EntryPoint
            {
                [LuaFunction]
                public static void Method(string text)
                {
                }
            }
            """);

        // Act
        IAssemblySymbol assembly = CompilationHelper.Compile(csharp, out ImmutableArray<Diagnostic> diagnostics);

        // Assert
        Assert.Empty(diagnostics);

        INamedTypeSymbol? entryPointClassTypeSymbol = assembly.GetTypeByMetadataName(TestConstants.LuaEntryPointClassName);
        Assert.NotNull(entryPointClassTypeSymbol);
        Assert.Single(entryPointClassTypeSymbol.GetMembers("Test_EntryPoint_Method"));
    }
}
