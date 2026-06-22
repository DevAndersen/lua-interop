-- Arrange
local interop = require("luainteropdemo")
local arg = 42

-- Act
local result = interop.readReturnInteger(arg)

-- Assert
assert(math.type(result) == "integer")
assert(result == arg)
