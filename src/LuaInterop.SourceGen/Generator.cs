using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace LuaInterop.SourceGen;

[Generator(LanguageNames.CSharp)]
internal class Generator : IIncrementalGenerator
{
    private const string _luaOpenAttributeFullName = "LuaInterop.Attributes.LuaOpenAttribute";
    private const string _luaFunctionAttributeFullName = "LuaInterop.Attributes.LuaFunctionAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Check for assembly attribute.
        IncrementalValueProvider<(IAssemblySymbol, INamedTypeSymbol)> isMarkedAssembly = context.CompilationProvider.Select((compilation, _) =>
        {
            INamedTypeSymbol? attributeTypeSymbol = compilation.GetTypeByMetadataName(_luaOpenAttributeFullName);

            if (attributeTypeSymbol == null)
            {
                return default;
            }

            return (compilation.Assembly, attributeTypeSymbol);
        });

        // Check for method attributes.
        IncrementalValuesProvider<ISymbol> methods = context.SyntaxProvider.ForAttributeWithMetadataName(
            _luaFunctionAttributeFullName,
            static (syntaxNode, _) => syntaxNode is MethodDeclarationSyntax,
            static (syntaxContext, _) => syntaxContext.TargetSymbol);

        // Group methods and pair them with the assembly.
        IncrementalValueProvider<(ImmutableArray<ISymbol> methods, (IAssemblySymbol, INamedTypeSymbol) Right)> provider = methods.Collect().Combine(isMarkedAssembly);

        context.RegisterSourceOutput(provider, static (ctx, compilation) =>
        {
            (ImmutableArray<ISymbol> symbols, (IAssemblySymbol Assembly, INamedTypeSymbol Attribute) assemblyData) = compilation;
            if (assemblyData.Assembly == null)
            {
                return;
            }

            var matchingAttribute = GetAttributeData(assemblyData.Assembly, assemblyData.Attribute);
            if (matchingAttribute == null)
            {
                return;
            }

            string str = TryGetAttributeNamedArgument(matchingAttribute, "Number", out int value) ? value.ToString() : "FAILED";

            // language=c#
            string src = $$"""
                namespace Demo.Marker.{{assemblyData.Assembly.Name}};

                /*
                > {{assemblyData.Attribute.Name}} : {{str}}
                > {{assemblyData.Assembly.Name}}
                */

                /*
                {{string.Join("\r\n", symbols.Select(x => x.Name))}}
                */

                public static class Generated2
                {
                    public static void SayHello()
                    {
                        global::System.Console.WriteLine("Hello, World!");
                    }
                }
                """;

            ctx.AddSource($"{assemblyData.Assembly.Name}.Test2.g.cs", src);
        });
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
}
