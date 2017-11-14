@echo off
echo Press any key to publish
pause
".nuget\NuGet.exe" push Rssdp.3.5.8.nupkg -Source https://www.nuget.org/api/v2/package
pause