using System.Runtime.InteropServices;
using System.Text;
using LuaInterop.Native;

namespace LuaInterop.Tests.Demo;

public static unsafe class EntryPoint
{
    [UnmanagedCallersOnly(EntryPoint = "luaopen_luainteropdemo")] // Must match "luaopen_[ASSEMBLY NAME]", must seemingly be lower-case, can be set with <AssemblyName> in the .csproj file.
    public static int LuaOpen(nint luaState)
    {
        const int tableIndex = 1;

        Lua.CreateTable(luaState, 0, 4);
        
        RegisterFunction(luaState, "sayMessage", &SayMessage);
        RegisterFunction(luaState, "returnString", &ReturnString);
        RegisterFunction(luaState, "returnBooleanTrue", &ReturnBooleanTrue);
        RegisterFunction(luaState, "returnBooleanFalse", &ReturnBooleanFalse);

        return tableIndex;
    }

    private static void RegisterFunction(nint luaStatePtr, string functionName, delegate* unmanaged<nint, int> functionPointer)
    {
        const int stackIndex = -2;

        Lua.PushCClosure(luaStatePtr, (nint)functionPointer, 0);
        Lua.SetField(luaStatePtr, stackIndex, functionName);
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
}
