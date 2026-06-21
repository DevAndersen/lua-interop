using System.Runtime.InteropServices;
using LuaInterop.Native;

namespace LuaInterop;

public static class EntryPoint
{
    [UnmanagedCallersOnly(EntryPoint = "luaopen_dotnetinterop")]
    public static int LuaOpen(nint luaState)
    {
        Lua.lua_createtable(luaState, 0, 1);
        //RegisterFunction(luaState, "sayMessage", &SayMessage);
        return 1;
    }

    [UnmanagedCallersOnly]
    private static int SayMessage(nint luaStatePtr)
    {
        // Read argument
        string arg1 = ReadStringArg(luaStatePtr, 1);

        string message = $"You said: {arg1}";

        // Push output string onto the stack.
        Lua.lua_pushstring(luaStatePtr, message);

        // Return the number of pushed values.
        return 1;
    }

    private static string ReadStringArg(nint luaStatePtr, int argumentIndex)
    {
        nint argumentPtr = Lua.luaL_checklstring(luaStatePtr, argumentIndex, out nuint length);
        return Marshal.PtrToStringUTF8(argumentPtr, (int)length);
    }
}
