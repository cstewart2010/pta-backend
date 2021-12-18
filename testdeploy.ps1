param (
  [Parameter(Mandatory=$true)]
  [string]$WWW_ROOT,
  [Parameter(Mandatory=$true)]
  [string]$CORE_API
)

# validate environment
if (!(Test-Path $CORE_API)){
  Write-Host "Invalid location for pta backend source: $CORE_API"
  return
}

if (!(Test-Path $WWW_ROOT)){
  Write-Host "Invalid location for wwwroot: $WWW_ROOT"
  return
}

# stop iis
Write-Host "Stopping IIS"
net stop WAS /y

# deploy release config
Write-Host "Building Core Api"
Set-Location $CORE_API
dotnet publish -c Release -o $WWW_ROOT

# start iis
Write-Host "Restarting IIS"
net start W3SVC

$response = Invoke-WebRequest -Uri http://localhost/api/v1/pokedex/florges -Method Get
$response.Content | convertfrom-json | convertto-json -depth 100