using static LuaInterop.SourceGen.GeneratorHelper;

namespace LuaInterop.SourceGen;

internal static class ModuleInitializerBuilder
{
    public static CompilationUnitSyntax GenerateModuleInitializer()
    {
        AttributeSyntax moduleInitializerAttribute = SF.Attribute(
            SF.IdentifierName(GeneratorConstants.ModuleInitializerAttributeAttributeGlobalFullName));

        // Method invocation, LuaModuleInitializer.Initialize
        ExpressionStatementSyntax moduleInitializeMethodInvocation = SF.ExpressionStatement(
            SF.InvocationExpression(
                SF.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SF.IdentifierName(GeneratorConstants.ModuleInitializerHelperTypeGlobalFullName),
                    SF.IdentifierName(GeneratorConstants.ModuleInitializerHelperMethodName))));

        // Method, module initializer
        MethodDeclarationSyntax methodDeclaration = SF.MethodDeclaration(
                SF.PredefinedType(SF.Token(SyntaxKind.VoidKeyword)),
                SF.Identifier(GeneratorConstants.ModuleInitializer))
            .WithModifiers([
                SF.Token(SyntaxKind.InternalKeyword),
                SF.Token(SyntaxKind.StaticKeyword)])
            .WithBody(SF.Block(moduleInitializeMethodInvocation))
            .WithAttributeLists([
                SF.AttributeList([moduleInitializerAttribute])])
            .AddXmlDocumentation("Entry point for Lua, exposes available functions.");

        // Class
        ClassDeclarationSyntax classDeclaration = SF.ClassDeclaration($"{GeneratorConstants.ModuleInitializerClassName}")
            .WithModifiers([
                SF.Token(SyntaxKind.InternalKeyword),
                SF.Token(SyntaxKind.StaticKeyword)
            ])
            .WithMembers([methodDeclaration])
            .WithAttributeLists([
                SF.AttributeList([GenerateGeneratedCodeAttributeAttribute()])])
            .AddXmlDocumentation("Defines module initializer logic to perform setup logic.");

        // Namespace
        NamespaceDeclarationSyntax namespaceDeclaration = SF.NamespaceDeclaration(
                SF.IdentifierName(GeneratorConstants.GeneratedCodeNamespace))
            .AddMembers(classDeclaration);

        return SF.CompilationUnit()
            .WithMembers([namespaceDeclaration]);
    }
}
