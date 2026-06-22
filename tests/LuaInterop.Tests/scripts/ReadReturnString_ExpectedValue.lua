-- Arrange
local interop = require("luainteropdemo")
local arg = "Hello, World!"

-- Act
local result = interop.readReturnString(arg)

-- Assert
assert(type(result) == "string")
assert(result == arg)
