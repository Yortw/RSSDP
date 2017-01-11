@echo off
echo Press any key to publish
pause
".nuget\NuGet.exe" push Rssdp.2.0.9.nupkg
pause