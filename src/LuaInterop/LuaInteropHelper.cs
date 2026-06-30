using LuaInterop.Native;

namespace LuaInterop;

public static class LuaInteropHelper
{
    public static void CreateTable(nint luaStatePtr, int count)
    {
        Lua.CreateTable(luaStatePtr, 0, count);
    }

    public static unsafe void RegisterFunction(nint luaStatePtr, string functionName, delegate* unmanaged<nint, int> functionPointer)
    {
        const int stackIndex = -2;

        Lua.PushCClosure(luaStatePtr, (nint)functionPointer, 0);
        Lua.SetField(luaStatePtr, stackIndex, functionName);
    }
}
