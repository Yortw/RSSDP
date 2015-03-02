@echo off
echo Press any key to publish
pause
".nuget\NuGet.exe" push Rssdp.1.0.0.1.nupkg
pause