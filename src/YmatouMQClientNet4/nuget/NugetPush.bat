@echo on
echo 'start pack'
nuget.exe pack ..\YmatouMQ.ClientNet4.csproj
echo 'start upload'
NuGetPackageUploader .