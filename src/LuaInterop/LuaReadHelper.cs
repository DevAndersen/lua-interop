using LuaInterop.Native;
using System.Text;

namespace LuaInterop;

public static class LuaReadHelper
{
    public static unsafe string ReadString(nint luaStatePtr, int argumentIndex, string parameterName)
    {
        byte* ptr = Lua.CheckLString(luaStatePtr, argumentIndex, out nuint length);

        if (ptr == null)
        {
            throw new ArgumentNullException(parameterName, $"Lua string argument '{parameterName}' was null");
        }

        Span<byte> bytes = new Span<byte>(ptr, (int)length);
        return Encoding.UTF8.GetString(bytes);
    }

    public static long ReadLong(nint luaStatePtr, int argumentIndex, string parameterName)
    {
        return Lua.CheckInteger(luaStatePtr, argumentIndex);
    }

    public static int ReadInt(nint luaStatePtr, int argumentIndex, string parameterName)
    {
        return checked((int)ReadLong(luaStatePtr, argumentIndex, parameterName));
    }

    public static short ReadShort(nint luaStatePtr, int argumentIndex, string parameterName)
    {
        return checked((short)ReadLong(luaStatePtr, argumentIndex, parameterName));
    }

    public static byte ReadByte(nint luaStatePtr, int argumentIndex, string parameterName)
    {
        return checked((byte)ReadLong(luaStatePtr, argumentIndex, parameterName));
    }

    public static double ReadDouble(nint luaStatePtr, int argumentIndex, string parameterName)
    {
        return Lua.CheckNumber(luaStatePtr, argumentIndex);
    }

    public static float ReadFloat(nint luaStatePtr, int argumentIndex, string parameterName)
    {
        return checked((float)ReadDouble(luaStatePtr, argumentIndex, parameterName));
    }

    public static bool ReadBoolean(nint luaStatePtr, int argumentIndex, string parameterName)
    {
        ThrowIfNotExpectedType(luaStatePtr, argumentIndex, parameterName, LuaType.LUA_TBOOLEAN);
        return Lua.ToBoolean(luaStatePtr, argumentIndex);
    }

    public static bool? ReadNullableBoolean(nint luaStatePtr, int argumentIndex, string parameterName)
    {
        if (Lua.Type(luaStatePtr, argumentIndex) == LuaType.LUA_TNIL)
        {
            return null;
        }

        return Lua.ToBoolean(luaStatePtr, argumentIndex);
    }

    public static int GetTop(nint luaStatePtr)
    {
        return Lua.GetTop(luaStatePtr);
    }

    private static void ThrowIfNotExpectedType(nint luaStatePtr, int argumentIndex, string parameterName, LuaType expectedType)
    {
        LuaType actualType = Lua.Type(luaStatePtr, argumentIndex);
        if (actualType != expectedType)
        {
            throw new ArgumentException($"Parameter '{parameterName}' was unexpected type '{actualType}', expected '{expectedType}'.");
        }
    }
}
