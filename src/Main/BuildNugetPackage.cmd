copy ..\..\LICENSE.md lib
copy ..\..\README.md lib
del /F /Q /S *.CodeAnalysisLog.xml

"..\.nuget\NuGet.exe" pack -sym Rssdp.nuspec -BasePath .\
pause