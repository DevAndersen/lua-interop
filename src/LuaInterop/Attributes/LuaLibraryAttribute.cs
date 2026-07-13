using System.Runtime.CompilerServices;

namespace LuaInterop.Attributes;

/// <summary>
/// Enables generation of Lua interop code on the decorated assembly.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class LuaLibraryAttribute : Attribute
{
    /// <summary>
    /// Determines if the module initializer class gets automatically generated.
    /// If set to <c>false</c>, code must be written so that a method decorated with
    /// <see cref="ModuleInitializerAttribute"/> calls <see cref="LuaModuleInitializer.Initialize"/>.
    /// </summary>
    public bool GenerateModuleInitializerClass { get; init; } = true;
}
