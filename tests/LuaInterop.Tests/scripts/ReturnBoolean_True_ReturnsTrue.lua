-- Arrange
local interop = require("luainteropdemo")

-- Act
local result = interop.returnBooleanTrue()

-- Assert
assert(type(result) == "boolean")
assert(result == true)
