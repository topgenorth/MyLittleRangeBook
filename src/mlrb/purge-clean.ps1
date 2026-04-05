# First shut down any process that might be hanging on to files.
Get-Process msbuild, dotnet, vbcscompiler, devenv, rider64 -ErrorAction SilentlyContinue |  Stop-Process -Force -ErrorAction SilentlyContinue

# Kill all build artifacts + caches
Get-ChildItem . -include bin,obj -Recurse | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
dotnet nuget locals all --clear
#dotnet clean /clp:ErrorsOnly
dotnet clean
dotnet restore
dotnet build
