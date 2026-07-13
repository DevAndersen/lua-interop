using LuaInterop.SourceGen.Builders;
using static LuaInterop.SourceGen.GeneratorHelper;

namespace LuaInterop.SourceGen;

[Generator(LanguageNames.CSharp)]
internal class LuaGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Gather compilation data.
        IncrementalValueProvider<CompilationData> compilationDataProvider = context.CompilationProvider.Select(BuildCompilationData);

        // Check for method attributes.
        IncrementalValuesProvider<IMethodSymbol?> methodProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            GeneratorConstants.LuaFunctionAttributeFullName,
            static (syntaxNode, _) => syntaxNode is MethodDeclarationSyntax,
            static (syntaxContext, _) => syntaxContext.TargetSymbol as IMethodSymbol);

        // Group methods and compilation data.
        IncrementalValueProvider<(ImmutableArray<IMethodSymbol?> methodSymbols, CompilationData metadata)> combinedProvider = methodProvider
            .Collect()
            .Combine(compilationDataProvider);

        // Generate output.
        context.RegisterSourceOutput(combinedProvider, BuildSource);
    }

    private static CompilationData BuildCompilationData(Compilation compilation, CancellationToken cancellationToken)
    {
        INamedTypeSymbol? assemblyAttributeTypeSymbol = compilation.GetTypeByMetadataName(GeneratorConstants.LuaLibraryAttributeFullName);
        INamedTypeSymbol? methodAttributeTypeSymbol = compilation.GetTypeByMetadataName(GeneratorConstants.LuaFunctionAttributeFullName);
        INamedTypeSymbol? unmanagedCallersOnlyAttributeTypeSymbol = compilation.GetTypeByMetadataName(GeneratorConstants.UnmanagedCallersOnlyAttributeFullName);

        // Create a dictionary containing INamedTypeSymbols for commonly used types.
        NullableTypeDictionary typeDictionary = new NullableTypeDictionary
        {
            [TypeDictionaryId.Int] = (typeof(int).FullName, compilation.GetSpecialType(SpecialType.System_Int32)),
            [TypeDictionaryId.IntPtr] = (typeof(nint).FullName, compilation.GetSpecialType(SpecialType.System_IntPtr)),
            [TypeDictionaryId.Dictionary2] = (GeneratorConstants.TypeMetadataNameDictionary2, GetTypeByMetadataName(compilation, GeneratorConstants.TypeMetadataNameDictionary2)),
            [TypeDictionaryId.LuaLibraryAttribute] = (GeneratorConstants.LuaLibraryAttributeFullName, assemblyAttributeTypeSymbol),
            [TypeDictionaryId.LuaFunctionAttribute] = (GeneratorConstants.LuaFunctionAttributeFullName, methodAttributeTypeSymbol),
            [TypeDictionaryId.UnmanagedCallersOnlyAttribute] = (GeneratorConstants.UnmanagedCallersOnlyAttributeFullName, unmanagedCallersOnlyAttributeTypeSymbol),
        };

        return new CompilationData(
            compilation.AssemblyName,
            compilation.Assembly,
            typeDictionary);
    }

    private static void BuildSource(SourceProductionContext context, (ImmutableArray<IMethodSymbol?> methodSymbols, CompilationData metadata) data)
    {
        // Deconstruct.
        (ImmutableArray<IMethodSymbol?> methodSymbols, CompilationData compilationData) = data;
        (string? assemblyName, IAssemblySymbol assembly, NullableTypeDictionary nullableTypeDictionary) = compilationData;

        // Validate type dictionary.
        KeyValuePair<TypeDictionaryId, (string FullTypeName, INamedTypeSymbol? NamedTypeSymbol)>[] unresolvedTypes = compilationData
            .TypeDictionary
            .Where(x => x.Value.NamedTypeSymbol == null)
            .ToArray();

        if (unresolvedTypes.Length > 0)
        {
            foreach (KeyValuePair<TypeDictionaryId, (string FullTypeName, INamedTypeSymbol? NamedTypeSymbol)> unresolvedType in unresolvedTypes)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.RequiredTypeMissing,
                    null,
                    unresolvedType.Value.FullTypeName));
            }

            return;
        }

        TypeDictionary typeDictionary = compilationData.TypeDictionary.ToDictionary(
            x => x.Key,
            x => x.Value.NamedTypeSymbol!);

        // Validate the assembly.
        if (IsNullOrWhiteSpace(assemblyName) || !SyntaxFacts.IsValidIdentifier(assemblyName))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.InvalidAssemblyName,
                null,
                assemblyName));

            return;
        }

        // Check if the assembly has been decorated with LuaLibraryAttribute.
        AttributeData? matchingAttribute = GetAttributeData(assembly, typeDictionary[TypeDictionaryId.LuaLibraryAttribute]);
        if (matchingAttribute == null)
        {
            return;
        }

        // Null check to satisfy nullability analyzer.
        IMethodSymbol[] methodSymbolArray = methodSymbols.OfType<IMethodSymbol>().ToArray();

        // Filter out method symbols that fail validation.
        (IMethodSymbol MethodSymbol, string FunctionName, string MethodName, bool IsManualMethod)[] validatedMethods = LuaFunctionBuilder.ValidateFunctionMethods(methodSymbolArray, typeDictionary, context);

        CompilationUnitSyntax compilationUnit = LuaEntryPointBuilder.CreateCompilationUnit(
            assemblyName,
            validatedMethods,
            typeDictionary);

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
        NullableTypeDictionary TypeDictionary);
}
