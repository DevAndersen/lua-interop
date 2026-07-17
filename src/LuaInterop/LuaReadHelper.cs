using System.Text;

namespace LuaInterop;

public static class LuaReadHelper
{
    public static string? ReadNullableString(nint luaStatePtr, int argumentIndex, string parameterName)
    {
        return ReadNullableReferenceType(luaStatePtr, argumentIndex, parameterName, ReadString);
    }

    // Todo: Version of "ReadString" that supports null, maybe a class version of "ReadNullable"?
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
        _ = parameterName; // Todo: Type check, throw exception with parameter name if bad data.

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
        _ = parameterName; // Todo: Type check, throw exception with parameter name if bad data.

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
        _ = parameterName; // Todo: Unnecessary parameter, but makes source generation easier.

        return ReadNullableValueType(luaStatePtr, argumentIndex, Lua.ToBoolean);
    }

    public static int GetTop(nint luaStatePtr)
    {
        return Lua.GetTop(luaStatePtr);
    }

    public static void ThrowIfUnexpectedParameterCount(nint luaStatePtr, int expectedParameterCount, string luaFunctionName)
    {
        int actualParameterCount = GetTop(luaStatePtr);
        if (actualParameterCount != expectedParameterCount)
        {
            throw new ArgumentException($"Incorrect number of parameters passed to Lua function '{luaFunctionName}', expected {expectedParameterCount} got {actualParameterCount}.");
        }
    }

    /// <summary>
    /// Reads the value type argument at <paramref name="argumentIndex"/>, with support for reading <c>null</c>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="luaStatePtr"></param>
    /// <param name="argumentIndex"></param>
    /// <param name="nonNullableFunc"></param>
    /// <returns></returns>
    private static T? ReadNullableValueType<T>(nint luaStatePtr, int argumentIndex, Func<nint, int, T> nonNullableFunc) where T : struct
    {
        if (Lua.Type(luaStatePtr, argumentIndex) == LuaType.LUA_TNIL)
        {
            return null;
        }

        return nonNullableFunc(luaStatePtr, argumentIndex);
    }

    /// <summary>
    /// Reads the reference type argument at <paramref name="argumentIndex"/>, with support for reading <c>null</c>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="luaStatePtr"></param>
    /// <param name="argumentIndex"></param>
    /// <param name="parameterName"></param>
    /// <param name="nonNullableFunc"></param>
    /// <returns></returns>
    private static T? ReadNullableReferenceType<T>(nint luaStatePtr, int argumentIndex, string parameterName, Func<nint, int, string, T> nonNullableFunc) where T : class
    {
        if (Lua.Type(luaStatePtr, argumentIndex) == LuaType.LUA_TNIL)
        {
            return null;
        }

        return nonNullableFunc(luaStatePtr, argumentIndex, parameterName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the type of the parameter at <paramref name="argumentIndex"/> is not <paramref name="expectedType"/>.
    /// </summary>
    /// <param name="luaStatePtr"></param>
    /// <param name="argumentIndex"></param>
    /// <param name="parameterName"></param>
    /// <param name="expectedType"></param>
    /// <exception cref="ArgumentException"></exception>
    private static void ThrowIfNotExpectedType(nint luaStatePtr, int argumentIndex, string parameterName, LuaType expectedType)
    {
        LuaType actualType = Lua.Type(luaStatePtr, argumentIndex);
        if (actualType != expectedType)
        {
            throw new ArgumentException($"Parameter '{parameterName}' was unexpected type '{actualType}', expected '{expectedType}'.");
        }
    }
}
