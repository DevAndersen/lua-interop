-- Arrange
local interop = require("luainteropdemo")

-- Act
local result = interop.returnNull()

-- Assert
assert(type(result) == "nil")
assert(result == nil)
