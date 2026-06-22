-- Arrange
local interop = require("luainteropdemo")

-- Act
local result = interop.returnInteger()

-- Assert
assert(math.type(result) == "integer")
assert(result == 42)
