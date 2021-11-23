# check environment
if (!$env:MongoDBConnectionString){
    throw "Missing MongoDBConnectionString Environment Variable"
}

if (!$env:MongoUsername){
    throw "Missing MongoUsername Environment Variable"
}

if (!$env:MongoPassword){
    throw "Missing MongoPassword Environment Variable"
}

if (!$env:CookieKey){
    throw "Missing CookieKey Environment Variable"
}

# prebuild
Write-Host Running .\testbuild.ps1
$currentDirectory = (Get-Location).Path
$repository = Split-Path -Path $MyInvocation.MyCommand.Path -Parent
Set-Location -Path $repository
mongosh $env:MongoDBConnectionString -f database\scripts\update.js

# build
$env:Database = "test"
dotnet build src/PTABackend.sln
mongosh $env:MongoDBConnectionString -f  .\database\scripts\update.js
dotnet test .\src\PTABackEnd.sln --logger:"trx;LogFileName=C:\Users\zachagrey\.jenkins\workspace\PTA backend develop build/TestOutput.trx" --filter:Category=smoke

# postbuild
mongosh $env:MongoDBConnectionString -f .\database\scripts\catalog_logs.js
Set-Location -Path $currentDirectory