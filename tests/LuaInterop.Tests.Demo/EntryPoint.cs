using LuaInterop.Attributes;

//[assembly: LuaLibrary]

namespace LuaInterop.Tests.Demo;

public static class EntryPoint
{
    // For navigating to source generated code.
    //private const string _generatedClassName = nameof(LuaInterop.Generated.LuaEntryPoint);

    [System.Runtime.InteropServices.UnmanagedCallersOnly(EntryPoint = "luaopen_luainteropdemo")]
    public static unsafe int LuaOpen(nint luaState)
    {
        LuaInteropHelper.CreateTable(luaState, 1);
        LuaInteropHelper.RegisterFunction(luaState, "CallbackTest", &CallbackTest);
        return 1;
    }

    [System.Runtime.InteropServices.UnmanagedCallersOnly] // Todo: Allow registering non-generated methods.
    public static int CallbackTest(nint luaState)
    {
        // Read integer input (argument 1).
        int arg = LuaReadHelper.ReadInt(luaState, 1, "");

        // Push function to top (argument 2).
        LuaInteropHelper.PushValue(luaState, 2);

        // Push function argument.
        int argumentCount = LuaPushHelper.PushInt(luaState, arg * 2);

        // Call function.
        LuaInteropHelper.CallFunction(luaState, argumentCount, 1);

        // Read function return value.
        int result = LuaReadHelper.ReadInt(luaState, -1, "");
        Console.WriteLine($"Callback returned: {result}");

        return 0;
    }

    // Test functions

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
