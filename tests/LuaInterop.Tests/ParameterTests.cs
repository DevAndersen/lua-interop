using Xunit;

namespace LuaInterop.Tests;

// Todo: Test source generator itself. Ensure that diagnostics are reported correctly (e.g. nested types).
public class ParameterTests
{
    private const string _luaBoolean = "boolean";
    private const string _luaNil = "nil";

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 2, 3)]
    [InlineData(2, 1, 3)]
    [InlineData(1000, -500, 500)]
    public async Task ValidParameters_ValidInput_ReturnsExpectedValue(int a, int b, int result)
    {
        // Act
        string str = await LuaHelper.RunScriptAsync($"""
            -- Act
            local result = interop.Addition({a}, {b})

            -- Assert
            assert(math.type(result) == "integer")
            print(result)
            """);

        // Assert
        Assert.Equal(result.ToString(), str.Trim(Environment.NewLine));
    }

    [Theory]
    [InlineData] // No parameters
    [InlineData(1)] // Too few parameters
    [InlineData(1, 2, 3)] // Too many parameters
    public async Task Parameters_IncorrectParameterQuantity_Fails(params int[] parameters)
    {
        // Act
        LuaHelper.ProcessResult result = await LuaHelper.RunLuaScriptResultAsync($"""
            -- Act
            local result = interop.Addition({string.Join(", ", parameters)})

            -- Assert
            assert(math.type(result) == "integer")
            print(result)
            """);

        // Assert
        Assert.False(result.IsSuccessful);
    }

    [Theory]
    [InlineData(0, 3_000_000_000)]
    [InlineData(0, -3_000_000_000)]
    public async Task Int32Parameters_Overflow_Fails(long a, long b)
    {
        // Act
        LuaHelper.ProcessResult result = await LuaHelper.RunLuaScriptResultAsync($"""
            -- Act
            local result = interop.Addition({a}, {b})

            -- Assert
            assert(math.type(result) == "integer")
            print(result)
            """);

        // Assert
        Assert.False(result.IsSuccessful);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReadWriteBoolean_ReturnsExpectedValue(bool value)
    {
        // Act
        LuaHelper.ProcessResult result = await LuaHelper.RunLuaScriptResultAsync($"""
            -- Act
            local result = interop.ReadWriteBoolean({ToLua(value)})

            -- Assert
            assert(type(result) == "boolean")
            print(result)
            """);

        // Assert
        Assert.True(result.IsSuccessful, result.StandardError);
        Assert.Equal(value, bool.Parse(result.StandardOutput.Trim(Environment.NewLine)));
    }

    [Theory]
    [InlineData(false, _luaBoolean)]
    [InlineData(true, _luaBoolean)]
    [InlineData(null, _luaNil)]
    public async Task ReadWriteNullableBoolean_ReturnsExpectedValue(bool? value, string expectedType)
    {
        // Act
        LuaHelper.ProcessResult result = await LuaHelper.RunLuaScriptResultAsync($"""
            -- Act
            local result = interop.ReadWriteNullableBoolean({ToLua(value)})

            -- Assert
            assert(type(result) == "{expectedType}")
            print(result)
            """);

        // Assert
        Assert.True(result.IsSuccessful, result.StandardError);

        string trimmed = new string(result.StandardOutput.Trim(Environment.NewLine));
        Assert.Equal(value, trimmed == _luaNil ? null : bool.Parse(trimmed));
    }

    private static string ToLua<T>(T value)
    {
        return value switch
        {
            bool b => b.ToString().ToLower(),
            null => "nil",
            _ => throw new Exception($"No mapping defined for {typeof(T).FullName}")
        };
    }
}
