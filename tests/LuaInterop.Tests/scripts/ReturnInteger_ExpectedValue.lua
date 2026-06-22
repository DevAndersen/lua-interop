-- Arrange
local interop = require("luainteropdemo")

-- Act
local result = interop.returnInteger()

-- Assert
assert(type(result) == "integer")
assert(result == 42)
