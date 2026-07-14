#!/bin/sh

# Attempt to resolve the runtime identifier.
RID=$(dotnet msbuild ../LuaInterop.Tests.Demo -getProperty:NETCoreSdkPortableRuntimeIdentifier)
OUTPUT_PATH=$1
LUA_VERSION=$2

echo "Building native library to '$OUTPUT_PATH' for Lua $LUA_VERSION for '$RID'"

# Ensure that the runtime identifier was resolved.
if [ -z "$RID" ]; then
  echo "Failed to resolve runtime identifier"
  exit 1
fi

# Ensure that an output directory was specified.
if [ -z "$OUTPUT_PATH" ]; then
  echo "Output directory not specified"
  exit 1
fi

# Ensure that a Lua version was specified.
if [ -z "$LUA_VERSION" ]; then
  echo "Lua version not specified"
  exit 1
fi

dotnet publish ../LuaInterop.Tests.Demo -r $RID -c Release -o $OUTPUT_PATH -p:LuaVersion=$LUA_VERSION
