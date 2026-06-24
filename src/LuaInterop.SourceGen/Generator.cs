using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace LuaInterop.SourceGen;

[Generator(LanguageNames.CSharp)]
internal class Generator : IIncrementalGenerator
{
    private const string _luaOpenAttributeFullName = "LuaInterop.Attributes.LuaOpenAttribute";
    private const string _luaFunctionAttributeFullName = "LuaInterop.Attributes.LuaFunctionAttribute";
    private const string _luaFunctionAttributeNameArgumentName = "FunctionName";

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
        IncrementalValuesProvider<ISymbol> methodProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            _luaFunctionAttributeFullName,
            static (syntaxNode, _) => syntaxNode is MethodDeclarationSyntax,
            static (syntaxContext, _) => syntaxContext.TargetSymbol);

        // Group methods and pair them with the assembly.
        IncrementalValueProvider<(ImmutableArray<ISymbol> methods, CompilationData compilationData)> provider = methodProvider.Collect().Combine(compilationDataProvider);

        context.RegisterSourceOutput(provider, (ctx, data) =>
        {
            // Deconstruct.
            (ImmutableArray<ISymbol> symbols, CompilationData compilationData) = data;
            (IAssemblySymbol assembly, INamedTypeSymbol assemblyAttribute, INamedTypeSymbol methodAttribute) = compilationData;

            if (compilationData == default)
            {
                return;
            }

            AttributeData? matchingAttribute = GetAttributeData(assembly, assemblyAttribute);
            if (matchingAttribute == null)
            {
                return;
            }

            string str = TryGetAttributeNamedArgument(matchingAttribute, "Number", out int value) ? value.ToString() : "FAILED";

            string abc(ISymbol symbol)
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
                {{string.Join("\r\n", symbols.Select(x => abc(x)))}}
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
        });
    }

    private static bool TryGetAttributeValue<T>(ISymbol symbol, INamedTypeSymbol attributeTypeSymbol, string argumentName, out T? value)
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

    private record struct CompilationData(IAssemblySymbol Assembly, INamedTypeSymbol AssemblyAttribute, INamedTypeSymbol MethodAttribute);
}
