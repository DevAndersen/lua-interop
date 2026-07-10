namespace LuaInterop.Attributes;

/// <summary>
/// Enables generation of Lua interop code on the decorated assembly.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public class LuaLibraryAttribute : Attribute
{
}
