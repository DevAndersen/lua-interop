using LuaInterop.Attributes;

namespace LuaInterop.Tests.Demo;

internal class TestMethods
{
    [LuaFunction]
    public static int Addition(int a, int b)
    {
        return a + b;
    }

    [LuaFunction]
    public static bool ReadWriteBoolean(bool value)
    {
        return value;
    }

    [LuaFunction]
    public static bool? ReadWriteNullableBoolean(bool? value)
    {
        return value;
    }

    [LuaFunction]
    public static string ReadWriteString(string value)
    {
        return value;
    }

    [LuaFunction]
    public static string? ReadWriteNullableString(string? value)
    {
        return value;
    }

    [LuaFunction]
    public static string ReadStringWithNullCharacter()
    {
        return "abc\0def";
    }

    [LuaFunction]
    public static Dictionary<int, string> WriteDictionary()
    {
        return new Dictionary<int, string>
        {
            [1] = "A",
            [2] = "B",
            [3] = "C",
        };
    }
}
