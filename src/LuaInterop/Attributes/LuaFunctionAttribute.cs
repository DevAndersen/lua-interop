namespace LuaInterop.Attributes;

/// <summary>
/// Marks this method as a Lua function, generating interop code so the method can be called from Lua.
/// </summary>
/// <remarks>
/// The assembly must also be decorated with <see cref="LuaLibraryAttribute"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class LuaFunctionAttribute : Attribute
{
    /// <summary>
    /// Set a custom name for the Lua function.
    /// If not specified, the method's name will be used.
    /// </summary>
    public string? FunctionName { get; init; }

    /// <summary>
    /// Marks the method as a manual interop method.
    /// This opts out of source generated wrapper methods, while still exposing the method to Lua.
    /// This method therefore has to fulfill the requirements for Lua interop,
    /// and manually invoke the relevant interop helper methods to read/write data to and from Lua.
    /// </summary>
    public bool ManualFunction { get; init; }

    /// <summary>
    /// Mark the method as a Lua function, exposing it to Lua as the name of the method.
    /// </summary>
    public LuaFunctionAttribute()
    {
    }

    /// <summary>
    /// Mark the method as a Lua function, exposing it to Lua as <paramref name="functionName"/>.
    /// </summary>
    /// <param name="functionName"></param>
    public LuaFunctionAttribute(string functionName)
    {
        FunctionName = functionName;
    }
}
