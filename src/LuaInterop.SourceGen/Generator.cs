using Microsoft.CodeAnalysis;

namespace LuaInterop.SourceGen;

[Generator(LanguageNames.CSharp)]
internal class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("Test.g.cs", """
                namespace Demo;

                public static class Generated
                {
                    public static void SayHello()
                    {
                        global::System.Console.WriteLine("Hello, World!");
                    }
                }
                """);
        });
    }
}
