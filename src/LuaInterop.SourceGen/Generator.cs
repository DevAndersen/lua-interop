using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LuaInterop.SourceGen;

[Generator(LanguageNames.CSharp)]
internal class Generator : IIncrementalGenerator
{
    private const string _luaOpenAttributeFullName = "LuaInterop.Attributes.LuaOpenAttribute";
    private const string _luaFunctionAttributeFullName = "LuaInterop.Attributes.LuaFunctionAttribute";
    private const string _luaFunctionAttributeNameArgumentName = "FunctionName";
    private const string _luaInteropTypeFullName = "global::LuaInterop.Native.Lua";
    private const string _luaInteropHelperTypeFullName = "global::LuaInterop.LuaInteropHelper";
    private const string _unmanagedCallersOnlyAttributeFullName = "global::System.Runtime.InteropServices.UnmanagedCallersOnly";
    private const string _unmanagedCallersOnlyAttributeEntryPointArgument = "EntryPoint";
    private const string _nintFullName = "global::System.IntPtr";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Gather compilation data.
        IncrementalValueProvider<CompilationData> compilationDataProvider = context.CompilationProvider.Select((compilation, _) =>
        {
            INamedTypeSymbol? assemblyAttributeTypeSymbol = compilation.GetTypeByMetadataName(_luaOpenAttributeFullName);
            if (assemblyAttributeTypeSymbol == null)
            {
                return default;
            }

            INamedTypeSymbol? methodAttributeTypeSymbol = compilation.GetTypeByMetadataName(_luaFunctionAttributeFullName);
            if (methodAttributeTypeSymbol == null)
            {
                return default;
            }

            return new CompilationData(compilation.Assembly, assemblyAttributeTypeSymbol, methodAttributeTypeSymbol);
        });

