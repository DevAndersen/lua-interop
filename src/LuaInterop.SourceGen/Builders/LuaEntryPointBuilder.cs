using static LuaInterop.SourceGen.GeneratorHelper;

namespace LuaInterop.SourceGen.Builders;

internal static class LuaEntryPointBuilder
{
    public static CompilationUnitSyntax CreateCompilationUnit(
        string assemblyName,
        (IMethodSymbol MethodSymbol, string FunctionName, string MethodName, bool IsManualMethod)[] validateMethods,
        TypeDictionary typeDictionary)
    {
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
                GenerateLuaOpenMethod(assemblyName, validateMethods, typeDictionary),
                .. LuaFunctionBuilder.GenerateFunctionMethods(validatedAutomaticMethods, typeDictionary)])
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
        (IMethodSymbol MethodSymbol, string FunctionName, string MethodName, bool IsManualMethod)[] methodSymbols,
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
            .. methodSymbols.Select(x => GenerateRegisterFunctionInvocation(x.FunctionName, x.MethodName)),
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

    private static ExpressionStatementSyntax GenerateRegisterFunctionInvocation(string functionName, string methodName)
    {
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
}
