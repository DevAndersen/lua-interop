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
            Lua.PushLString(luaState, value, (nuint)value.Length);
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

    public static int PushNull(nint luaState)
    {
        Lua.PushNil(luaState);
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

    public static int PushDictionary<TKey, TValue>(nint luaState, IDictionary<TKey, TValue> dictionary)
    {
        // Todo: Validate key- and value type.

        LuaInteropHelper.CreateTable(luaState, 0);

        foreach ((TKey key, TValue value) in dictionary)
        {
            int tableStackPosition = -1;
            tableStackPosition -= PushTableValue(luaState, key);
            tableStackPosition -= PushTableValue(luaState, value);
            Lua.RawSet(luaState, tableStackPosition);
        }

        return 1;
    }

    private static int PushTableValue<T>(nint luaState, T value)
    {
        return value switch
        {
            null => PushNull(luaState),
            byte v => PushByte(luaState, v),
            short v => PushShort(luaState, v),
            int v => PushInt(luaState, v),
            long v => PushLong(luaState, v),
            float v => PushFloat(luaState, v),
            double v => PushDouble(luaState, v),
            bool v => PushBoolean(luaState, v),
            string v => PushString(luaState, v),
            _ => throw new Exception(), // Todo: Throw an appropriate exception with a message.
        };
    }
}
