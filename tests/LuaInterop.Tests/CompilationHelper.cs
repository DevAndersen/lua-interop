using LuaInterop.Attributes;
using LuaInterop.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Reflection;
using Xunit;

namespace LuaInterop.Tests;

internal static class CompilationHelper
{
    /// <summary>
    /// Compiles <paramref name="csharp"/>, returning the compiled result and any potential <paramref name="diagnostics"/>.
    /// </summary>
    /// <param name="csharp"></param>
    /// <param name="diagnostics"></param>
    /// <returns></returns>
    public static IAssemblySymbol Compile(
        string csharp,
        out ImmutableArray<Diagnostic> diagnostics,
        string? assemblyName = TestConstants.TestAssemblyName)
    {
        Compilation compilationInput = CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(csharp)],
            [
                MetadataReference.CreateFromFile(typeof(int).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                MetadataReference.CreateFromFile(typeof(LuaLibraryAttribute).GetTypeInfo().Assembly.Location)
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        IIncrementalGenerator generator = new LuaGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        var ranDriver = driver.RunGeneratorsAndUpdateCompilation(
            compilationInput,
            out Compilation compilationOutput,
            out diagnostics);

        // Ensure there are no compilation-related diagnostics.
        ImmutableArray<Diagnostic> compilationDiagnostics = compilationOutput.GetDiagnostics();
        if (compilationDiagnostics.Length != 0)
        {
            Assert.Fail($"""
                Compilation emitted unexpected diagnostics:
                {string.Join(Environment.NewLine, compilationDiagnostics)}
                """);
        }

        return compilationOutput.Assembly;
    }
}
