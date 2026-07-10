param (
    [Parameter(Mandatory=$true)]
    [string]$OutputPath
)

Write-Host "Building native library to '$OutputPath'"
dotnet publish ..\LuaInterop.Tests.Demo -r win-x64 -c Release -o $OutputPath
