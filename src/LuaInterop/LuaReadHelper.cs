using LuaInterop.Native;
using System.Text;

namespace LuaInterop;

public static class LuaReadHelper
{
    public static unsafe string ReadString(nint luaStatePtr, int argumentIndex)
    {
        byte* ptr = Lua.CheckLString(luaStatePtr, argumentIndex, out nuint length);

        if (ptr == null)
        {
            throw new Exception(); // Todo: Throw an appropriate exception with message.
        }

        Span<byte> bytes = new Span<byte>(ptr, (int)length);
        return Encoding.UTF8.GetString(bytes);
    }

    public static long ReadLong(nint luaStatePtr, int argumentIndex)
    {
        return Lua.CheckInteger(luaStatePtr, argumentIndex);
    }

    public static int ReadInt(nint luaStatePtr, int argumentIndex)
    {
        return checked((int)ReadLong(luaStatePtr, argumentIndex));
    }

    public static short ReadShort(nint luaStatePtr, int argumentIndex)
    {
        return checked((short)ReadLong(luaStatePtr, argumentIndex));
    }

    public static byte ReadByte(nint luaStatePtr, int argumentIndex)
    {
        return checked((byte)ReadLong(luaStatePtr, argumentIndex));
    }

    public static double ReadDouble(nint luaStatePtr, int argumentIndex)
    {
        return Lua.CheckNumber(luaStatePtr, argumentIndex);
    }

    public static float ReadFloat(nint luaStatePtr, int argumentIndex)
    {
        return checked((float)ReadDouble(luaStatePtr, argumentIndex));
    }

    public static bool ReadBoolean(nint luaStatePtr, int argumentIndex)
    {
        return Lua.ToBoolean(luaStatePtr, argumentIndex);
    }

    public static bool? ReadNullableBoolean(nint luaStatePtr, int argumentIndex)
    {
        if (Lua.Type(luaStatePtr, argumentIndex) == LuaType.LUA_TNIL)
        {
            return null;
        }

        return Lua.ToBoolean(luaStatePtr, argumentIndex);
    }
}
