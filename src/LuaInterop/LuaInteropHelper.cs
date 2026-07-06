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

    /// <summary>
    /// Push a copy of the stack value at <paramref name="stackIndex"/> to the top of the stack.
    /// </summary>
    /// <param name="luaStatePtr"></param>
    /// <param name="stackIndex"></param>
    public static void PushValue(nint luaStatePtr, int stackIndex)
    {
        Lua.PushValue(luaStatePtr, stackIndex);
    }

    /// <summary>
    /// Call a passed Lua function.
    /// </summary>
    /// <remarks>
    /// The top of the stack must be the address of the function, followed by its arguments.
    /// </remarks>
    /// <param name="luaStatePtr"></param>
    /// <param name="argumentCount"></param>
    /// <param name="resultCount"></param>
    /// <returns></returns>
    public static int CallFunction(nint luaStatePtr, int argumentCount, int resultCount)
    {
        return (int)Lua.PCallK(luaStatePtr, argumentCount, resultCount, 0, 0, 0);
    }
}
