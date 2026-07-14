param (
    [Parameter(Mandatory=$true)]
    [string]$OutputPath,

    [Parameter(Mandatory=$true)]
    [string]$LuaVersion
)

Write-Host "Building native library to '$OutputPath' for Lua $LuaVersion"
dotnet publish $PSScriptRoot\..\..\LuaInterop.Tests.Demo -r win-x64 -c Release -o $OutputPath -p:LuaVersion=$LuaVersion
