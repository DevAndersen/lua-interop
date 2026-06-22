-- Arrange
local interop = require("luainteropdemo")

-- Act
local result = interop.returnBooleanFalse()

-- Assert
assert(type(result) == "boolean")
assert(result == false)
