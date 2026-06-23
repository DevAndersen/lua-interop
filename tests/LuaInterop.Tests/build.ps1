param (
    [Parameter(Mandatory=$true)]
    [string]$OutputPath
)

dotnet publish ../LuaInterop.Tests.Demo -r win-x64 -c Release -o $OutputPath
