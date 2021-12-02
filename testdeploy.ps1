# stop iis
Write-Host "Stopping IIS"
net stop WAS /y

# deploy release config
Write-Host "Building Core Api"
Set-Location src/TheReplacement.PTA.Services.Core
dotnet publish -c Release -o C:\PtaApi

# start iis
Write-Host "Restarting IIS"
net start W3SVC