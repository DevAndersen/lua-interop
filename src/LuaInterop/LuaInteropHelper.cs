using LuaInterop.Native;
using System.Text;

namespace LuaInterop;

public static class LuaInteropHelper
{
    public static unsafe void RegisterFunction(nint luaStatePtr, string functionName, delegate* unmanaged<nint, int> functionPointer)
    {
        const int stackIndex = -2;

        Lua.PushCClosure(luaStatePtr, (nint)functionPointer, 0);
        Lua.SetField(luaStatePtr, stackIndex, functionName);
    }

    public static unsafe string ReadStringArg(nint luaStatePtr, int argumentIndex)
    {
        byte* ptr = Lua.CheckLString(luaStatePtr, argumentIndex, out nuint length);

        if (ptr == null)
        {
            throw new Exception(); // Todo: Throw an appropriate exception with message.
        }

        Span<byte> bytes = new Span<byte>(ptr, (int)length);
        return Encoding.UTF8.GetString(bytes);
    }

    public static long ReadIntegerArg(nint luaStatePtr, int argumentIndex)
    {
        return Lua.CheckInteger(luaStatePtr, argumentIndex);
    }

    public static double ReadNumberArg(nint luaStatePtr, int argumentIndex)
    {
        return Lua.CheckNumber(luaStatePtr, argumentIndex);
    }
}
