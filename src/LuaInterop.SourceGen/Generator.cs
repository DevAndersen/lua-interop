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
    private const string _generatedCodeNamespace = "LuaInterop.Generated";
    private const string _luaOpenAttributeFullName = "LuaInterop.Attributes.LuaOpenAttribute";
    private const string _luaFunctionAttributeFullName = "LuaInterop.Attributes.LuaFunctionAttribute";
    private const string _luaFunctionAttributeNameArgumentName = "FunctionName";
    private const string _luaInteropHelperTypeFullName = "global::LuaInterop.LuaInteropHelper";
    private const string _luaReadHelperTypeFullName = "global::LuaInterop.LuaReadHelper";
    private const string _luaPushHelperTypeFullName = "global::LuaInterop.LuaPushHelper";
    private const string _unmanagedCallersOnlyAttributeFullName = "global::System.Runtime.InteropServices.UnmanagedCallersOnly";
    private const string _generatedCodeAttributeAttributeFullName = "global::System.CodeDom.Compiler.GeneratedCodeAttribute";
    private const string _unmanagedCallersOnlyAttributeEntryPointArgument = "EntryPoint";
    private const string _nintFullName = "global::System.IntPtr";
    private const string _returnVariableName = "returnedValue";
    private const string _luaOpenClassName = "LuaEntryPoint";
    private const string _luaStateVariableName = "luaState";

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

            return new CompilationData(compilation.Assembly, assemblyAttributeTypeSymbol, methodAttributeTypeSymbol, compilation.AssemblyName);
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
            (IAssemblySymbol assembly, INamedTypeSymbol assemblyAttribute, INamedTypeSymbol methodAttribute, string? assemblyName) = compilationData;

            IMethodSymbol[] methodSymbols = nullableMethodSymbols.OfType<IMethodSymbol>().ToArray();

            if (compilationData == default)
            {
                return;
            }

            AttributeData? matchingAttribute = GetAttributeData(assembly, assemblyAttribute);
            if (matchingAttribute == null)
            {
                return;
            }

            if (IsNullOrWhiteSpace(assemblyName))
            {
                return;
            }

            // Todo: Validate assembly name (Lua appears to require all lower-case?)

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

                if (!IsAllowedReturnType(methodSymbol))
                {
                    // Todo: Disallow methods with unmappable return types.
                }

                // Todo: Disallow methods with the same names/function names.
            }

            CompilationUnitSyntax compilationUnit = CreateCompilationUnit(methodSymbols, methodAttribute, assemblyName).NormalizeWhitespace();
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

    private static CompilationUnitSyntax CreateCompilationUnit(IMethodSymbol[] methodSymbols, INamedTypeSymbol methodAttribute, string assemblyName)
    {
        // Class access modifiers
        SyntaxTokenList classAccessModifierSyntax = SF.TokenList(
            SF.Token(SyntaxKind.PublicKeyword),  // Todo: Can the class be private? Check if Lua can call it if private.
            SF.Token(SyntaxKind.StaticKeyword));

        // Class
        ClassDeclarationSyntax classDeclaration = SF.ClassDeclaration(_luaOpenClassName)
            .WithModifiers(classAccessModifierSyntax)
            .WithMembers(SF.List<MemberDeclarationSyntax>([
                GenerateLuaOpenMethod(methodSymbols, methodAttribute, assemblyName),
                .. methodSymbols.Select(GenerateFunctionMethod)]))
                .WithAttributeLists([
                    SF.AttributeList([GenerateGeneratedCodeAttributeAttribute()])])
                .WithLeadingTrivia(GenerateXmlSummary("Contains Lua interoperability logic."));

        // Namespace
        NamespaceDeclarationSyntax namespaceDeclaration = SF.NamespaceDeclaration(SF.IdentifierName(_generatedCodeNamespace))
            .AddMembers(classDeclaration);

        return SF.CompilationUnit()
            .WithMembers(SF.SingletonList<MemberDeclarationSyntax>(namespaceDeclaration));
    }

    private static MethodDeclarationSyntax GenerateLuaOpenMethod(IMethodSymbol[] methodSymbols, INamedTypeSymbol methodAttribute, string assemblyName)
    {
        // Parameters
        SeparatedSyntaxList<ParameterSyntax> parameterSyntaxList = SF.SeparatedList([
            SF.Parameter(SF.Identifier(_luaStateVariableName)).WithType(SF.IdentifierName(_nintFullName))]);

        // Method invocation, Lua.CreateTable
        ExpressionStatementSyntax createTableMethodInvocation = SF.ExpressionStatement(SF.InvocationExpression(
            SF.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SF.IdentifierName(_luaInteropHelperTypeFullName),
                SF.IdentifierName("CreateTable")),
            SF.ArgumentList([
                SF.Argument(SF.IdentifierName(_luaStateVariableName)),
                SF.Argument(SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.Literal(methodSymbols.Length)))])));

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
                        SF.Literal($"luaopen_{assemblyName}")))
                .WithNameEquals(
                    SF.NameEquals(
                        SF.IdentifierName(_unmanagedCallersOnlyAttributeEntryPointArgument)))]));

        // Method
        MethodDeclarationSyntax methodDeclaration = SF.MethodDeclaration(
            SF.PredefinedType(SF.Token(SyntaxKind.IntKeyword)),
            SF.Identifier("LuaOpen"))
                .WithModifiers(SF.TokenList(
                    SF.Token(SyntaxKind.PublicKeyword), // Todo: Can the luaopen method be private? Check if Lua can call it if private.
                    SF.Token(SyntaxKind.StaticKeyword),
                    SF.Token(SyntaxKind.UnsafeKeyword)))
                .WithParameterList(SF.ParameterList(parameterSyntaxList))
                .WithBody(SF.Block(statementList))
                .WithAttributeLists([
                    SF.AttributeList([unmanagedCallersOnlyAttribute])])
                .WithLeadingTrivia(GenerateXmlSummary("Entry point for Lua, exposes available functions."));

        return methodDeclaration;
    }

    private static ExpressionStatementSyntax GenerateRegisterFunctionInvocation(IMethodSymbol methodSymbol, INamedTypeSymbol methodAttribute)
    {
        // Determine function name
        string functionName = TryGetAttributeValue(methodSymbol, methodAttribute, _luaFunctionAttributeNameArgumentName, out string? customFunctionName)
            ? customFunctionName
            : methodSymbol.Name;

        // Method invocation
        return SF.ExpressionStatement(SF.InvocationExpression(
            SF.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SF.IdentifierName(_luaInteropHelperTypeFullName),
                SF.IdentifierName("RegisterFunction")),
            SF.ArgumentList([
                SF.Argument(SF.IdentifierName(_luaStateVariableName)),
                SF.Argument(SF.LiteralExpression(SyntaxKind.StringLiteralExpression, SF.Literal(functionName))),
                SF.Argument(SF.PrefixUnaryExpression(SyntaxKind.AddressOfExpression, SF.IdentifierName(methodSymbol.Name)))])));
    }

    private static MethodDeclarationSyntax GenerateFunctionMethod(IMethodSymbol methodSymbol)
    {
        string containingTypeFullName = methodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        (LocalDeclarationStatementSyntax StatementSyntax, string ArgumentName)[] argumentReads = methodSymbol.Parameters.Select(GenerateParameterRead).ToArray();
        ExpressionStatementSyntax? pushStatement = GenerateValuePushInvocation(methodSymbol);

        // Parameters
        SeparatedSyntaxList<ParameterSyntax> parameterSyntaxList = SF.SeparatedList([
            SF.Parameter(SF.Identifier(_luaStateVariableName)).WithType(SF.IdentifierName(_nintFullName))]);

        // Attribute, UnmanagedCallersOnly
        AttributeSyntax unmanagedCallersOnlyAttribute = SF.Attribute(SF.IdentifierName(_unmanagedCallersOnlyAttributeFullName));

        // Method invocation
        InvocationExpressionSyntax wrappedMethodInvocation = SF.InvocationExpression(
            SF.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SF.IdentifierName(containingTypeFullName),
                SF.IdentifierName(methodSymbol.Name)),
            SF.ArgumentList([
                .. argumentReads.Select(x => SF.Argument(SF.IdentifierName(x.ArgumentName)))]));

        StatementSyntax methodInvocation;
        if (methodSymbol.ReturnsVoid)
        {
            // Wrapped void method invocation statement
            methodInvocation = SF.ExpressionStatement(wrappedMethodInvocation);
        }
        else
        {
            // Wrapped void method invocation statement with return variable
            methodInvocation = SF.LocalDeclarationStatement(
                SF.VariableDeclaration(
                    SF.IdentifierName(methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))
                .WithVariables(
                    SF.SingletonSeparatedList(
                    SF.VariableDeclarator(
                        SF.Identifier(_returnVariableName))
                .WithInitializer(
                    SF.EqualsValueClause(wrappedMethodInvocation)))));
        }

        // Return statement
        ReturnStatementSyntax returnStatement = SF.ReturnStatement(
            SF.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                SF.Literal(methodSymbol.ReturnsVoid ? 0 : 1)))
            .WithTrailingTrivia(SF.Comment("// Number of pushed values"));

        // Method statements
        StatementSyntax?[] statements = [
            .. argumentReads.Select(x => x.StatementSyntax),
            methodInvocation,
            pushStatement,
            returnStatement
        ];

        // Method
        MethodDeclarationSyntax methodDeclaration = SF.MethodDeclaration(
            SF.PredefinedType(SF.Token(SyntaxKind.IntKeyword)),
            SF.Identifier(methodSymbol.Name))
                .WithModifiers(SF.TokenList(
                    SF.Token(SyntaxKind.PrivateKeyword),
                    SF.Token(SyntaxKind.StaticKeyword)))
                .WithParameterList(SF.ParameterList(parameterSyntaxList))
                .WithBody(SF.Block(SF.List(statements.OfType<StatementSyntax>())))
                .WithAttributeLists([
                    SF.AttributeList([unmanagedCallersOnlyAttribute])]);

        return methodDeclaration;
    }

    private static (LocalDeclarationStatementSyntax statementSyntax, string argumentName) GenerateParameterRead(IParameterSymbol parameter, int index)
    {
        // Todo: Support nullable parameters
        int luaIndex = index + 1;
        string argumentName = $"arg{luaIndex}";
        string fullTypeName = parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        // Method invocation arguments, read argument
        ArgumentListSyntax parameterReadArguments = SF.ArgumentList([
            SF.Argument(SF.IdentifierName(_luaStateVariableName)),
            SF.Argument(
                SF.LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    SF.Literal(luaIndex))),
            SF.Argument(
                SF.LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    SF.Literal(parameter.Name)))]);

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
                            SF.IdentifierName(_luaReadHelperTypeFullName),
                            SF.IdentifierName(GetReadMethodName(parameter.Type))))
                    .WithArgumentList(parameterReadArguments))))))
            .WithTrailingTrivia(SF.Comment($"// Parameter \"{parameter.Name}\""));

        return (parameterReadStatement, argumentName);
    }

    private static ExpressionStatementSyntax? GenerateValuePushInvocation(IMethodSymbol methodSymbol)
    {
        if (methodSymbol.ReturnsVoid)
        {
            return null;
        }

        string pushMethodName = GetPushMethodName(methodSymbol.ReturnType);

        return SF.ExpressionStatement(SF.InvocationExpression(
            SF.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SF.IdentifierName(_luaPushHelperTypeFullName),
                SF.IdentifierName(pushMethodName)),
            SF.ArgumentList([
                SF.Argument(SF.IdentifierName(_luaStateVariableName)),
                SF.Argument(SF.IdentifierName(_returnVariableName))])));
    }

    private static AttributeSyntax GenerateGeneratedCodeAttributeAttribute()
    {
        string? name = typeof(Generator).Assembly.GetName().Name;
        string version = typeof(Generator).Assembly.GetName().Version.ToString();

        // Attribute, GeneratedCodeAttribute
        return SF.Attribute(
            SF.IdentifierName(_generatedCodeAttributeAttributeFullName),
            SF.AttributeArgumentList([
                SF.AttributeArgument(
                    SF.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SF.Literal(name))),
                SF.AttributeArgument(
                    SF.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SF.Literal(version)))]));
    }

    private static SyntaxTriviaList GenerateXmlSummary(string summary)
    {
        string[] lines = summary
            .Split(["\r\n", "\n"], StringSplitOptions.None)
            .Select(line => "/// " + line)
            .ToArray();

        return SF.ParseLeadingTrivia($"""
            /// <summary>
            {string.Join("\r\n", lines)}
            /// </summary>

            """);
    }

    private static string GetReadMethodName(ITypeSymbol typeSymbol)
    {
        // Check for nullable value types.
        if (typeSymbol is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T, TypeArguments: [ITypeSymbol argumentType] })
        {
            if (argumentType.SpecialType == SpecialType.System_Boolean)
            {
                return "ReadNullableBoolean";
            }
            return "TODO_NULLABLE_" + GetReadMethodName(argumentType); // Todo: Debug example
        }

        return typeSymbol switch
        {
            _ when typeSymbol.SpecialType == SpecialType.System_String => "ReadString",
            _ when typeSymbol.SpecialType == SpecialType.System_Double => "ReadDouble",
            _ when typeSymbol.SpecialType == SpecialType.System_Single => "ReadFloat",
            _ when typeSymbol.SpecialType == SpecialType.System_Byte => "ReadByte",
            _ when typeSymbol.SpecialType == SpecialType.System_Int16 => "ReadShort",
            _ when typeSymbol.SpecialType == SpecialType.System_Int32 => "ReadInt",
            _ when typeSymbol.SpecialType == SpecialType.System_Int64 => "ReadLong",
            _ when typeSymbol.SpecialType == SpecialType.System_Boolean => "ReadBoolean",
            _ => "" // Todo: Handle unmappable types
        };
    }

    private static string GetPushMethodName(ITypeSymbol typeSymbol)
    {
        // Support nullable parameters
        if (typeSymbol is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T, TypeArguments: [ITypeSymbol argumentType] })
        {
            return GetPushMethodName(argumentType);
        }

        return typeSymbol switch // Todo: Does this work correctly for nullable values?
        {
            _ when typeSymbol.SpecialType == SpecialType.System_String => "PushString",
            _ when typeSymbol.SpecialType == SpecialType.System_Double => "PushDouble",
            _ when typeSymbol.SpecialType == SpecialType.System_Single => "PushFloat",
            _ when typeSymbol.SpecialType == SpecialType.System_Byte => "PushByte",
            _ when typeSymbol.SpecialType == SpecialType.System_Int16 => "PushShort",
            _ when typeSymbol.SpecialType == SpecialType.System_Int32 => "PushInt",
            _ when typeSymbol.SpecialType == SpecialType.System_Int64 => "PushLong",
            _ when typeSymbol.SpecialType == SpecialType.System_Boolean => "PushBoolean",
            _ => "" // Todo: Handle unmappable types
        };
    }

    private static bool IsAllowedReturnType(IMethodSymbol methodSymbol)
    {
        ITypeSymbol returnType = methodSymbol.ReturnType;

        return methodSymbol.ReturnsVoid
            || returnType.SpecialType == SpecialType.System_String
            || returnType.SpecialType == SpecialType.System_Double
            || returnType.SpecialType == SpecialType.System_Single
            || returnType.SpecialType == SpecialType.System_Byte
            || returnType.SpecialType == SpecialType.System_Int16
            || returnType.SpecialType == SpecialType.System_Int32
            || returnType.SpecialType == SpecialType.System_Int64
            || returnType.SpecialType == SpecialType.System_Boolean;
    }

    /// <summary>
    /// Provides null-or-whitespace check with <see cref="NotNullWhenAttribute"/> for .NET Standard 2.0.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static bool IsNullOrWhiteSpace([NotNullWhen(false)] string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    private record struct CompilationData(
        IAssemblySymbol Assembly,
        INamedTypeSymbol AssemblyAttribute,
        INamedTypeSymbol MethodAttribute,
        string? AssemblyName);
}
