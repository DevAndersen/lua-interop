using LuaInterop.SourceGen.Builders;
using static LuaInterop.SourceGen.GeneratorHelper;

namespace LuaInterop.SourceGen;

[Generator(LanguageNames.CSharp)]
internal class LuaGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Gather compilation data.
        IncrementalValueProvider<CompilationData?> compilationDataProvider = context.CompilationProvider.Select(BuildCompilationData);

        // Check for method attributes.
        IncrementalValuesProvider<IMethodSymbol?> methodProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            GeneratorConstants.LuaFunctionAttributeFullName,
            static (syntaxNode, _) => syntaxNode is MethodDeclarationSyntax,
            static (syntaxContext, _) => syntaxContext.TargetSymbol as IMethodSymbol);

        // Group methods and compilation data.
        IncrementalValueProvider<(ImmutableArray<IMethodSymbol?> methodSymbols, CompilationData? metadata)> combinedProvider = methodProvider
            .Collect()
            .Combine(compilationDataProvider);

        // Generate output.
        context.RegisterSourceOutput(combinedProvider, BuildSource);
    }

    private static CompilationData? BuildCompilationData(Compilation compilation, CancellationToken cancellationToken)
    {
        // Attempt to resolve the type symbol for LuaLibraryAttribute.
        INamedTypeSymbol? assemblyAttributeTypeSymbol = compilation.GetTypeByMetadataName(GeneratorConstants.LuaLibraryAttributeFullName);
        if (assemblyAttributeTypeSymbol == null)
        {
            return null; // Todo: Emit diagnostics.
        }

        // Attempt to resolve the type symbol for LuaFunctionAttribute.
        INamedTypeSymbol? methodAttributeTypeSymbol = compilation.GetTypeByMetadataName(GeneratorConstants.LuaFunctionAttributeFullName);
        if (methodAttributeTypeSymbol == null)
        {
            return null; // Todo: Emit diagnostics.
        }

        // Attempt to resolve the type symbol for LuaFunctionAttribute.
        INamedTypeSymbol? unmanagedCallersOnlyAttributeTypeSymbol = compilation.GetTypeByMetadataName(GeneratorConstants.UnmanagedCallersOnlyAttributeFullName);
        if (unmanagedCallersOnlyAttributeTypeSymbol == null)
        {
            return null; // Todo: Emit diagnostics.
        }

        // Create a dictionary containing INamedTypeSymbols for commonly used types.
        TypeDictionary typeDictionary = new TypeDictionary
        {
            [TypeDictionaryId.Int] = compilation.GetSpecialType(SpecialType.System_Int32),
            [TypeDictionaryId.IntPtr] = compilation.GetSpecialType(SpecialType.System_IntPtr),
            [TypeDictionaryId.Dictionary2] = GetTypeByMetadataName(compilation, GeneratorConstants.TypeMetadataNameDictionary2),
            [TypeDictionaryId.LuaLibraryAttribute] = assemblyAttributeTypeSymbol,
            [TypeDictionaryId.LuaFunctionAttribute] = methodAttributeTypeSymbol,
            [TypeDictionaryId.UnmanagedCallersOnlyAttribute] = unmanagedCallersOnlyAttributeTypeSymbol
        };

        return new CompilationData(
            compilation.AssemblyName,
            compilation.Assembly,
            typeDictionary);
    }

    private static void BuildSource(SourceProductionContext context, (ImmutableArray<IMethodSymbol?> methodSymbols, CompilationData? metadata) data)
    {
        // Deconstruct.
        (ImmutableArray<IMethodSymbol?> methodSymbols, CompilationData? compilationData) = data;

        // Validate compilation data.
        if (compilationData == null)
        {
            return;
        }

        // Deconstruct compilation data.
        (string? assemblyName, IAssemblySymbol assembly, TypeDictionary typeDictionary) = compilationData;

        // Ensure that the assembly name is non-empty.
        if (IsNullOrWhiteSpace(assemblyName))
        {
            return; // Todo: Emit diagnostics.
        }

        // Check if the assembly has been decorated with LuaLibraryAttribute.
        AttributeData? matchingAttribute = GetAttributeData(assembly, typeDictionary[TypeDictionaryId.LuaLibraryAttribute]);
        if (matchingAttribute == null)
        {
            return;
        }

        // Todo: Validate assembly name (Lua appears to require all lower-case?)

        // Null check to satisfy nullability analyzer.
        IMethodSymbol[] methodSymbolArray = methodSymbols.OfType<IMethodSymbol>().ToArray();

        CompilationUnitSyntax compilationUnit = LuaEntryPointBuilder.CreateCompilationUnit(
            assemblyName,
            methodSymbolArray,
            typeDictionary,
            context);

        SyntaxTree syntaxTree = SF.SyntaxTree(compilationUnit.NormalizeWhitespace(), encoding: Encoding.Unicode);
        context.AddSource($"{assemblyName}.g.cs", syntaxTree.GetText());

        // Determine if the module initializer class should be generated.
        bool generateModuleInitializerClass = TryGetAttributeValue(GeneratorConstants.LuaLibraryAttributeInitializerArgumentName, assembly, typeDictionary[TypeDictionaryId.LuaLibraryAttribute], out bool attributeValue)
            ? attributeValue
            : true;

        if (generateModuleInitializerClass)
        {
            SyntaxTree moduleInitializerSyntaxTree = SF.SyntaxTree(ModuleInitializerBuilder.GenerateModuleInitializer().NormalizeWhitespace(), encoding: Encoding.Unicode);
            context.AddSource($"{GeneratorConstants.ModuleInitializerClassName}.g.cs", moduleInitializerSyntaxTree.GetText());
        }
    }

    private record CompilationData(
        string? AssemblyName,
        IAssemblySymbol Assembly,
        TypeDictionary TypeDictionary);
}
