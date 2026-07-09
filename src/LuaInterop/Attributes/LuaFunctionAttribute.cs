namespace LuaInterop.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class LuaFunctionAttribute : Attribute
{
    public string? FunctionName { get; init; }

    public bool ManualFunction { get; init; }

    public LuaFunctionAttribute()
    {
    }

    public LuaFunctionAttribute(string functionName)
    {
        FunctionName = functionName;
    }
}
