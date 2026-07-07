using static LuaInterop.SourceGen.GeneratorHelper;

namespace LuaInterop.SourceGen;

internal static class LuaFunctionGenerator
{
    public static IEnumerable<MethodDeclarationSyntax> GenerateFunctionMethods(IMethodSymbol[] methods, TypeDictionary typeDictionary, SourceProductionContext context)
    {
        return methods
            .Where(x => FilterMethodSymbol(x, context, typeDictionary))
            .Select(y => GenerateFunctionMethod(y, typeDictionary));
    }

    public static MethodDeclarationSyntax GenerateFunctionMethod(IMethodSymbol methodSymbol, TypeDictionary typeDictionary)
    {
        string containingTypeFullName = methodSymbol.ContainingType.GetFullName();
        (LocalDeclarationStatementSyntax StatementSyntax, string ArgumentName)[] argumentReads = methodSymbol.Parameters.Select(GenerateParameterRead).ToArray();

        // Parameters
        SeparatedSyntaxList<ParameterSyntax> parameterSyntaxList = SF.SeparatedList([
            SF.Parameter(
                SF.Identifier(GeneratorConstants.LuaStateVariableName))
            .WithType(
                SF.IdentifierName(typeDictionary.GetNameOrThrow(TypeDictionaryId.IntPtr)))]);

        // Attribute, UnmanagedCallersOnly
        AttributeSyntax unmanagedCallersOnlyAttribute = SF.Attribute(SF.IdentifierName(GeneratorConstants.UnmanagedCallersOnlyAttributeFullName));

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
                    SF.IdentifierName(methodSymbol.ReturnType.GetFullName()))
                .WithVariables(
                    SF.SingletonSeparatedList(
                    SF.VariableDeclarator(
                        SF.Identifier(GeneratorConstants.ReturnVariableName))
                .WithInitializer(
                    SF.EqualsValueClause(wrappedMethodInvocation)))));
        }

        // Return statement
        ReturnStatementSyntax returnStatement = SF.ReturnStatement(
            GenerateValuePushInvocation(methodSymbol, typeDictionary))
            .AddComment(methodSymbol.ReturnsVoid ? "Void method, no values to be pushed" : "Push number of values");

        // Method statements
        StatementSyntax?[] statements = [
            .. argumentReads.Select(x => x.StatementSyntax),
            methodInvocation,
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
        int luaIndex = index + 1;
        string argumentName = $"arg{luaIndex}";
        string fullTypeName = parameter.Type.GetFullName();

        string readMethodName = GetReadMethodName(parameter.Type)
            ?? throw new Exception($"LuaInterop failed, {nameof(GetReadMethodName)} returned null for argument '{argumentName}'"); // Should never happen, check performed earlier.;

        // Method invocation arguments, read argument
        ArgumentListSyntax parameterReadArguments = SF.ArgumentList([
            SF.Argument(SF.IdentifierName(GeneratorConstants.LuaStateVariableName)),
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
                            SF.IdentifierName(GeneratorConstants.LuaReadHelperTypeFullName),
                            SF.IdentifierName(readMethodName)))
                    .WithArgumentList(parameterReadArguments))))))
            .AddComment($"Parameter \"{parameter.Name}\"");

        return (parameterReadStatement, argumentName);
    }

    private static ExpressionSyntax GenerateValuePushInvocation(IMethodSymbol methodSymbol, TypeDictionary typeDictionary)
    {
        if (methodSymbol.ReturnsVoid)
        {
            // Literal expression, 0
            return SF.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                SF.Literal(0));
        }

        string pushMethodName = GetPushMethodName(methodSymbol.ReturnType, typeDictionary)
            ?? throw new Exception($"LuaInterop failed, {nameof(GetPushMethodName)} returned null for method '{methodSymbol.GetFullName()}'"); // Should never happen, check performed earlier.

        // Invocation expression, push method
        return SF.InvocationExpression(
                    SF.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SF.IdentifierName(GeneratorConstants.LuaPushHelperTypeFullName),
                        SF.IdentifierName(pushMethodName)),
                    SF.ArgumentList([
                        SF.Argument(SF.IdentifierName(GeneratorConstants.LuaStateVariableName)),
                        SF.Argument(SF.IdentifierName(GeneratorConstants.ReturnVariableName))]));
    }

    private static string? GetReadMethodName(ITypeSymbol typeSymbol)
    {
        // Check for nullable value types.
        if (typeSymbol is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T, TypeArguments: [ITypeSymbol argumentType] })
        {
            if (argumentType.SpecialType == SpecialType.System_Boolean)
            {
                return "ReadNullableBoolean";
            }

            // Todo: If the nested call returns null, return null, in order to avoid returning broken method names for unmappable types.
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
            _ => null
        };
    }

    private static string? GetPushMethodName(ITypeSymbol typeSymbol, TypeDictionary typeDictionary)
    {
        // Support nullable parameters.
        if (typeSymbol is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T, TypeArguments: [ITypeSymbol argumentType] })
        {
            return GetPushMethodName(argumentType, typeDictionary);
        }

        // Support dictionaries.
        INamedTypeSymbol dictionary2TypeSymbol = typeDictionary.GetOrThrow(TypeDictionaryId.Dictionary2);
        if (typeSymbol.AllInterfaces.Select(x => x.OriginalDefinition).Contains(dictionary2TypeSymbol, SymbolEqualityComparer.Default))
        {
            // Todo: Validate type arguments.
            return "PushDictionary";
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
            _ => null
        };
    }

    private static bool FilterMethodSymbol(IMethodSymbol methodSymbol, SourceProductionContext context, TypeDictionary typeDictionary)
    {
        // Disallow instanced methods.
        if (!methodSymbol.IsStatic)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.MethodNotStatic,
                methodSymbol.Locations.FirstOrDefault(),
                methodSymbol.Locations));

            return false;
        }

        // Disallow unreachable methods.
        if (methodSymbol.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.MethodNotAccessible,
                methodSymbol.Locations.FirstOrDefault(),
                methodSymbol.Locations));

            return false;
        }

        // Disallow unsupported return types.
        if (IsReturnTypeUnsupported(methodSymbol, typeDictionary))
        {
            INamedTypeSymbol v1 = typeDictionary.GetOrThrow(TypeDictionaryId.Dictionary2);
            INamedTypeSymbol v2 = methodSymbol.ReturnType.OriginalDefinition as INamedTypeSymbol ?? throw new Exception();

            bool b = methodSymbol.ReturnType.AllInterfaces.Select(x => x.OriginalDefinition).Contains(v1, SymbolEqualityComparer.Default);

            var v3 = methodSymbol.ReturnType.OriginalDefinition.Equals(v1.OriginalDefinition, SymbolEqualityComparer.Default);

            if (methodSymbol.ReturnType.OriginalDefinition.Equals(v1, SymbolEqualityComparer.Default))
            {

            }

            if (methodSymbol.ReturnType is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T, TypeArguments: [ITypeSymbol argumentType] })
            {
            }

            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.ReturnTypeNotSupported,
                methodSymbol.Locations.FirstOrDefault(),
                methodSymbol.Locations,
                methodSymbol.ReturnType.Name));

            return false;
        }

        // Disallow inaccessible containing types.
        if (AreContainingTypesInaccessible(methodSymbol.ContainingType, out ITypeSymbol? problematicTypeSymbol))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.ContainingTypeNotAccessible,
                methodSymbol.Locations.FirstOrDefault(),
                methodSymbol.Locations,
                problematicTypeSymbol.Name));

            return false;
        }

        // Disallow unsupported parameters.
        foreach (IParameterSymbol parameter in methodSymbol.Parameters)
        {
            if (IsParameterUnsupported(parameter))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.ParameterTypeNotSupported,
                    methodSymbol.Locations.FirstOrDefault(),
                    methodSymbol.Locations,
                    parameter.Type.Name));

                return false;
            }
        }

        // Todo: Disallow methods with the same names/function names.

        return true;
    }

    private static bool IsReturnTypeUnsupported(IMethodSymbol methodSymbol, TypeDictionary typeDictionary)
    {
        return !methodSymbol.ReturnsVoid
            && GetPushMethodName(methodSymbol.ReturnType, typeDictionary) == null;
    }

    private static bool IsParameterUnsupported(IParameterSymbol parameterSymbol)
    {
        return GetReadMethodName(parameterSymbol.Type) == null;
    }
}
