namespace LuaInterop.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class LuaFunctionAttribute : Attribute
{
    public string? FunctionName { get; set; }

    public bool ManualFunction { get; set; }
}
