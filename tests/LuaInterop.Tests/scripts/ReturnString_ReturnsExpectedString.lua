-- Arrange
local interop = require("luainteropdemo")

-- Act
local result = interop.returnString()

-- Assert
assert(type(result) == "string")
assert(result == "Hello, World!")
