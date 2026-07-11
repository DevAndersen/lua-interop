#!/bin/sh

# Attempt to resolve the runtime identifier.
RID=$(dotnet msbuild ../LuaInterop.Tests.Demo -getProperty:NETCoreSdkPortableRuntimeIdentifier)

echo "Building native library to '$1' for '$RID'"

# Ensure that an output directory was specified.
if [ -z "$1" ]; then
  echo "Output directory not specified"
  exit 1
fi

# Ensure that the runtime identifier was resolved.
if [ -z "$RID" ]; then
  echo "Failed to resolve runtime identifier"
  exit 1
fi

dotnet publish ../LuaInterop.Tests.Demo -r $RID -c Release -o $1
