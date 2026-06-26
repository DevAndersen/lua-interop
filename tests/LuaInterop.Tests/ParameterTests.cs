using Xunit;

namespace LuaInterop.Tests;

public class ParameterTests
{
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
}
