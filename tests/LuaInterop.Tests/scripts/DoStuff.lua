-- Arrange
local interop = require("luainterop")

-- Act
local result = interop.sayMessage("Hello, World!")

-- Assert
assert(type(result) == "string")
