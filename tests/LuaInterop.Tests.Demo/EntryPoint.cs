using LuaInterop.Attributes;

[assembly: LuaOpen]

namespace LuaInterop.Tests.Demo;

public static class EntryPoint
{
    // For navigating to source generated code.
    private const string _generatedClassName = nameof(LuaInterop.Generated.LuaEntryPoint);

    [LuaFunction]
    public static void DoWork1(string text)
    {
        Console.WriteLine($"Work 1 being done, the text is \"{text}\"");
    }

    [LuaFunction(FunctionName = "customName")]
    public static void DoWork2()
    {
        Console.WriteLine("Work 2 being done");
    }

    [LuaFunction]
    public static int DoWork3()
    {
        Console.WriteLine("Work 3 being done");
        return 4;
    }

    [LuaFunction]
    public static int DoWork4(string text)
    {
        Console.WriteLine($"Work 4 being done, the text is \"{text}\"");
        return 4;
    }

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
}
