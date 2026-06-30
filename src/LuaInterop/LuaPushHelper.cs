using LuaInterop.Native;

namespace LuaInterop;

public static class LuaPushHelper
{
    public static int PushString(nint luaState, string? value)
    {
        if (value == null)
        {
            Lua.PushNil(luaState);
        }
        else
        {
            Lua.PushString(luaState, value);
        }

        return 1;
    }

    public static int PushLong(nint luaState, long? value)
    {
        PushNullable(luaState, value, Lua.PushInteger);
        return 1;
    }

    public static int PushInt(nint luaState, int? value)
    {
        PushLong(luaState, value);
        return 1;
    }

    public static int PushShort(nint luaState, short? value)
    {
        PushLong(luaState, value);
        return 1;
    }

    public static int PushByte(nint luaState, byte? value)
    {
        PushLong(luaState, value);
        return 1;
    }

    public static int PushDouble(nint luaState, double? value)
    {
        PushNullable(luaState, value, Lua.PushNumber);
        return 1;
    }

    public static int PushFloat(nint luaState, float? value)
    {
        PushFloat(luaState, value);
        return 1;
    }

    public static int PushBoolean(nint luaState, bool? value)
    {
        PushNullable(luaState, value, Lua.PushBoolean);
        return 1;
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
