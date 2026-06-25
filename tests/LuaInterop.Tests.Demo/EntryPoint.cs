using System.Runtime.InteropServices;
using System.Text;
using LuaInterop.Attributes;
using LuaInterop.Native;

[assembly: LuaOpen(Number = 12)]

namespace LuaInterop.Tests.Demo;

public static unsafe class EntryPoint
{
    //[UnmanagedCallersOnly(EntryPoint = "luaopen_luainteropdemo")] // Must match "luaopen_[ASSEMBLY NAME]", must seemingly be lower-case, can be set with <AssemblyName> in the .csproj file.
    public static int LuaOpen(nint luaState)
    {
        global::Demo.Marker.luainteropdemo.Generated2.SayHello();

        _ = typeof(global::Abc.Def.DemoClass);

        const int tableIndex = 1;

        Lua.CreateTable(luaState, 0, 7);

        RegisterFunction(luaState, "sayMessage", &SayMessage);

        RegisterFunction(luaState, "returnString", &ReturnString);
        RegisterFunction(luaState, "returnBooleanTrue", &ReturnBooleanTrue);
        RegisterFunction(luaState, "returnBooleanFalse", &ReturnBooleanFalse);
        RegisterFunction(luaState, "returnInteger", &ReturnInteger);
        RegisterFunction(luaState, "returnNumber", &ReturnNumber);
        RegisterFunction(luaState, "returnNull", &ReturnNull);

        RegisterFunction(luaState, "readReturnString", &ReadReturnString);
        RegisterFunction(luaState, "readReturnInteger", &ReadReturnInteger);
        RegisterFunction(luaState, "readReturnNumber", &ReadReturnNumber);

        return tableIndex;
    }

    private static void RegisterFunction(nint luaStatePtr, string functionName, delegate* unmanaged<nint, int> functionPointer)
    {
        const int stackIndex = -2;

        Lua.PushCClosure(luaStatePtr, (nint)functionPointer, 0);
        Lua.SetField(luaStatePtr, stackIndex, functionName);
    }

    [LuaFunction]
    public static void DoWork1(string myNumber)
    {
        Console.WriteLine("Work 1 being done");
    }

    [LuaFunction(FunctionName = "customName")]
    public static void DoWork2()
    {
        Console.WriteLine("Work 2 being done");
    }

    [UnmanagedCallersOnly]
    private static int SayMessage(nint luaStatePtr)
    {
        // Read argument
        string arg1 = ReadStringArg(luaStatePtr, 1);

        string message = $"You said: {arg1}";

        // Push output string onto the stack.
        Lua.PushString(luaStatePtr, message);

        // Return the number of pushed values.
        return 1;
    }

    [UnmanagedCallersOnly]
    private static int ReturnString(nint luaStatePtr)
    {
        Lua.PushString(luaStatePtr, "Hello, World!");
        return 1;
    }

    [UnmanagedCallersOnly]
    private static int ReturnBooleanTrue(nint luaStatePtr)
    {
        Lua.PushBoolean(luaStatePtr, true);
        return 1;
    }

    [UnmanagedCallersOnly]
    private static int ReturnBooleanFalse(nint luaStatePtr)
    {
        Lua.PushBoolean(luaStatePtr, false);
        return 1;
    }

    [UnmanagedCallersOnly]
    private static int ReturnInteger(nint luaStatePtr)
    {
        Lua.PushInteger(luaStatePtr, 42);
        return 1;
    }

    [UnmanagedCallersOnly]
    private static int ReturnNumber(nint luaStatePtr)
    {
        Lua.PushNumber(luaStatePtr, 123.456);
        return 1;
    }

    [UnmanagedCallersOnly]
    private static int ReturnNull(nint luaStatePtr)
    {
        Lua.PushNil(luaStatePtr);
        return 1;
    }

    [UnmanagedCallersOnly]
    private static int ReadReturnString(nint luaStatePtr)
    {
        string arg = ReadStringArg(luaStatePtr, 1);
        Lua.PushString(luaStatePtr, arg);
        return 1;
    }

    [UnmanagedCallersOnly]
    private static int ReadReturnInteger(nint luaStatePtr)
    {
        long arg = ReadIntegerArg(luaStatePtr, 1);
        Lua.PushInteger(luaStatePtr, arg);
        return 1;
    }

    [UnmanagedCallersOnly]
    private static int ReadReturnNumber(nint luaStatePtr)
    {
        double arg = ReadNumberArg(luaStatePtr, 1);
        Lua.PushNumber(luaStatePtr, arg);
        return 1;
    }

    private static string ReadStringArg(nint luaStatePtr, int argumentIndex)
    {
        byte* ptr = Lua.CheckLString(luaStatePtr, argumentIndex, out nuint length);

        if (ptr == null)
        {
            throw new Exception(); // Todo: Throw an appropriate exception with message.
        }

        Span<byte> bytes = new Span<byte>(ptr, (int)length);
        return Encoding.UTF8.GetString(bytes);
    }

    private static long ReadIntegerArg(nint luaStatePtr, int argumentIndex)
    {
        return Lua.CheckInteger(luaStatePtr, argumentIndex);
    }

    private static double ReadNumberArg(nint luaStatePtr, int argumentIndex)
    {
        return Lua.CheckNumber(luaStatePtr, argumentIndex);
    }
}
