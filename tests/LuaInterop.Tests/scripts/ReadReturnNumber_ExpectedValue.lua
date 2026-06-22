-- Arrange
local interop = require("luainteropdemo")
local arg = 123.456

-- Act
local result = interop.readReturnNumber(arg)

-- Assert
assert(type(result) == "number")
assert(result == arg)
