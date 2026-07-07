using static LuaInterop.SourceGen.GeneratorHelper;

namespace LuaInterop.SourceGen;

[Generator(LanguageNames.CSharp)]
internal class Generator : IIncrementalGenerator
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

        IncrementalValuesProvider<IMethodSymbol?> manualMethodProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            GeneratorConstants.ManualLuaFunctionAttributeFullName,
            static (syntaxNode, _) => syntaxNode is MethodDeclarationSyntax,
            static (syntaxContext, _) => syntaxContext.TargetSymbol as IMethodSymbol);

        // Group methods and pair them with the assembly.
        IncrementalValueProvider<((ImmutableArray<IMethodSymbol?> autoMethods, ImmutableArray<IMethodSymbol?> manualMethods) Left, CompilationData? metadata)> combinedProvider = methodProvider.Collect().Combine(manualMethodProvider.Collect()).Combine(compilationDataProvider);

        // Generate output.
        context.RegisterSourceOutput(combinedProvider, BuildSource);
    }

    private static CompilationData? BuildCompilationData(Compilation compilation, CancellationToken cancellationToken)
    {
        // Attempt to resolve the type symbol for LuaLibraryAttribute.
        INamedTypeSymbol? assemblyAttributeTypeSymbol = compilation.GetTypeByMetadataName(GeneratorConstants.LuaLibraryAttributeFullName);
        if (assemblyAttributeTypeSymbol == null)
        {
            return null;
        }

        // Attempt to resolve the type symbol for LuaFunctionAttribute.
        INamedTypeSymbol? methodAttributeTypeSymbol = compilation.GetTypeByMetadataName(GeneratorConstants.LuaFunctionAttributeFullName);
        if (methodAttributeTypeSymbol == null)
        {
            return null;
        }

        // Create a dictionary containing INamedTypeSymbols for commonly used types.
        TypeDictionary typeDictionary = new TypeDictionary
        {
            [TypeDictionaryId.Int] = compilation.GetSpecialType(SpecialType.System_Int32),
            [TypeDictionaryId.IntPtr] = compilation.GetSpecialType(SpecialType.System_IntPtr),
            [TypeDictionaryId.Dictionary2] = GetTypeByMetadataName(compilation, GeneratorConstants.TypeMetadataNameDictionary2),
            [TypeDictionaryId.LuaLibraryAttribute] = assemblyAttributeTypeSymbol,
            [TypeDictionaryId.LuaFunctionAttribute] = methodAttributeTypeSymbol,
        };

        return new CompilationData(
            compilation.AssemblyName,
            compilation.Assembly,
            typeDictionary);
    }

    private static void BuildSource(SourceProductionContext context, ((ImmutableArray<IMethodSymbol?> autoMethods, ImmutableArray<IMethodSymbol?> manualMethods) Left, CompilationData? metadata) data)
    {
        // Deconstruct.
        ((ImmutableArray<IMethodSymbol?> autoMethods, ImmutableArray<IMethodSymbol?> manualMethods), CompilationData? compilationData) = data;

        // Validate compilation data.
        if (compilationData == null)
        {
            return;
        }

        // Deconstruct compilation data.
        (string? assemblyName, IAssemblySymbol assembly, TypeDictionary typeDictionary) = compilationData;

        if (IsNullOrWhiteSpace(assemblyName))
        {
            return;
        }

        AttributeData? matchingAttribute = GetAttributeData(assembly, typeDictionary[TypeDictionaryId.LuaLibraryAttribute]);
        if (matchingAttribute == null)
        {
            return;
        }

        // Todo: Validate assembly name (Lua appears to require all lower-case?)

        // Validate methods.
        IMethodSymbol[] methodSymbolArray = autoMethods.OfType<IMethodSymbol>().ToArray();
        IMethodSymbol[] manualMethodArray = manualMethods.OfType<IMethodSymbol>().ToArray();

        CompilationUnitSyntax compilationUnit = CreateCompilationUnit(
            assemblyName,
            methodSymbolArray,
            typeDictionary[TypeDictionaryId.LuaFunctionAttribute],
            typeDictionary,
            context);

        SyntaxTree syntaxTree = SF.SyntaxTree(compilationUnit.NormalizeWhitespace(), encoding: Encoding.Unicode);
        context.AddSource("SyntaxTreeTest.g.cs", syntaxTree.GetText()); // Todo: Find a better file hint name.
    }

    private static bool TryGetAttributeValue<T>(string argumentName, ISymbol symbol, INamedTypeSymbol attributeTypeSymbol, [NotNullWhen(true)] out T? value)
    {
        AttributeData? matchingAttribute = GetAttributeData(symbol, attributeTypeSymbol);
        if (matchingAttribute == null)
        {
            value = default;
            return false;
        }

        return TryGetAttributeNamedArgument(matchingAttribute, argumentName, out value);
    }

    private static AttributeData? GetAttributeData(ISymbol symbol, INamedTypeSymbol attributeTypeSymbol)
    {
        return symbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.Equals(attributeTypeSymbol, SymbolEqualityComparer.Default) == true);
    }

    private static bool TryGetAttributeNamedArgument<T>(AttributeData attributeData, string argumentName, out T? value)
    {
        KeyValuePair<string, TypedConstant> argument = attributeData.NamedArguments.FirstOrDefault(x => x.Key == argumentName);

        if (!argument.Equals(default) && argument.Value.Value is T typedValue)
        {
            value = typedValue;
            return true;
        }

        value = default;
        return false;
    }

    private static CompilationUnitSyntax CreateCompilationUnit(
        string assemblyName,
        IMethodSymbol[] methodSymbols,
        INamedTypeSymbol methodAttribute,
        TypeDictionary typeDictionary,
        SourceProductionContext context)
    {
        // Class access modifiers
        SyntaxTokenList classAccessModifierSyntax = SF.TokenList(
            SF.Token(SyntaxKind.PublicKeyword),  // Todo: Can the class be private? Check if Lua can call it if private.
            SF.Token(SyntaxKind.StaticKeyword));

        // Class
        ClassDeclarationSyntax classDeclaration = SF.ClassDeclaration(GeneratorConstants.LuaOpenClassName)
            .WithModifiers(classAccessModifierSyntax)
            .WithMembers(SF.List<MemberDeclarationSyntax>([
                GenerateLuaOpenMethod(assemblyName, methodSymbols, methodAttribute, typeDictionary),
                .. LuaFunctionGenerator.GenerateFunctionMethods(methodSymbols, typeDictionary, context)]))
                .WithAttributeLists([
                    SF.AttributeList([GenerateGeneratedCodeAttributeAttribute()])])
                .AddXmlDocumentation("Contains Lua interoperability logic.");

        // Namespace
        NamespaceDeclarationSyntax namespaceDeclaration = SF.NamespaceDeclaration(
            SF.IdentifierName(GeneratorConstants.GeneratedCodeNamespace))
            .AddMembers(classDeclaration);

        return SF.CompilationUnit()
            .WithMembers(SF.SingletonList<MemberDeclarationSyntax>(namespaceDeclaration));
    }

    private static MethodDeclarationSyntax GenerateLuaOpenMethod(
        string assemblyName,
        IMethodSymbol[] methodSymbols,
        INamedTypeSymbol methodAttribute,
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
                    SF.IdentifierName(GeneratorConstants.LuaInteropHelperTypeFullName),
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
            .. methodSymbols.Select(x => GenerateRegisterFunctionInvocation(x, methodAttribute)),
            returnStatement]);

        // Attribute, UnmanagedCallersOnly
        AttributeSyntax unmanagedCallersOnlyAttribute = SF.Attribute(
            SF.IdentifierName(GeneratorConstants.UnmanagedCallersOnlyAttributeFullName),
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
                .WithModifiers(SF.TokenList(
                    SF.Token(SyntaxKind.PublicKeyword), // Todo: Can the luaopen method be private? Check if Lua can call it if private.
                    SF.Token(SyntaxKind.StaticKeyword),
                    SF.Token(SyntaxKind.UnsafeKeyword)))
                .WithParameterList(SF.ParameterList(parameterSyntaxList))
                .WithBody(SF.Block(statementList))
                .WithAttributeLists([
                    SF.AttributeList([unmanagedCallersOnlyAttribute])])
                .AddXmlDocumentation("Entry point for Lua, exposes available functions.");

        return methodDeclaration;
    }

    private static ExpressionStatementSyntax GenerateRegisterFunctionInvocation(IMethodSymbol methodSymbol, INamedTypeSymbol methodAttribute)
    {
        // Determine function name
        string functionName = TryGetAttributeValue(GeneratorConstants.LuaFunctionAttributeNameArgumentName, methodSymbol, methodAttribute, out string? customFunctionName)
            ? customFunctionName
            : methodSymbol.Name;

        // Method invocation
        return SF.ExpressionStatement(SF.InvocationExpression(
            SF.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SF.IdentifierName(GeneratorConstants.LuaInteropHelperTypeFullName),
                SF.IdentifierName(GeneratorConstants.LuaInteropHelperRegisterFunctionMethodName)),
            SF.ArgumentList([
                SF.Argument(SF.IdentifierName(GeneratorConstants.LuaStateVariableName)),
                SF.Argument(SF.LiteralExpression(SyntaxKind.StringLiteralExpression, SF.Literal(functionName))),
                SF.Argument(SF.PrefixUnaryExpression(SyntaxKind.AddressOfExpression, SF.IdentifierName(methodSymbol.Name)))])));
    }

    private record CompilationData(
        string? AssemblyName,
        IAssemblySymbol Assembly,
        TypeDictionary TypeDictionary);
}
