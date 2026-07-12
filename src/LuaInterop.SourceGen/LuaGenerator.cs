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

        CompilationUnitSyntax compilationUnit = CreateCompilationUnit(
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

    private static CompilationUnitSyntax CreateCompilationUnit(
        string assemblyName,
        IMethodSymbol[] methodSymbols,
        TypeDictionary typeDictionary,
        SourceProductionContext context)
    {
        // Filter out method symbols that fail validation.
        IEnumerable<(IMethodSymbol MethodSymbol, bool IsManualMethod)> validateMethods = LuaFunctionGenerator.ValidateFunctionMethods(methodSymbols, typeDictionary, context).ToArray();

        // Todo: Disallow methods with the same names/function names.

        // All validated methods (methods to be registered).
        IMethodSymbol[] validatedMethods = validateMethods
            .Select(x => x.MethodSymbol)
            .ToArray();

        // Validated automatic methods (methods to generate function methods for).
        IMethodSymbol[] validatedAutomaticMethods = validateMethods
            .Where(x => !x.IsManualMethod)
            .Select(x => x.MethodSymbol)
            .ToArray();

        // Class access modifiers
        SyntaxTokenList classAccessModifierSyntax = SF.TokenList(
            SF.Token(SyntaxKind.InternalKeyword),
            SF.Token(SyntaxKind.StaticKeyword));

        // Class
        ClassDeclarationSyntax classDeclaration = SF.ClassDeclaration($"{GeneratorConstants.LuaOpenClassName}_{assemblyName}")
            .WithModifiers(classAccessModifierSyntax)
            .WithMembers([
                GenerateLuaOpenMethod(assemblyName, validatedMethods, typeDictionary),
                .. LuaFunctionGenerator.GenerateFunctionMethods(validatedAutomaticMethods, typeDictionary)])
            .WithAttributeLists([
                SF.AttributeList([GenerateGeneratedCodeAttributeAttribute()])])
            .AddXmlDocumentation("Contains Lua interoperability logic.");

        // Namespace
        NamespaceDeclarationSyntax namespaceDeclaration = SF.NamespaceDeclaration(
            SF.IdentifierName(GeneratorConstants.GeneratedCodeNamespace))
            .AddMembers(classDeclaration);

        return SF.CompilationUnit()
            .WithMembers([namespaceDeclaration]);
    }

    private static MethodDeclarationSyntax GenerateLuaOpenMethod(
        string assemblyName,
        IMethodSymbol[] methodSymbols,
        TypeDictionary typeDictionary)
    {
        // Parameters
        SeparatedSyntaxList<ParameterSyntax> parameterSyntaxList = SF.SeparatedList([
            SF.Parameter(
                SF.Identifier(GeneratorConstants.LuaStateVariableName))
            .WithType(
                SF.IdentifierName(typeDictionary.GetNameOrThrow(TypeDictionaryId.IntPtr)))]);

        // Method invocation, Lua.CreateTable
        ExpressionStatementSyntax createTableMethodInvocation = SF.ExpressionStatement(
            SF.InvocationExpression(
                SF.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SF.IdentifierName(GeneratorConstants.LuaInteropHelperTypeGlobalFullName),
                    SF.IdentifierName(GeneratorConstants.LuaInteropHelperCreateTableMethodName)),
                SF.ArgumentList([
                    SF.Argument(SF.IdentifierName(GeneratorConstants.LuaStateVariableName)),
                    SF.Argument(SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.Literal(methodSymbols.Length)))])));

        // Return statement
        ReturnStatementSyntax returnStatement = SF.ReturnStatement(
            SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.Literal(1)))
            .AddComment("Stack index of table");

        // Method statements
        SyntaxList<StatementSyntax> statementList = SF.List<StatementSyntax>([
            createTableMethodInvocation,
            .. methodSymbols.Select(x => GenerateRegisterFunctionInvocation(x, typeDictionary[TypeDictionaryId.LuaFunctionAttribute])),
            returnStatement]);

        // Attribute, UnmanagedCallersOnly
        AttributeSyntax unmanagedCallersOnlyAttribute = SF.Attribute(
            SF.IdentifierName(GeneratorConstants.UnmanagedCallersOnlyAttributeGlobalFullName),
            SF.AttributeArgumentList([
                SF.AttributeArgument(
                    SF.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SF.Literal($"luaopen_{assemblyName}")))
                .WithNameEquals(
                    SF.NameEquals(
                        SF.IdentifierName(GeneratorConstants.UnmanagedCallersOnlyAttributeEntryPointArgument)))]));

        // Method
        MethodDeclarationSyntax methodDeclaration = SF.MethodDeclaration(
            SF.PredefinedType(SF.Token(SyntaxKind.IntKeyword)),
            SF.Identifier(GeneratorConstants.LuaOpenMethodName))
            .WithModifiers([
                SF.Token(SyntaxKind.PrivateKeyword),
                SF.Token(SyntaxKind.StaticKeyword),
                SF.Token(SyntaxKind.UnsafeKeyword)])
            .WithParameterList(SF.ParameterList(parameterSyntaxList))
            .WithBody(SF.Block(statementList))
            .WithAttributeLists([
                SF.AttributeList([unmanagedCallersOnlyAttribute])])
            .AddXmlDocumentation("Entry point for Lua, exposes available functions.");

        return methodDeclaration;
    }

    private static ExpressionStatementSyntax GenerateRegisterFunctionInvocation(IMethodSymbol methodSymbol, INamedTypeSymbol methodAttribute)
    {
        // Determine function name.
        string functionName = TryGetAttributeValue(GeneratorConstants.LuaFunctionAttributeNameArgumentName, methodSymbol, methodAttribute, out string? customFunctionName)
            ? customFunctionName
            : methodSymbol.Name;

        // Check if the LuaFunction attribute marks the method as manual.
        bool isManualMethod = TryGetAttributeValue(GeneratorConstants.LuaFunctionAttributeManualArgumentName, methodSymbol, methodAttribute, out bool manualAttribute)
            ? manualAttribute
            : false;

        // Determine the name of the method to invoke.
        string methodName = isManualMethod
            ? methodSymbol.GetFullName()
            : LuaFunctionGenerator.GetSafeMethodName(methodSymbol);

        // Method invocation
        return SF.ExpressionStatement(
            SF.InvocationExpression(
                SF.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SF.IdentifierName(GeneratorConstants.LuaInteropHelperTypeGlobalFullName),
                    SF.IdentifierName(GeneratorConstants.LuaInteropHelperRegisterFunctionMethodName)),
                SF.ArgumentList([
                    SF.Argument(SF.IdentifierName(GeneratorConstants.LuaStateVariableName)),
                    SF.Argument(SF.LiteralExpression(SyntaxKind.StringLiteralExpression, SF.Literal(functionName))),
                    SF.Argument(SF.PrefixUnaryExpression(SyntaxKind.AddressOfExpression, SF.IdentifierName(methodName)))])));
    }

    private record CompilationData(
        string? AssemblyName,
        IAssemblySymbol Assembly,
        TypeDictionary TypeDictionary);
}
