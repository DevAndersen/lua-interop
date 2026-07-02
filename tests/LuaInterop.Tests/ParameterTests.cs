using Xunit;

namespace LuaInterop.Tests;

// Todo: Test source generator itself. Ensure that diagnostics are reported correctly (e.g. nested types).
// Todo: Test reading and writing strings that contains null characters.
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
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("?!$#([{}]})='.;--`")]
    [InlineData(@"abc\0def")]
    public async Task ReadWriteString_ReturnsExpectedValue(string? value)
    {
        string str = $"""
            -- Act
            print({ToLua(value)})
            """;

        // Act
        LuaHelper.ProcessResult result = await LuaHelper.RunLuaScriptResultAsync($"""
            -- Act
            local result = interop.ReadStringWithNullCharacter({ToLua(value)})

            -- Assert
            assert(type(result) == "string")
            print(result)
            """);

        // Assert
        Assert.True(result.IsSuccessful, result.StandardError);
        Assert.Equal("abc\0def", result.StandardOutput.Trim(Environment.NewLine));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReadStringWithNullCharacter_ReturnsStringWithNullCharacter(bool value)
    {
        // Act
        LuaHelper.ProcessResult result = await LuaHelper.RunLuaScriptResultAsync($"""
            -- Act
            local result = interop.ReadStringWithNullCharacter({ToLua(value)})

            -- Assert
            assert(type(result) == "string")
            print(result)
            """);

        // Assert
        Assert.True(result.IsSuccessful, result.StandardError);
        Assert.Equal("abc\0def", result.StandardOutput.Trim(Environment.NewLine));
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
            null => "nil",
            bool b => b.ToString().ToLower(),
            string s => $"\"{s}\"",
            byte or short or int or long => $"{value}",
            float or double => $"{value.ToString()?.Replace(',', '.')}",
            _ => throw new Exception($"No mapping defined for {typeof(T).FullName}")
        };
    }
}
