using Microsoft.CodeAnalysis;

namespace LuaInterop.SourceGen;

[Generator(LanguageNames.CSharp)]
internal class Generator : IIncrementalGenerator
{
    private const string _luaOpenAttributeFullName = "global::LuaInterop.Attributes.LuaOpenAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider, (ctx, compilation)=>
        {
            string? assemblyName = compilation.AssemblyName;
            if (string.IsNullOrWhiteSpace(assemblyName))
            {
                return;
            }

            if (!HasLuaOpenAttribute(compilation))
            {
                return;
            }

            // language=c#
            string src = $$"""
                namespace Demo.{{assemblyName}};

                public static class Generated
                {
                    public static void SayHello()
                    {
                        // {{assemblyName}}
                        global::System.Console.WriteLine("Hello, World!");
                    }
                }
                """;

            ctx.AddSource($"{assemblyName}.Test.g.cs", src);
        });
    }

    private static bool HasLuaOpenAttribute(Compilation compilation)
    {
        return compilation.Assembly
            .GetAttributes()
            .Any(x => x.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == _luaOpenAttributeFullName);
    }
}
