namespace LuaInterop.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ManualLuaFunctionAttribute : Attribute
{
    public string? FunctionName { get; set; }
}
