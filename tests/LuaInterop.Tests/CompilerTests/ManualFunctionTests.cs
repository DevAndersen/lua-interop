using LuaInterop.SourceGen;

namespace LuaInterop.Tests.CompilerTests;

public class ManualFunctionTests
{
    [Fact]
    public void ManualLuaFunction_Correct_CompilesWithNoDiagnostics()
    {
        // Arrange
        string csharp = """
            using LuaInterop.Attributes;
            using System.Runtime.InteropServices;

            [assembly: LuaLibrary]

            namespace Test;

            public static class EntryPoint
            {
                [LuaFunction(ManualFunction = true)]
                [UnmanagedCallersOnly]
                public static int ManualMethod(nint luaState)
                {
                    return 0;
                }
            }
            """;

        // Act
        IAssemblySymbol assembly = CompilationHelper.Compile(csharp, out ImmutableArray<Diagnostic> diagnostics);

        // Assert
        Assert.Empty(diagnostics);

        INamedTypeSymbol? entryPointClassTypeSymbol = assembly.GetTypeByMetadataName(TestConstants.LuaEntryPointClassName);
        Assert.NotNull(entryPointClassTypeSymbol);
        Assert.Empty(entryPointClassTypeSymbol.GetMembers("Test_EntryPoint_ManualMethod"));
    }

    [Fact]
    public void ManualLuaFunction_NoUnmanagedCallersOnlyAttribute_Diagnostics()
    {
        // Arrange
        string csharp = """
            using LuaInterop.Attributes;

            [assembly: LuaLibrary]

            namespace Test;

            public static class EntryPoint
            {
                [LuaFunction(ManualFunction = true)]
                public static int ManualMethod(nint luaState)
                {
                    return 0;
                }
            }
            """;

        // Act
        CompilationHelper.Compile(csharp, out ImmutableArray<Diagnostic> diagnostics);

        // Assert
        Assert.Diagnostics(diagnostics, Diagnostics.ManualMethodMissingUnmanagedCallersOnlyAttribute.Id);
    }

    [Fact]
    public void ManualLuaFunction_WrongParameterType_Diagnostics()
    {
        // Arrange
        string csharp = """
            using LuaInterop.Attributes;
            using System.Runtime.InteropServices;

            [assembly: LuaLibrary]

            namespace Test;

            public static class EntryPoint
            {
                [LuaFunction(ManualFunction = true)]
                [UnmanagedCallersOnly]
                public static int ManualMethod(long luaState)
                {
                    return 0;
                }
            }
            """;

        // Act
        CompilationHelper.Compile(csharp, out ImmutableArray<Diagnostic> diagnostics);

        // Assert
        Assert.Diagnostics(diagnostics, Diagnostics.ManualMethodNotAcceptIntPtr.Id);
    }

    [Fact]
    public void ManualLuaFunction_WrongReturnType_Diagnostics()
    {
        // Arrange
        string csharp = """
            using LuaInterop.Attributes;
            using System.Runtime.InteropServices;

            [assembly: LuaLibrary]

            namespace Test;

            public static class EntryPoint
            {
                [LuaFunction(ManualFunction = true)]
                [UnmanagedCallersOnly]
                public static long ManualMethod(nint luaState)
                {
                    return 0;
                }
            }
            """;

        // Act
        CompilationHelper.Compile(csharp, out ImmutableArray<Diagnostic> diagnostics);

        // Assert
        Assert.Diagnostics(diagnostics, Diagnostics.ManualMethodNotReturnInt.Id);
    }
}
