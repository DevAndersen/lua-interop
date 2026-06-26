using LuaInterop.Native;

namespace LuaInterop;

public static class LuaPushHelper
{
    public static void PushString(nint luaState, string? value)
    {
        if (value == null)
        {
            Lua.PushNil(luaState);
        }
        else
        {
            Lua.PushString(luaState, value);
        }
    }

    public static void PushLong(nint luaState, long? value)
    {
        PushNullable(luaState, value, Lua.PushInteger);
    }

    public static void PushInt(nint luaState, int? value)
    {
        PushLong(luaState, value);
    }

    public static void PushShort(nint luaState, short? value)
    {
        PushLong(luaState, value);
    }

    public static void PushByte(nint luaState, byte? value)
    {
        PushLong(luaState, value);
    }

    public static void PushDouble(nint luaState, double? value)
    {
        PushNullable(luaState, value, Lua.PushNumber);
    }

    public static void PushFloat(nint luaState, float? value)
    {
        PushFloat(luaState, value);
    }

    public static void PushBoolean(nint luaState, bool? value)
    {
        PushNullable(luaState, value, Lua.PushBoolean);
    }

    private static void PushNullable<T>(nint luaState, T? value, Action<nint, T> func) where T : struct
    {
        if (value == null)
        {
            Lua.PushNil(luaState);
        }
        else
        {
            func(luaState, value.Value);
        }
    }
}