        // Check for method attributes.
        IncrementalValuesProvider<IMethodSymbol?> methodProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            _luaFunctionAttributeFullName,
            static (syntaxNode, _) => syntaxNode is MethodDeclarationSyntax,
            static (syntaxContext, _) => syntaxContext.TargetSymbol as IMethodSymbol);

        // Group methods and pair them with the assembly.
        IncrementalValueProvider<(ImmutableArray<IMethodSymbol?>, CompilationData)> provider = methodProvider.Collect().Combine(compilationDataProvider);

        context.RegisterSourceOutput(provider, (ctx, data) =>
        {
            // Deconstruct.
            (ImmutableArray<IMethodSymbol?> nullableMethodSymbols, CompilationData compilationData) = data;
            (IAssemblySymbol assembly, INamedTypeSymbol assemblyAttribute, INamedTypeSymbol methodAttribute) = compilationData;

            IEnumerable<IMethodSymbol> methodSymbols = nullableMethodSymbols.OfType<IMethodSymbol>();

            if (compilationData == default)
            {
                return;
            }

            AttributeData? matchingAttribute = GetAttributeData(assembly, assemblyAttribute);
            if (matchingAttribute == null)
            {
                return;
            }

            foreach (IMethodSymbol methodSymbol in methodSymbols)
            {
                if (!methodSymbol.IsStatic)
                {
                    // Todo: Disallow instanced methods.
                }

                if (methodSymbol.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
                {
                    // Todo: Disallow unreachable methods.
                }
            }

            string str = TryGetAttributeNamedArgument(matchingAttribute, "Number", out int value) ? value.ToString() : "FAILED";

            string abc(IMethodSymbol symbol)
            {
                if (TryGetAttributeValue(symbol, methodAttribute, _luaFunctionAttributeNameArgumentName, out string? abc))
                {
                    return abc ?? "NULL";
                }
                return "FALSE";
            }

            // language=c#
            string src = $$"""
                namespace Demo.Marker.{{assembly.Name}};

                /*
                > {{assemblyAttribute.Name}} : {{str}}
                > {{assembly.Name}}
                */

                /*
                {{string.Join("\r\n", methodSymbols.Select(x => abc(x)))}}
                */

                public static class Generated2
                {
                    public static void SayHello()
                    {
                        global::System.Console.WriteLine("Hello, World!");
                    }
                }
                """;

            ctx.AddSource($"{compilationData.Assembly.Name}.Test2.g.cs", src);

            CompilationUnitSyntax compilationUnit = CreateCompilationUnit(methodSymbols, assemblyAttribute, methodAttribute).NormalizeWhitespace();
            SyntaxTree syntaxTree = SF.SyntaxTree(compilationUnit, encoding: Encoding.Unicode);
            ctx.AddSource("SyntaxTreeTest.g.cs", syntaxTree.GetText());
        });
    }

    private static bool TryGetAttributeValue<T>(ISymbol symbol, INamedTypeSymbol attributeTypeSymbol, string argumentName, [NotNullWhen(true)] out T? value)
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

    private static CompilationUnitSyntax CreateCompilationUnit(IEnumerable<IMethodSymbol> methodSymbols, INamedTypeSymbol assemblyAttribute, INamedTypeSymbol methodAttribute)
    {
        // Class access modifiers
        SyntaxTokenList classAccessModifierSyntax = SF.TokenList(
            SF.Token(SyntaxKind.PublicKeyword),  // Todo: Can the class be private? Check if Lua can call it if private.
            SF.Token(SyntaxKind.StaticKeyword),
            SF.Token(SyntaxKind.UnsafeKeyword));

        // Class
        ClassDeclarationSyntax classDeclaration = SF.ClassDeclaration("DemoClass")
            .WithModifiers(classAccessModifierSyntax)
            .WithMembers(SF.List<MemberDeclarationSyntax>([
                GenerateLuaOpenMethod(methodSymbols, assemblyAttribute, methodAttribute),
                .. methodSymbols.Select(GenerateFunctionMethod)]));

        // Namespace
        NamespaceDeclarationSyntax namespaceDeclaration = SF.NamespaceDeclaration(SF.IdentifierName("Abc.Def"))
            .AddMembers(classDeclaration);

        return SF.CompilationUnit()
            .WithMembers(SF.SingletonList<MemberDeclarationSyntax>(namespaceDeclaration));
    }

    private static MethodDeclarationSyntax GenerateLuaOpenMethod(IEnumerable<IMethodSymbol> methodSymbols, INamedTypeSymbol assemblyAttribute, INamedTypeSymbol methodAttribute)
    {
        // Parameters
        SeparatedSyntaxList<ParameterSyntax> parameterSyntaxList = SF.SeparatedList([
            SF.Parameter(SF.Identifier("luaState")).WithType(SF.IdentifierName(_nintFullName))]);

        // Method invocation, Lua.CreateTable
        ExpressionStatementSyntax createTableMethodInvocation = SF.ExpressionStatement(SF.InvocationExpression(
            SF.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SF.IdentifierName(_luaInteropTypeFullName),
                SF.IdentifierName("CreateTable")),
            SF.ArgumentList([
                SF.Argument(SF.IdentifierName("luaState")),
                SF.Argument(SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.Literal(0))),
                SF.Argument(SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.Literal(methodSymbols.Count())))])));

        // Return statement
        ReturnStatementSyntax returnStatement = SF.ReturnStatement(SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.Literal(1)))
            .WithTrailingTrivia(SF.Comment("// Stack index of table"));

        // Method statements
        SyntaxList<StatementSyntax> statementList = SF.List<StatementSyntax>([
            createTableMethodInvocation,
            .. methodSymbols.Select(x => GenerateRegisterFunctionInvocation(x, methodAttribute)),
            returnStatement]);

        // Attribute, UnmanagedCallersOnly
        AttributeSyntax unmanagedCallersOnlyAttribute = SF.Attribute(
            SF.IdentifierName(_unmanagedCallersOnlyAttributeFullName),
            SF.AttributeArgumentList([
                SF.AttributeArgument(
                    SF.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SF.Literal("luaopen_luainteropdemo"))) // Todo: Use assembly name.
                .WithNameEquals(
                    SF.NameEquals(
                        SF.IdentifierName(_unmanagedCallersOnlyAttributeEntryPointArgument)))]));

        // Method
        MethodDeclarationSyntax methodDeclaration = SF.MethodDeclaration(
            SF.PredefinedType(SF.Token(SyntaxKind.IntKeyword)),
            SF.Identifier("LuaOpen"))
                .WithModifiers(SF.TokenList(
                    SF.Token(SyntaxKind.PublicKeyword), // Todo: Can the luaopen method be private? Check if Lua can call it if private.
                    SF.Token(SyntaxKind.StaticKeyword)))
                .WithParameterList(SF.ParameterList(parameterSyntaxList))
                .WithBody(SF.Block(statementList))
                .WithAttributeLists([
                    SF.AttributeList([unmanagedCallersOnlyAttribute])]);

        return methodDeclaration;
    }

    private static ExpressionStatementSyntax GenerateRegisterFunctionInvocation(IMethodSymbol methodSymbol, INamedTypeSymbol methodAttribute)
    {
        string functionName = TryGetAttributeValue(methodSymbol, methodAttribute, _luaFunctionAttributeNameArgumentName, out string? customFunctionName)
            ? customFunctionName
            : methodSymbol.Name;

        return SF.ExpressionStatement(SF.InvocationExpression(
            SF.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SF.IdentifierName(_luaInteropHelperTypeFullName),
                SF.IdentifierName("RegisterFunction")),
            SF.ArgumentList([
                SF.Argument(SF.IdentifierName("luaState")),
                SF.Argument(SF.LiteralExpression(SyntaxKind.StringLiteralExpression, SF.Literal(functionName))),
                SF.Argument(SF.PrefixUnaryExpression(SyntaxKind.AddressOfExpression, SF.IdentifierName(methodSymbol.Name)))])));
    }

    private static MethodDeclarationSyntax GenerateFunctionMethod(IMethodSymbol methodSymbol)
    {
        string containingTypeFullName = methodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        IEnumerable<(LocalDeclarationStatementSyntax StatementSyntax, string ArgumentName)> argumentReads = methodSymbol.Parameters.Select(GenerateParameterRead);

        // Parameters
        SeparatedSyntaxList<ParameterSyntax> parameterSyntaxList = SF.SeparatedList([
            SF.Parameter(SF.Identifier("luaState")).WithType(SF.IdentifierName(_nintFullName))]);

        // Attribute, UnmanagedCallersOnly
        AttributeSyntax unmanagedCallersOnlyAttribute = SF.Attribute(SF.IdentifierName(_unmanagedCallersOnlyAttributeFullName));

        // Method invocation, RegisterFunction
        ExpressionStatementSyntax consoleWriteLineStatement = SF.ExpressionStatement(SF.InvocationExpression(
            SF.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SF.IdentifierName(containingTypeFullName),
                SF.IdentifierName(methodSymbol.Name)),
            SF.ArgumentList([
                .. argumentReads.Select(x => SF.Argument(SF.IdentifierName(x.ArgumentName)))])));

        // Method invocation arguments, read argument
        ArgumentListSyntax parameterReadArguments = SF.ArgumentList([
            SF.Argument(SF.IdentifierName("luaState")),
            SF.Argument(
                SF.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                SF.Literal(1)))]);

        // Method invocation, read argument
        LocalDeclarationStatementSyntax parameterReadStatement = SF.LocalDeclarationStatement(
            SF.VariableDeclaration(
                SF.PredefinedType(
                    SF.Token(SyntaxKind.StringKeyword)))
            .WithVariables(
                SF.SingletonSeparatedList(
                SF.VariableDeclarator(
                    SF.Identifier("arg1"))
            .WithInitializer(
                SF.EqualsValueClause(
                    SF.InvocationExpression(
                        SF.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SF.IdentifierName(_luaInteropHelperTypeFullName),
                            SF.IdentifierName("ReadStringArg")))
                    .WithArgumentList(parameterReadArguments))))));

        // Return statement
        ReturnStatementSyntax returnStatement = SF.ReturnStatement(
            SF.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                SF.Literal(0)))
            .WithTrailingTrivia(SF.Comment("// Number of pushed values"));

        // Method statements
        SyntaxList<StatementSyntax> statementList = SF.List<StatementSyntax>([
            .. argumentReads.Select(x => x.StatementSyntax),
            consoleWriteLineStatement,
            returnStatement]);

        // Method
        MethodDeclarationSyntax methodDeclaration = SF.MethodDeclaration(
            SF.PredefinedType(SF.Token(SyntaxKind.IntKeyword)),
            SF.Identifier(methodSymbol.Name))
                .WithModifiers(SF.TokenList(
                    SF.Token(SyntaxKind.PrivateKeyword),
                    SF.Token(SyntaxKind.StaticKeyword)))
                .WithParameterList(SF.ParameterList(parameterSyntaxList))
                .WithBody(SF.Block(statementList))
                .WithAttributeLists([
                    SF.AttributeList([unmanagedCallersOnlyAttribute])]);

        return methodDeclaration;
    }

    private static (LocalDeclarationStatementSyntax statementSyntax, string argumentName) GenerateParameterRead(IParameterSymbol parameter, int index)
    {
        int luaIndex = index + 1;
        string argumentName = $"arg{luaIndex}";
        string fullTypeName = parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        // Method invocation arguments, read argument
        ArgumentListSyntax parameterReadArguments = SF.ArgumentList([
            SF.Argument(SF.IdentifierName("luaState")),
            SF.Argument(
                SF.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                SF.Literal(luaIndex)))]);

        // Method invocation, read argument
        LocalDeclarationStatementSyntax parameterReadStatement = SF.LocalDeclarationStatement(
            SF.VariableDeclaration(
                SF.IdentifierName(fullTypeName))
            .WithVariables(
                SF.SingletonSeparatedList(
                SF.VariableDeclarator(
                    SF.Identifier(argumentName))
            .WithInitializer(
                SF.EqualsValueClause(
                    SF.InvocationExpression(
                        SF.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SF.IdentifierName(_luaInteropHelperTypeFullName),
                            SF.IdentifierName(GetReadMethodName(parameter.Type))))
                    .WithArgumentList(parameterReadArguments))))))
            .WithTrailingTrivia(SF.Comment($"// {parameter.Name}"));

        return (parameterReadStatement, argumentName);
    }

    private static string GetReadMethodName(ITypeSymbol typeSymbol)
    {
        return typeSymbol switch
        {
            _ when typeSymbol.SpecialType == SpecialType.System_String => "ReadStringArg",
            _ when typeSymbol.SpecialType == SpecialType.System_Double => "ReadNumberArg",
            _ when typeSymbol.SpecialType == SpecialType.System_Int32 => "ReadIntegerArg",
            _ when typeSymbol.SpecialType == SpecialType.System_Int64 => "ReadIntegerArg",
            _ => "" // Todo: Handle unmappable types
        };
    }

    private record struct CompilationData(IAssemblySymbol Assembly, INamedTypeSymbol AssemblyAttribute, INamedTypeSymbol MethodAttribute);
}
