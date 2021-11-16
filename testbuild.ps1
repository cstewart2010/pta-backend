# check environment
if (!$env:MongoDBConnectionString){
    throw "Missing MongoDBConnectionString"
}

if (!$env:MongoUsername){
    throw "Missing MongoDBConnectionString"
}

if (!$env:MongoPassword){
    throw "Missing MongoDBConnectionString"
}

# prebuild
Write-Host Running .\testbuild.ps1
$currentDirectory = (Get-Location).Path
$repository = Split-Path -Path $MyInvocation.MyCommand.Path -Parent
Set-Location -Path $repository
mongosh $env:MongoDBConnectionString -f database\scripts\update.js

# build
$env:MongoDBConnectionString = "mongodb+srv://$($env:MongoUsername):$($env:MongoPassword)@ptatestcluster.1ekcs.mongodb.net/test?retryWrites=true&w=majority"

dotnet build src/PTABackend.sln
mongosh $env:MongoDBConnectionString -f  .\database\scripts\update.js
dotnet test .\src\PTABackEnd.sln --logger:"trx;LogFileName=C:\Users\zachagrey\.jenkins\workspace\PTA backend develop build/TestOutput.trx" --filter:Category=smoke

# postbuild
mongosh $env:MongoDBConnectionString -f .\database\scripts\catalog_logs.js
Set-Location -Path $currentDirectory