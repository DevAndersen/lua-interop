namespace LuaInterop.Attributes;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public class LuaOpenAttribute : Attribute
{
    public int Number { get; set; }
}
