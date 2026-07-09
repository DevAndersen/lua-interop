using LuaInterop.SourceGen;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Xunit;

namespace LuaInterop.Tests.CompilerTests;

public class ManualFunctionTests
{
    [Fact]
    public async Task ManualLuaFunction_Correct_CompilesWithNoDiagnostics()
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
    public async Task ManualLuaFunction_NoUnmanagedCallersOnlyAttribute_Diagnostics()
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
        if (!(diagnostics is [Diagnostic diag] && diag.Id == Diagnostics.ManualMethodMissingUnmanagedCallersOnlyAttribute.Id))
        {
            Assert.Fail();
        }
    }

    [Fact]
    public async Task ManualLuaFunction_WrongParameterType_Diagnostics()
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
        if (!(diagnostics is [Diagnostic diag] && diag.Id == Diagnostics.ManualMethodNotAcceptIntPtr.Id))
        {
            Assert.Fail();
        }
    }

    [Fact]
    public async Task ManualLuaFunction_WrongReturnType_Diagnostics()
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
        if (!(diagnostics is [Diagnostic diag] && diag.Id == Diagnostics.ManualMethodNotReturnInt.Id))
        {
            Assert.Fail();
        }
    }
}
