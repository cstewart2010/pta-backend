param (
    [Parameter(Mandatory=$true)]
    [int]$BUILD_NUMBER
)

# validate environment
$destination = "$env:SOURCE_LOCATION/src/TheReplacement.PTA.Services.Core"
if (!(Test-Path $destination)){
  Write-Host "Invalid location for pta backend source: $env:SOURCE_LOCATION"
  return
}

# stop iis
Write-Host "Stopping IIS"
net stop WAS /y

# deploy release config
Write-Host "Building Core Api"
Set-Location $destination
dotnet publish -c Release -o C:\PtaApi -p:Version="0.1.$BUILD_NUMBER.$BUILD_NUMBER"

# start iis
Write-Host "Restarting IIS"
net start W3SVC

$response = Invoke-WebRequest -Uri http://localhost/api/v1/pokedex/florges -Method Get
$response.Content | convertfrom-json | convertto-json -depth 100