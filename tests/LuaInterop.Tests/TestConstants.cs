using LuaInterop.SourceGen;

namespace LuaInterop.Tests;

internal static class TestConstants
{
    public const string TestAssemblyName = nameof(TestAssemblyName);

    public const string LuaEntryPointClassName = $"{GeneratorConstants.GeneratedCodeNamespace}.{GeneratorConstants.LuaOpenClassName}_{TestAssemblyName}";
}
